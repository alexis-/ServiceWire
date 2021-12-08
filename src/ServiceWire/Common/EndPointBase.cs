namespace ServiceWire.Common
{
  using System;

  [Serializable]
  public abstract class EndPointBase
  {
    #region Methods Abs

    public abstract BaseClient<TInterface> CreateClient<TInterface>(ISerializer serializer = null)
      where TInterface : class;

    #endregion
  }
}
