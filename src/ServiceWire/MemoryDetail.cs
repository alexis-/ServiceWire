namespace ServiceWire
{
  public class MemoryDetail
  {
    #region Properties & Fields - Public

    public ulong TotalVisibleMemorySize { get; set; }
    public ulong TotalVirtualMemorySize { get; set; }
    public ulong FreePhysicalMemory     { get; set; }
    public ulong FreeVirtualMemory      { get; set; }

    #endregion
  }
}
