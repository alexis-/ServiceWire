namespace ServiceWire.TcpIp
{
  using System.Net;

  public class TcpZkEndPoint
  {
    #region Constructors

    public TcpZkEndPoint(string username, string password, IPEndPoint endPoint, int connectTimeOutMs = 2500)
    {
      Username         = username;
      Password         = password;
      EndPoint         = endPoint;
      ConnectTimeOutMs = connectTimeOutMs;
    }

    #endregion




    #region Properties & Fields - Public

    public string     Username         { get; set; }
    public string     Password         { get; set; }
    public IPEndPoint EndPoint         { get; set; }
    public int        ConnectTimeOutMs { get; set; }

    #endregion
  }
}
