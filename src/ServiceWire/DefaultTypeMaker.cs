namespace ServiceWire
{
  using System;

  internal class DefaultTypeMaker
  {
    #region Methods

    public object GetDefault(Type t)
    {
      return GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
    }

    public T GetDefaultGeneric<T>()
    {
      return default;
    }

    #endregion
  }
}
