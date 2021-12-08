namespace ServiceWireTests.TestServices
{
  using System;

  [Serializable]
  public struct TestResponse
  {
    public Guid   Id       { get; set; }
    public string Label    { get; set; }
    public long   Quantity { get; set; }
  }
}
