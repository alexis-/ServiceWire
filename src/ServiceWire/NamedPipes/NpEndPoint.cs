namespace ServiceWire.NamedPipes
{
  using System;
  using Common;

  [Serializable]
  public class NpEndPoint : EndPointBase
  {
    #region Constructors

    public NpEndPoint(string pipeName, int connectTimeOutMs = 2500) : this(".", pipeName, connectTimeOutMs) { }

    public NpEndPoint(string serverName, string pipeName, int connectTimeOutMs = 2500)
    {
      ServerName       = serverName;
      PipeName         = pipeName;
      ConnectTimeOutMs = connectTimeOutMs;
    }

    #endregion




    #region Properties & Fields - Public

    public string ServerName       { get; set; }
    public string PipeName         { get; set; }
    public int    ConnectTimeOutMs { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override BaseClient<TInterface> CreateClient<TInterface>(ISerializer serializer = null)
    {
      return new NpClient<TInterface>(this, serializer);
    }

    #endregion
  }
}
