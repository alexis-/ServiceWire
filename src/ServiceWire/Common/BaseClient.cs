namespace ServiceWire.Common
{
  using System;

  public abstract class BaseClient<TInterface> : IDisposable
    where TInterface : class
  {
    #region Constructors

    /// <inheritdoc />
    public abstract void Dispose();

    #endregion




    #region Methods

    public virtual void InjectLoggerStats(ILog logger, IStats stats) { }

    #endregion




    #region Methods Abs

    public abstract TInterface Proxy { get; }

    public abstract bool IsConnected { get; }

    #endregion
  }
}
