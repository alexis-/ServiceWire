namespace ServiceWire.NamedPipes
{
  using System;
  using System.IO.Pipes;
  using System.Threading;
  using System.Threading.Tasks;

#if NET462
  using System.Security.AccessControl;
  using System.Security.Principal;
#endif

  public class NpListener
  {
    private          bool            _running;
    private readonly EventWaitHandle _terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
    private readonly int             _maxConnections;
#if NET462
    private PipeSecurity _pipeSecurity = null;
#endif
    private readonly ILog   _log   = new NullLogger();
    private readonly IStats _stats = new NullStats();

    public string                                            PipeName { get; set; }
    public event EventHandler<PipeClientConnectionEventArgs> OnClientConnected;

    public NpListener(string pipeName, int maxConnections = 254, ILog log = null, IStats stats = null)
    {
      _log   = log ?? _log;
      _stats = stats ?? _stats;

      _maxConnections = Math.Min(254, maxConnections);
      PipeName        = pipeName;

#if NET462
      _pipeSecurity = new PipeSecurity();

      SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

      _pipeSecurity.AddAccessRule(new PipeAccessRule(everyone, PipeAccessRights.ReadWrite, AccessControlType.Allow));
      _pipeSecurity.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User, PipeAccessRights.FullControl, AccessControlType.Allow));
      _pipeSecurity.AddAccessRule(new PipeAccessRule(@"SYSTEM", PipeAccessRights.FullControl, AccessControlType.Allow));
#endif
    }

    public void StartListen()
    {
      _running = true;

      Task.Factory.StartNew(ServerLoop, TaskCreationOptions.LongRunning);
    }

    public void StopListen()
    {
      if (!_running)
        return;

      _running = false;

      //make fake connection to terminate the waiting stream
      try
      {
        using (var client = new NamedPipeClientStream(PipeName))
          client.Connect(50);
      }
      catch (Exception e)
      {
        _log.Error("Stop error: {0}", e.ToString().Flatten());
      }

      _terminateHandle.WaitOne();
    }

    private void ServerLoop()
    {
      try
      {
        while (_running)
          WaitForClientConnection();
      }
      catch (Exception e)
      {
        _log.Fatal("ServerLoop fatal error: {0}", e.ToString().Flatten());
      }
      finally
      {
        _terminateHandle.Set();
      }
    }

    private void WaitForClientConnection()
    {
      try
      {
#if NET462
        var pipeStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, _maxConnections, 
            PipeTransmissionMode.Byte, PipeOptions.None, 512, 512, _pipeSecurity);
#else
        var pipeStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, _maxConnections);
#endif

        pipeStream.WaitForConnection();

        //Task.Factory.StartNew(() => ProcessClientThread(pipeStream), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(() => ProcessClientThread(pipeStream));
      }
      catch (Exception e)
      {
        //If there are no more avail connections (254 is in use already) then just keep looping until one is avail
        _log.Error("ProcessNextClient error: {0}", e.ToString().Flatten());
      }
    }

    private void ProcessClientThread(NamedPipeServerStream pipeStream)
    {
      try
      {
        if (OnClientConnected == null)
          return;

        var args = new PipeClientConnectionEventArgs(pipeStream);

        OnClientConnected(this, args);
      }
      catch (Exception e)
      {
        _log.Error("ProcessClientThread error: {0}", e.ToString().Flatten());
      }
      finally
      {
        if (pipeStream.IsConnected) pipeStream.Close();
        pipeStream.Dispose();
      }
    }
  }

  // Defines the data protocol for reading and writing strings on our stream

  // Contains the method executed in the context of the impersonated user
}
