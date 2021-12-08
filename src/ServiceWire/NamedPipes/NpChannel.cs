namespace ServiceWire.NamedPipes
{
  using System;
  using System.IO;
  using System.IO.Pipes;

  public class NpChannel : StreamingChannel
  {
    #region Properties & Fields - Non-Public

    private readonly NamedPipeClientStream _clientStream;

    #endregion




    #region Constructors

    /// <summary>
    ///   Creates a connection to the concrete object handling method calls on the pipeName
    ///   server side
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="npEndPoint"></param>
    /// <param name="serializer">
    ///   Inject your own serializer for complex objects and avoid using the
    ///   Newtonsoft JSON DefaultSerializer.
    /// </param>
    public NpChannel(Type serviceType, NpEndPoint npEndPoint, ISerializer serializer)
      : base(serializer)
    {
      _serviceType  = serviceType;
      _clientStream = new NamedPipeClientStream(npEndPoint.ServerName, npEndPoint.PipeName, PipeDirection.InOut);
      _clientStream.Connect(npEndPoint.ConnectTimeOutMs);
      _stream    = new BufferedStream(_clientStream);
      _binReader = new BinaryReader(_clientStream);
      _binWriter = new BinaryWriter(_clientStream);
      try
      {
        SyncInterface(_serviceType);
      }
      catch (Exception)
      {
        Dispose(true);
        throw;
      }
    }

    #endregion




    #region Properties Impl - Public

    public override bool IsConnected => null != _clientStream && _clientStream.IsConnected;

    #endregion
  }
}
