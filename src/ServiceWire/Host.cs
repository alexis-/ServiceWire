namespace ServiceWire
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Reflection;
  using System.Threading.Tasks;
  using ZeroKnowledge;

  public abstract class Host : IDisposable
  {
    #region Properties & Fields - Non-Public

    protected readonly ISerializer             _serializer;
    protected readonly ParameterTransferHelper _parameterTransferHelper;

    private            bool          _disposed = false;
    protected volatile bool          _isOpen;
    protected volatile bool          _continueListening    = true;
    protected          bool          _useCompression       = false;      // default is false
    protected          int           _compressionThreshold = 128 * 1024; // 128KB
    protected          ILog          _log                  = new NullLogger();
    protected          IStats        _stats                = new NullStats();
    protected          IZkRepository _zkRepository         = new ZkNullRepository();
    private volatile   bool          _requireZk            = false;

    protected ConcurrentDictionary<string, int>          _serviceKeys = new ConcurrentDictionary<string, int>();
    protected ConcurrentDictionary<int, ServiceInstance> _services    = new ConcurrentDictionary<int, ServiceInstance>();

    protected bool Continue
    {
      get => _continueListening;
      set => _continueListening = value;
    }

    #endregion




    #region Constructors

    protected Host(ISerializer serializer)
    {
      _serializer              = serializer ?? new DefaultSerializer();
      _parameterTransferHelper = new ParameterTransferHelper(_serializer);
    }

    #endregion




    #region Properties & Fields - Public

    public IZkRepository ZkRepository
    {
      get => _zkRepository;
      set
      {
        _zkRepository = value;
        _requireZk    = !(_zkRepository is ZkNullRepository);
      }
    }

    public IStats Stats
    {
      get => _stats;
      set => _stats = value ?? _stats;
    }

    public ILog Log
    {
      get => _log;
      set => _log = value ?? _log;
    }

    /// <summary>
    ///   Enable parameter compression. Default is false. There is a performance penalty when
    ///   using compression that should be weighed against network transmission costs of large data
    ///   parameters being serialized across the wire.
    /// </summary>
    public bool UseCompression
    {
      get => _useCompression;
      set => _useCompression = value;
    }

    /// <summary>
    ///   Compression, if enabled, occurs once a parameter exceeds this value in the number of
    ///   bytes. Strings, byte and char arrays, and complex serialized types. The minimum is 1024
    ///   bytes. The default is 128KB.
    /// </summary>
    public int CompressionThreshold
    {
      get => _compressionThreshold;
      set => _compressionThreshold = Math.Max(1024, value);
    }

    #endregion




    #region Methods

    /// <summary>Add this service implementation to the host.</summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="service">The singleton implementation.</param>
    public void AddService<TService>(TService service) where TService : class
    {
      try
      {
        if (_isOpen)
          throw new Exception("Service cannot be added after the host is opened.");

        var serviceType = typeof(TService);

        if (!serviceType.IsInterface)
          throw new ArgumentException("TService must be an interface.", nameof(TService));

        //serviceType.ValidateServiceInterface(); //throws if one class in the interface or its members is not serializable
        var serviceKey = serviceType.ToConfigName(); // serviceType.AssemblyQualifiedName ?? serviceType.FullName;

        if (_serviceKeys.ContainsKey(serviceKey))
          throw new ArgumentException("Service already added. Only one instance allowed.", nameof(service));

        var keyIndex = _serviceKeys.Count;
        _serviceKeys.TryAdd(serviceKey, keyIndex);

        var instance = CreateMethodMap(keyIndex, serviceType, service);
        _services.TryAdd(keyIndex, instance);
      }
      catch (Exception e)
      {
        _log.Fatal("AddService exception on {0}. Error: {1}", service.GetType(), e.ToString().Flatten());
        throw;
      }
    }


    /// <summary>
    ///   Loads all methods from interfaces and assigns an identifier to each. These are later
    ///   synchronized with the client.
    /// </summary>
    private ServiceInstance CreateMethodMap(int keyIndex, Type serviceType, object service)
    {
      void AddMethodsToInstance(ServiceInstance svc, MethodInfo mi, ref int ident)
      {
        svc.InterfaceMethods.TryAdd(ident, mi);

        var parameterInfos = mi.GetParameters();
        var isByRef        = new bool[parameterInfos.Length];

        for (var i = 0; i < isByRef.Length; i++)
          isByRef[i] = parameterInfos[i].ParameterType.IsByRef;

        svc.MethodParametersByRef.TryAdd(ident, isByRef);
        ident++;
      }

      var instance = new ServiceInstance
      {
        KeyIndex              = keyIndex,
        InterfaceType         = serviceType,
        InterfaceMethods      = new ConcurrentDictionary<int, MethodInfo>(),
        MethodParametersByRef = new ConcurrentDictionary<int, bool[]>(),
        SingletonInstance     = service
      };

      var currentMethodIdent = 0;

      foreach (var mi in serviceType.GetMethods())
        AddMethodsToInstance(instance, mi, ref currentMethodIdent);

      foreach (var interfaceType in serviceType.GetInterfaces())
      foreach (var mi in interfaceType.GetMethods())
        AddMethodsToInstance(instance, mi, ref currentMethodIdent);

      //
      // Create a list of sync infos from the dictionary

      var methodSyncInfos = new List<MethodSyncInfo>();

      foreach (var kvp in instance.InterfaceMethods)
      {
        var parameters     = kvp.Value.GetParameters();
        var parameterTypes = new string[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
          parameterTypes[i] = parameters[i].ParameterType.ToConfigName();

        methodSyncInfos.Add(new MethodSyncInfo
        {
          MethodIdent      = kvp.Key,
          MethodName       = kvp.Value.Name,
          MethodReturnType = kvp.Value.ReturnType.ToConfigName(),
          ParameterTypes   = parameterTypes
        });
      }

      var serviceSyncInfo = new ServiceSyncInfo
      {
        ServiceKeyIndex      = keyIndex,
        CompressionThreshold = _compressionThreshold,
        UseCompression       = _useCompression,
        MethodInfos          = methodSyncInfos.ToArray()
      };

      instance.ServiceSyncInfo = serviceSyncInfo;

      return instance;
    }

    /// <summary>
    ///   Opens the host and starts a listener. This listener spawns a new thread (or uses a
    ///   thread pool thread) for each incoming connection.
    /// </summary>
    public void Start()
    {
      _isOpen = true;

      StartListener();
    }

    /// <summary>Closes the host and calls Dispose().</summary>
    public void Stop()
    {
      Dispose();
    }

    protected void ProcessRequest(Stream stream)
    {
      if (null == stream || !stream.CanWrite || !stream.CanRead)
      {
        _log.Error("Cannot process a request on a stream that is not read/write.");
        return;
      }

      ProcessRequest(stream, stream);
    }

    /// <summary>
    ///   This method handles all requests from a single client. There is one thread running
    ///   this method for each connected client.
    /// </summary>
    /// <param name="readStream">The read/write stream.</param>
    /// <param name="writeStream">The read/write stream.</param>
    protected virtual void ProcessRequest(Stream readStream, Stream writeStream)
    {
      if (null == readStream || null == writeStream)
        return;

      var binReader  = new BinaryReader(readStream);
      var binWriter  = new BinaryWriter(writeStream);
      var doContinue = true;

      try
      {
        ZkSession zkSession = null;
        var       sw        = new Stopwatch();

        do
        {
          sw.Start();

          try
          {
            //read message type
            var messageType = (MessageType)binReader.ReadInt32();

            switch (messageType)
            {
              case MessageType.ZkInitiate:
                zkSession  = new ZkSession(_zkRepository, _log, _stats);
                doContinue = zkSession.ProcessZkInitiation(binReader, binWriter, sw);
                break;

              case MessageType.ZkProof:
                if (null == zkSession)
                  throw new NullReferenceException("session null");

                doContinue = zkSession.ProcessZkProof(binReader, binWriter, sw);
                break;

              case MessageType.SyncInterface:
                ProcessSync(zkSession, binReader, binWriter, sw);
                break;

              case MessageType.MethodInvocation:
                ProcessInvocation(zkSession, binReader, binWriter, sw);
                break;

              case MessageType.TerminateConnection:
              default:
                doContinue = false;
                break;
            }
          }
          catch (Exception e) //do not resume operation on this thread if any errors are unhandled.
          {
            _log.Error("Error in ProcessRequest: {0}", e.ToString().Flatten());
            doContinue = false;
          }

          sw.Reset();
        } while (doContinue);
      }
      catch (Exception fatalException)
      {
        _log.Fatal("Fatal error in ProcessRequest: {0}", fatalException.ToString().Flatten());
      }
      finally
      {
        binReader.Close();
        binWriter.Close();
      }
    }

    private void ProcessSync(ZkSession session, BinaryReader binReader, BinaryWriter binWriter, Stopwatch sw)
    {
      string serviceTypeName;
      var    syncCat = "Sync";

      if (_requireZk)
      {
        //use session and encryption - if throws should not have gotten this far
        var len   = binReader.ReadInt32();
        var bytes = binReader.ReadBytes(len);
        var data  = session.Crypto.Decrypt(bytes);

        serviceTypeName = data.ConverToString();
      }
      else
      {
        serviceTypeName = binReader.ReadString();
      }

      if (_serviceKeys.TryGetValue(serviceTypeName, out var serviceKey)
        && _services.TryGetValue(serviceKey, out var svc))
      {
        syncCat = svc.InterfaceType.Name;

        //Create a list of sync infos from the dictionary
        var syncBytes = _serializer.Serialize(svc.ServiceSyncInfo);

        if (_requireZk)
        {
          _log.Debug("Unencrypted data sent to server: {0}", Convert.ToBase64String(syncBytes));

          var encData = session.Crypto.Encrypt(syncBytes);

          binWriter.Write(encData.Length);
          binWriter.Write(encData);

          _log.Debug("Encrypted data sent server: {0}", Convert.ToBase64String(encData));
        }
        else
        {
          binWriter.Write(syncBytes.Length);
          binWriter.Write(syncBytes);
        }
      }
      else
      {
        //return zero to indicate type or version of type not found
        binWriter.Write(0);
      }

      binWriter.Flush();
      _log.Debug("SyncInterface for {0} in {1}ms.", syncCat, sw.ElapsedMilliseconds);
    }

    private void ProcessInvocation(ZkSession session, BinaryReader binReader, BinaryWriter binWriter, Stopwatch sw)
    {
      // Read service instance key
      var cat               = "unknown";
      var methodName        = "MethodInvocation";
      var invokedServiceKey = binReader.ReadInt32();

      if (_services.TryGetValue(invokedServiceKey, out var svc))
      {
        cat = svc.InterfaceType.Name;

        // Read the method identifier
        var methodId = binReader.ReadInt32();

        if (svc.InterfaceMethods.ContainsKey(methodId))
          InvokeMethod(svc, session, binReader, binWriter, methodId, out methodName);
        else
          binWriter.Write((int)MessageType.UnknownMethod);
      }
      else
      {
        binWriter.Write((int)MessageType.UnknownMethod);
      }

      // Flush
      binWriter.Flush();
      _stats.Log(cat, methodName, sw.ElapsedMilliseconds);
    }

    private void InvokeMethod(
      ServiceInstance svc,
      ZkSession       session,
      BinaryReader    binReader,
      BinaryWriter    binWriter,
      int             methodId,
      out string      methodName)
    {
      svc.InterfaceMethods.TryGetValue(methodId, out var method);
      svc.MethodParametersByRef.TryGetValue(methodId, out var isByRef);

      methodName = method.Name;

      // Read parameter data
      object[] parameters;

      if (_requireZk)
      {
        var len     = binReader.ReadInt32();
        var encData = binReader.ReadBytes(len);

        _log.Debug("Encrypted data received from server: {0}", Convert.ToBase64String(encData));

        var data = session.Crypto.Decrypt(encData);

        _log.Debug("Decrypted data received from server: {0}", Convert.ToBase64String(data));

        using (var ms = new MemoryStream(data))
        using (var br = new BinaryReader(ms))
          parameters = _parameterTransferHelper.ReceiveParameters(br);
      }
      else
      {
        parameters = _parameterTransferHelper.ReceiveParameters(binReader);
      }

      // Invoke the method
      object[] returnParameters;
      var      returnMessageType = MessageType.ReturnValues;

      try
      {
        var returnValue = method.Invoke(svc.SingletonInstance, parameters);

        // TODO: Async
        if (returnValue is Task task)
        {
          task.GetAwaiter().GetResult();
          var prop = task.GetType().GetProperty("Result");
          returnValue = prop?.GetValue(task);
        }

        // The result to the client is the return value (null if void) and the input parameters
        returnParameters    = new object[1 + parameters.Length];
        returnParameters[0] = returnValue;

        for (var i = 0; i < parameters.Length; i++)
          returnParameters[i + 1] = isByRef[i] ? parameters[i] : null;
      }
      catch (Exception ex)
      {
        // An exception was caught. Rethrow it client side
        returnParameters  = new object[] { ex };
        returnMessageType = MessageType.ThrowException;
      }

      // Send the result back to the client
      // (1) write the message type
      binWriter.Write((int)returnMessageType);

      // (2) write the return parameters
      if (_requireZk)
      {
        byte[] data;

        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        {
          _parameterTransferHelper.SendParameters(
            svc.ServiceSyncInfo.UseCompression,
            svc.ServiceSyncInfo.CompressionThreshold,
            bw,
            returnParameters);
          data = ms.ToArray();
        }

        _log.Debug("Unencrypted data sent server: {0}", Convert.ToBase64String(data));

        var encData = session.Crypto.Encrypt(data);

        _log.Debug("Encrypted data sent server: {0}", Convert.ToBase64String(encData));

        binWriter.Write(encData.Length);
        binWriter.Write(encData);
      }
      else
      {
        _parameterTransferHelper.SendParameters(
          svc.ServiceSyncInfo.UseCompression,
          svc.ServiceSyncInfo.CompressionThreshold,
          binWriter,
          returnParameters);
      }
    }

    #endregion




    #region Methods Abs

    protected abstract void StartListener();

    #endregion




    #region IDisposable Members

    public void Dispose()
    {
      //MS recommended dispose pattern - prevents GC from disposing again
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (_disposed)
        return;

      _disposed = true; //prevent second call to Dispose

      if (!disposing)
        return;

      if (_log is Logger log) log.FlushLog();
      if (_stats is Stats stat) stat.FlushLog();

      _isOpen  = false;
      Continue = false;

      foreach (var instance in _services)
        if (instance.Value.SingletonInstance is IDisposable disposable)
          disposable.Dispose();
    }

    #endregion
  }
}
