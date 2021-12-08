namespace ServiceWire.TcpIp
{
  using System;
  using System.Net;
  using System.Threading.Tasks;
  using Common;
  using ZeroKnowledge;

  public class TcpBiDirectionalHost : TcpHost, IBiDirectionalContract
  {
    #region Constructors

    /// <inheritdoc />
    public TcpBiDirectionalHost(int           port,
                                ILog          log          = null,
                                IStats        stats        = null,
                                IZkRepository zkRepository = null,
                                ISerializer   serializer   = null)
      : base(port, log, stats, zkRepository, serializer) { }

    /// <inheritdoc />
    public TcpBiDirectionalHost(IPEndPoint    endpoint,
                                ILog          log          = null,
                                IStats        stats        = null,
                                IZkRepository zkRepository = null,
                                ISerializer   serializer   = null)
      : base(endpoint, log, stats, zkRepository, serializer) { }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public Task<bool> EstablishReverseConnection(EndPointBase reverseEndPoint)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool TestConnection()
    {
      throw new NotImplementedException();
    }

    #endregion




    #region Methods

    //public BaseClient<TInterface> CreateReverseClient<TInterface>(ISerializer serializer = null)
    //  where TInterface : class;

    #endregion
  }
}
