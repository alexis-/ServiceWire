namespace ServiceWire
{
  using System;
  using System.Collections.Concurrent;
  using System.Reflection;

  public class ServiceInstance
  {
    #region Properties & Fields - Public

    public int                                   KeyIndex              { get; set; }
    public Type                                  InterfaceType         { get; set; }
    public object                                SingletonInstance     { get; set; }
    public ConcurrentDictionary<int, MethodInfo> InterfaceMethods      { get; set; }
    public ConcurrentDictionary<int, bool[]>     MethodParametersByRef { get; set; }
    public ServiceSyncInfo                       ServiceSyncInfo       { get; set; }

    #endregion
  }
}
