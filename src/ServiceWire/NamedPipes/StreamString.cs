namespace ServiceWire.NamedPipes
{
  using System.IO;
  using System.Text;

  public class StreamString
  {
    #region Properties & Fields - Non-Public

    private readonly Stream          ioStream;
    private readonly UnicodeEncoding streamEncoding;

    #endregion




    #region Constructors

    public StreamString(Stream ioStream)
    {
      this.ioStream  = ioStream;
      streamEncoding = new UnicodeEncoding();
    }

    #endregion




    #region Methods

    public string ReadString()
    {
      var len = 0;

      len =  ioStream.ReadByte() * 256;
      len += ioStream.ReadByte();
      var inBuffer = new byte[len];
      ioStream.Read(inBuffer, 0, len);

      return streamEncoding.GetString(inBuffer);
    }

    public int WriteString(string outString)
    {
      var outBuffer = streamEncoding.GetBytes(outString);
      var len       = outBuffer.Length;
      if (len > ushort.MaxValue)
        len = (int)ushort.MaxValue;
      ioStream.WriteByte((byte)(len / 256));
      ioStream.WriteByte((byte)(len & 255));
      ioStream.Write(outBuffer, 0, len);
      ioStream.Flush();

      return outBuffer.Length + 2;
    }

    #endregion
  }
}
