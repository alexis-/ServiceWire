namespace ServiceWire.NamedPipes
{
  using System.IO;

  public class ReadFileToStream
  {
    #region Properties & Fields - Non-Public

    private readonly string       fn;
    private readonly StreamString ss;

    #endregion




    #region Constructors

    public ReadFileToStream(StreamString str, string filename)
    {
      fn = filename;
      ss = str;
    }

    #endregion




    #region Methods

    public void Start()
    {
      var contents = File.ReadAllText(fn);
      ss.WriteString(contents);
    }

    #endregion
  }
}
