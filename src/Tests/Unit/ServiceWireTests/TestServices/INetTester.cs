namespace ServiceWireTests.TestServices
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using ServiceWire.Common;

  public interface INetTesterV1
  {
    int                  Min(int                         a,     int    b);
    Dictionary<int, int> Range(int                       start, int    count);
    TestResponse         Get(Guid                        id,    string label, double weight, out int quantity);
    Task<int>            CalculateAsync(int              a,     int    b);
    bool                 SetReverseEndPoint(EndPointBase ep);
  }

  public interface INetTesterV2
  {
    int                  Max(int                         a,     int    b);
    Dictionary<int, int> Range(int                       start, int    count);
    TestResponse         Get(Guid                        id,    string label, double weight, out int quantity);
    Task<int>            CalculateAsync(int              a,     int    b);
    bool                 SetReverseEndPoint(EndPointBase ep);
  }
}
