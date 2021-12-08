namespace ServiceWire.TcpIp
{
  using System;
  using System.Net;
  using Common;

  [Serializable]
  public class TcpEndPoint : EndPointBase
  {
    #region Constructors

    public TcpEndPoint(IPEndPoint endPoint, int connectTimeOutMs = 2500)
    {
      AddressBytes     = endPoint.Address.GetAddressBytes();
      Port             = endPoint.Port;
      ConnectTimeOutMs = connectTimeOutMs;
    }

    #endregion




    #region Properties & Fields - Public

    public IPEndPoint EndPoint => new IPEndPoint(
      new IPAddress(AddressBytes),
      Port);

    public byte[] AddressBytes     { get; set; }
    public int    Port             { get; set; }
    public int    ConnectTimeOutMs { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override BaseClient<TInterface> CreateClient<TInterface>(ISerializer serializer = null)
    {
      return new TcpClient<TInterface>(this, serializer);
    }

    #endregion
  }
}
