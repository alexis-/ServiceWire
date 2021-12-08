namespace ServiceWireTests.Tests
{
  using System.Net;
  using ServiceWire;
  using ServiceWire.Common;
  using ServiceWire.TcpIp;

  public class TcpTests : CommonTests
  {
    #region Constants & Statics

    private const int Port = 8099;

    #endregion




    #region Properties & Fields - Non-Public

    private IPAddress IpAddress { get; } = IPAddress.Parse("127.0.0.1");

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override EndPointBase CreateEndPoint(int clientId)
    {
      return new TcpEndPoint(CreateIPEndPoint(clientId));
    }

    /// <inheritdoc />
    public override Host CreateHost(int hostId, ISerializer serializer = null)
    {
      return new TcpHost(CreateIPEndPoint(hostId), null, null, null, serializer);
    }

    #endregion




    #region Methods

    private IPEndPoint CreateIPEndPoint(int clientId)
    {
      return new(IpAddress, Port + clientId);
    }

    #endregion
  }
}
