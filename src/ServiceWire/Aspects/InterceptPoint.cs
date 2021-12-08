namespace ServiceWire.Aspects
{
  public class InterceptPoint
  {
    #region Properties & Fields - Public

    public int                  Id     { get; set; }
    public object               Target { get; set; }
    public CrossCuttingConcerns Cut    { get; set; }

    #endregion
  }
}
