namespace ServiceWireTests.Tests
{
  using ServiceWire;
  using ServiceWire.Common;
  using ServiceWire.NamedPipes;

  public class NpTests : CommonTests
  {
    #region Constants & Statics

    private const string PipeName = "ServiceWireTestHost";

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override EndPointBase CreateEndPoint(int clientId)
    {
      return new NpEndPoint(CreatePipeName(clientId));
    }

    /// <inheritdoc />
    public override Host CreateHost(int hostId, ISerializer serializer = null)
    {
      return new NpHost(CreatePipeName(hostId), null, null, serializer);
    }

    #endregion




    #region Methods

    private static string CreatePipeName(int clientId)
    {
      return PipeName + clientId;
    }

    #endregion
  }
}
