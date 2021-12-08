namespace ServiceWire.TcpIp
{
  using System;
  using System.IO;
  using System.Net;
  using System.Net.Sockets;
  using System.Threading;

  public class TcpChannel : StreamingChannel
  {
    #region Properties & Fields - Non-Public

    private Socket _client;
    private string _username;
    private string _password;

    #endregion




    #region Constructors

    /// <summary>Creates a connection to the concrete object handling method calls on the server side</summary>
    /// <param name="serviceType"></param>
    /// <param name="endpoint"></param>
    /// <param name="serializer">
    ///   Inject your own serializer for complex objects and avoid using the
    ///   Newtonsoft JSON DefaultSerializer.
    /// </param>
    public TcpChannel(Type serviceType, IPEndPoint endpoint, ISerializer serializer)
      : base(serializer)
    {
      Initialize(null, null, serviceType, endpoint, 2500);
    }

    /// <summary>Creates a connection to the concrete object handling method calls on the server side</summary>
    /// <param name="serviceType"></param>
    /// <param name="endpoint"></param>
    /// <param name="serializer">
    ///   Inject your own serializer for complex objects and avoid using the
    ///   Newtonsoft JSON DefaultSerializer.
    /// </param>
    public TcpChannel(Type serviceType, TcpEndPoint endpoint, ISerializer serializer)
      : base(serializer)
    {
      Initialize(null, null, serviceType, endpoint.EndPoint, endpoint.ConnectTimeOutMs);
    }

    /// <summary>
    ///   Creates a secure connection to the concrete object handling method calls on the
    ///   server side
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="endpoint"></param>
    /// <param name="serializer">
    ///   Inject your own serializer for complex objects and avoid using the
    ///   Newtonsoft JSON DefaultSerializer.
    /// </param>
    public TcpChannel(Type serviceType, TcpZkEndPoint endpoint, ISerializer serializer)
      : base(serializer)
    {
      if (endpoint == null) throw new ArgumentNullException("endpoint");
      if (endpoint.Username == null) throw new ArgumentNullException("endpoint.Username");
      if (endpoint.Password == null) throw new ArgumentNullException("endpoint.Password");

      Initialize(endpoint.Username, endpoint.Password,
                 serviceType, endpoint.EndPoint, endpoint.ConnectTimeOutMs);
    }




    #region IDisposable override

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (disposing)
        _client.Dispose();
    }

    #endregion

    #endregion




    #region Properties Impl - Public

    public override bool IsConnected => null != _client && _client.Connected;

    #endregion




    #region Methods

    private void Initialize(string     username,
                            string     password,
                            Type       serviceType,
                            IPEndPoint endpoint,
                            int        connectTimeoutMs)
    {
      _username = username;
      _password = password;
      _serviceType = serviceType;
      _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // TcpClient(AddressFamily.InterNetwork);
      _client.LingerState.Enabled = false;

      var connected = false;
      var connectEventArgs = new SocketAsyncEventArgs
      {
        RemoteEndPoint = endpoint
      };
      connectEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>((sender, e) => { connected = true; });

      if (_client.ConnectAsync(connectEventArgs))
        //operation pending - (false means completed synchronously)
        while (!connected)
          if (!SpinWait.SpinUntil(() => connected, connectTimeoutMs))
          {
            _client.Dispose();
            throw new TimeoutException("Unable to connect within " + connectTimeoutMs + "ms");
          }

      if (connectEventArgs.SocketError != SocketError.Success)
      {
        _client.Dispose();
        throw new SocketException((int)connectEventArgs.SocketError);
      }

      if (!_client.Connected)
      {
        _client.Dispose();
        throw new SocketException((int)SocketError.NotConnected);
      }

      _stream    = new BufferedStream(new NetworkStream(_client), 8192);
      _binReader = new BinaryReader(_stream);
      _binWriter = new BinaryWriter(_stream);
      try
      {
        SyncInterface(_serviceType, _username, _password);
      }
      catch
      {
        Dispose(true);
        throw;
      }
    }

    #endregion
  }
}
