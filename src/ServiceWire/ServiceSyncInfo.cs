namespace ServiceWire
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  [DataContract]
  public class ServiceSyncInfo
  {
    #region Properties & Fields - Public

    [DataMember(Order = 1)]
    public int ServiceKeyIndex { get; set; }
    [DataMember(Order = 2)]
    public MethodSyncInfo[] MethodInfos { get; set; }
    [DataMember(Order = 3)]
    public bool UseCompression { get; set; }
    [DataMember(Order = 4)]
    public int CompressionThreshold { get; set; }

    #endregion
  }
}
