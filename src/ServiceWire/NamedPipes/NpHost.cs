namespace ServiceWire.NamedPipes
{
  using System;
  using System.IO;

  public class NpHost : Host
  {
    #region Properties & Fields - Non-Public

    private readonly NpListener _listener;
    private readonly string     _pipeName;
    private          bool       _useThreadPool = false;

    #endregion




    #region Constructors

    /// <summary>
    ///   Constructs an instance of the host and starts listening for incoming connections. All
    ///   listener threads are regular background threads. NOTE: the instance is not automatically
    ///   thread safe!
    /// </summary>
    /// <param name="ep">The NP end point data</param>
    /// <param name="log"></param>
    /// <param name="stats"></param>
    /// <param name="serializer">
    ///   Inject your own serializer for complex objects and avoid using the
    ///   Newtonsoft JSON DefaultSerializer.
    /// </param>
    public NpHost(NpEndPoint ep, ILog log = null, IStats stats = null, ISerializer serializer = null)
    : this(ep.PipeName, log, stats, serializer)
    {
    }

    /// <summary>
    ///   Constructs an instance of the host and starts listening for incoming connections. All
    ///   listener threads are regular background threads. NOTE: the instance is not automatically
    ///   thread safe!
    /// </summary>
    /// <param name="pipeName">The pipe name for incoming requests</param>
    /// <param name="log"></param>
    /// <param name="stats"></param>
    /// <param name="serializer">
    ///   Inject your own serializer for complex objects and avoid using the
    ///   Newtonsoft JSON DefaultSerializer.
    /// </param>
    public NpHost(string pipeName, ILog log = null, IStats stats = null, ISerializer serializer = null)
      : base(serializer)
    {
      Log                      =  log;
      Stats                    =  stats;
      _pipeName                =  pipeName;
      _listener                =  new NpListener(_pipeName, log: Log, stats: Stats);
      _listener.OnClientConnected += ClientConnected;
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>Get or set whether the host should use regular or thread pool threads.</summary>
    public bool UseThreadPool
    {
      get => _useThreadPool;
      set
      {
        if (_isOpen)
          throw new Exception("The host is already open");

        _useThreadPool = value;
      }
    }

    /// <summary>Gets the end point this host is listening on</summary>
    public string PipeName => _pipeName;

    #endregion




    #region Methods Impl

    protected override void StartListener()
    {
      _listener.StartListen(); //start listening in the background
    }

    #endregion




    #region Methods

    /// <summary>
    ///   This method handles all requests on separate thread per client connection. There is
    ///   one thread running this method for each connected client.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ClientConnected(object sender, PipeClientConnectionEventArgs args)
    {
      var stream = new BufferedStream(args.PipeStream);

      ProcessRequest(stream);
    }

    #endregion




    #region IDisposable Members

    private bool _disposed = false;

    protected override void Dispose(bool disposing)
    {
      if (_disposed)
        return;

      _disposed = true; //prevent second call to Dispose

      if (disposing)
        _listener.StopListen();
    }

    #endregion
  }
}
