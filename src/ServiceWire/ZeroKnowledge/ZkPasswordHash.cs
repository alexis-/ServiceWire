namespace ServiceWire.ZeroKnowledge
{
  public class ZkPasswordHash
  {
    #region Properties & Fields - Public

    public byte[] Salt     { get; set; }
    public byte[] Key      { get; set; }
    public byte[] Verifier { get; set; }

    #endregion
  }
}
