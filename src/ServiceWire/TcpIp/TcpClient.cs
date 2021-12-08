namespace ServiceWire.TcpIp
{
  using System;
  using System.Net;
  using Common;

  public class TcpClient<TInterface> : BaseClient<TInterface>
    where TInterface : class
  {
    #region Constructors

    public TcpClient(TcpEndPoint endpoint, ISerializer serializer = null)
    {
      if (null == serializer) serializer = new DefaultSerializer();
      Proxy = TcpProxy.CreateProxy<TInterface>(endpoint, serializer);
    }

    public TcpClient(TcpZkEndPoint endpoint, ISerializer serializer = null)
    {
      if (null == serializer) serializer = new DefaultSerializer();
      Proxy = TcpProxy.CreateProxy<TInterface>(endpoint, serializer);
    }

    public TcpClient(IPEndPoint endpoint, ISerializer serializer = null)
    {
      if (null == serializer) serializer = new DefaultSerializer();
      Proxy = TcpProxy.CreateProxy<TInterface>(endpoint, serializer);
    }

    #endregion




    #region Properties Impl - Public

    public override TInterface Proxy { get; }

    public override bool IsConnected => (Proxy as TcpChannel)?.IsConnected == true;

    #endregion




    #region Methods Impl

    public override void InjectLoggerStats(ILog logger, IStats stats)
    {
      var channel = Proxy as Channel;
      channel?.InjectLoggerStats(logger, stats);
    }

    #endregion




    #region IDisposable Members

    private bool _disposed;

    public override void Dispose()
    {
      //MS recommended dispose pattern - prevents GC from disposing again
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        _disposed = true; //prevent second call to Dispose
        if (disposing)
          (Proxy as TcpChannel)?.Dispose();
      }
    }

    #endregion
  }
}
