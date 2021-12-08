namespace ServiceWire.NamedPipes
{
  using System;
  using System.IO.Pipes;

  public class PipeClientConnectionEventArgs : EventArgs
  {
    #region Constructors

    public PipeClientConnectionEventArgs(NamedPipeServerStream pipeStream)
    {
      PipeStream = pipeStream;
    }

    #endregion




    #region Properties & Fields - Public

    public NamedPipeServerStream PipeStream { get; set; }

    #endregion
  }
}
