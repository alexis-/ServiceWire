namespace ServiceWireTests.TestServices
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using ServiceWire.Common;

  public delegate bool SetReverseEndPointDel(EndPointBase ep);

  public abstract class NetTesterBase
  {
    #region Properties & Fields - Non-Public

    private readonly SetReverseEndPointDel _setEndPointCallback;

    #endregion




    #region Constructors

    protected NetTesterBase(SetReverseEndPointDel setEndPointCallback = null)
    {
      _setEndPointCallback = setEndPointCallback;
    }

    #endregion




    #region Methods

    public Dictionary<int, int> Range(int start, int count)
    {
      return Enumerable.Range(start, count).ToDictionary(key => key, el => el);
    }

    public Task<int> CalculateAsync(int a, int b)
    {
      return Task.FromResult(a + b);
    }

    public TestResponse Get(Guid id, string label, double weight, out int quantity)
    {
      quantity = 44;
      return new TestResponse { Id = id, Label = "MyLabel", Quantity = quantity };
    }

    public bool SetReverseEndPoint(EndPointBase ep)
    {
      return _setEndPointCallback(ep);
    }

    #endregion
  }

  public class NetTesterV1 : NetTesterBase, INetTesterV1
  {
    #region Constructors

    public NetTesterV1(SetReverseEndPointDel setEndPointCallback = null)
      : base(setEndPointCallback) { }

    #endregion




    #region Methods Impl

    public int Min(int a, int b)
    {
      return Math.Min(a, b);
    }

    #endregion
  }

  public class NetTesterV2 : NetTesterBase, INetTesterV2
  {
    #region Constructors

    public NetTesterV2(SetReverseEndPointDel setEndPointCallback = null)
      : base(setEndPointCallback) { }

    #endregion




    #region Methods Impl

    public int Max(int a, int b)
    {
      return Math.Max(a, b);
    }

    #endregion
  }
}
