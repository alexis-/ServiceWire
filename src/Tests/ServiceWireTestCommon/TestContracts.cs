namespace ServiceWireTestCommon
{
  using System;
  using System.Collections.Generic;
  using System.Net;
  using System.Threading.Tasks;
  using ServiceWire.NamedPipes;
  using ServiceWire.TcpIp;

  public interface IValTypes
  {
    decimal       GetDecimal(decimal      input);
    Task<decimal> GetDecimalAsync(decimal input);
    bool          OutDecimal(decimal      val);
    Task<bool>    OutDecimalAsync(decimal val);
  }

  public interface INetTester
  {
    Guid               GetId(string       source, double   weight, int    quantity, DateTime dt);
    TestResponse       Get(Guid           id,     string   label,  double weight,   out long quantity);
    long               TestLong(out long  id1,    out long id2);
    List<string>       GetItems(Guid      id);
    Task<List<string>> GetItemsAsync(Guid id);
  }

  public interface IMyTester
  {
    Guid         GetId(string  source, double weight, int    quantity);
    TestResponse Get(Guid      id,     string label,  double weight, out int quantity);
    List<string> GetItems(Guid id,     int[]  vals);
  }

  [Serializable]
  public struct TestResponse
  {
    public Guid   Id       { get; set; }
    public string Label    { get; set; }
    public long   Quantity { get; set; }
  }

  public class NetTcpTesterProxy : TcpClient<INetTester>, INetTester
  {
    #region Constructors

    public NetTcpTesterProxy(TcpEndPoint endpoint) : base(endpoint) { }

    public NetTcpTesterProxy(IPEndPoint endpoint) : base(endpoint) { }

    #endregion




    #region Methods Impl

    public Guid GetId(string source, double weight, int quantity, DateTime dt)
    {
      return Proxy.GetId(source, weight, quantity, dt);
    }

    public TestResponse Get(Guid id, string label, double weight, out long quantity)
    {
      return Proxy.Get(id, label, weight, out quantity);
    }

    public List<string> GetItems(Guid id)
    {
      return Proxy.GetItems(id);
    }

    public Task<List<string>> GetItemsAsync(Guid id)
    {
      return Task.FromResult(Proxy.GetItems(id));
    }

    public long TestLong(out long id1, out long id2)
    {
      id1 = 23;
      id2 = 24;
      return 25;
    }

    #endregion
  }

  public class NetNpTesterProxy : NpClient<INetTester>, INetTester
  {
    #region Constructors

    public NetNpTesterProxy(NpEndPoint npAddress) : base(npAddress) { }

    #endregion




    #region Methods Impl

    public Guid GetId(string source, double weight, int quantity, DateTime dt)
    {
      return Proxy.GetId(source, weight, quantity, dt);
    }

    public TestResponse Get(Guid id, string label, double weight, out long quantity)
    {
      return Proxy.Get(id, label, weight, out quantity);
    }

    public List<string> GetItems(Guid id)
    {
      return Proxy.GetItems(id);
    }

    public Task<List<string>> GetItemsAsync(Guid id)
    {
      return Task.FromResult(Proxy.GetItems(id));
    }

    public long TestLong(out long id1, out long id2)
    {
      return Proxy.TestLong(out id1, out id2);
    }

    #endregion
  }

  public class NetTcpMyTesterProxy : TcpClient<IMyTester>, IMyTester
  {
    #region Constructors

    public NetTcpMyTesterProxy(IPEndPoint endpoint)
      : base(endpoint) { }

    #endregion




    #region Methods Impl

    public Guid GetId(string source, double weight, int quantity)
    {
      return Proxy.GetId(source, weight, quantity);
    }

    public TestResponse Get(Guid id, string label, double weight, out int quantity)
    {
      return Proxy.Get(id, label, weight, out quantity);
    }

    public List<string> GetItems(Guid id, int[] vals)
    {
      return Proxy.GetItems(id, vals);
    }

    #endregion
  }

  public class NetNpMyTesterProxy : NpClient<IMyTester>, IMyTester
  {
    #region Constructors

    public NetNpMyTesterProxy(NpEndPoint npAddress)
      : base(npAddress) { }

    #endregion




    #region Methods Impl

    public Guid GetId(string source, double weight, int quantity)
    {
      return Proxy.GetId(source, weight, quantity);
    }

    public TestResponse Get(Guid id, string label, double weight, out int quantity)
    {
      return Proxy.Get(id, label, weight, out quantity);
    }

    public List<string> GetItems(Guid id, int[] vals)
    {
      return Proxy.GetItems(id, vals);
    }

    #endregion
  }
}
