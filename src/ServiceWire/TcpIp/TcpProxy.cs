namespace ServiceWire.TcpIp
{
  using System.Net;

  public sealed class TcpProxy
  {
    #region Methods

    public static TInterface CreateProxy<TInterface>(TcpZkEndPoint endpoint, ISerializer serializer) where TInterface : class
    {
      return ProxyFactory.CreateProxy<TInterface>(typeof(TcpChannel), typeof(TcpZkEndPoint), endpoint, serializer);
    }

    public static TInterface CreateProxy<TInterface>(TcpEndPoint endpoint, ISerializer serializer) where TInterface : class
    {
      return ProxyFactory.CreateProxy<TInterface>(typeof(TcpChannel), typeof(TcpEndPoint), endpoint, serializer);
    }

    public static TInterface CreateProxy<TInterface>(IPEndPoint endpoint, ISerializer serializer) where TInterface : class
    {
      return ProxyFactory.CreateProxy<TInterface>(typeof(TcpChannel), typeof(IPEndPoint), endpoint, serializer);
    }

    #endregion
  }
}
