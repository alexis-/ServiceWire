namespace ServiceWire
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  [DataContract]
  public class MethodSyncInfo
  {
    #region Properties & Fields - Public

    [DataMember(Order = 1)]
    public int MethodIdent { get; set; }
    [DataMember(Order = 2)]
    public string MethodName { get; set; }
    [DataMember(Order = 3)]
    public string MethodReturnType { get; set; }
    [DataMember(Order = 4)]
    public string[] ParameterTypes { get; set; }

    #endregion
  }
}
