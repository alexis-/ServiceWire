namespace ServiceWire.NamedPipes
{
  using System;
  using Common;

  public class NpClient<TInterface> : BaseClient<TInterface>
    where TInterface : class
  {
    #region Properties & Fields - Non-Public

    private readonly TInterface _proxy;
    private          bool       _disposed = false;

    #endregion




    #region Constructors

    /// <summary>Create a named pipes client.</summary>
    /// <param name="npAddress"></param>
    /// <param name="serializer">
    ///   Inject your own serializer for complex objects and avoid using the
    ///   Newtonsoft JSON DefaultSerializer.
    /// </param>
    public NpClient(NpEndPoint npAddress, ISerializer serializer = null)
    {
      if (null == serializer) serializer = new DefaultSerializer();
      _proxy = NpProxy.CreateProxy<TInterface>(npAddress, serializer);
    }


    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        _disposed = true; //prevent second call to Dispose
        if (disposing)
          (_proxy as NpChannel).Dispose();
      }
    }




    #region IDisposable Members

    public override void Dispose()
    {
      //MS recommended dispose pattern - prevents GC from disposing again
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    #endregion




    #region Properties Impl - Public

    public override TInterface Proxy => _proxy;

    public override bool IsConnected => _proxy != null && (_proxy as NpChannel).IsConnected;

    #endregion
  }
}
