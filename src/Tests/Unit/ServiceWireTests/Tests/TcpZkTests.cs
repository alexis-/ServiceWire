namespace ServiceWireTests.Tests
{
  using System;
  using System.Net;
  using System.Threading.Tasks;
  using ServiceWire.TcpIp;
  using ServiceWire.ZeroKnowledge;
  using TestServices;
  using Xunit;

  public class FakeZkRepository : IZkRepository
  {
    #region Properties & Fields - Non-Public

    private readonly string         password  = "cc3a6a12-0e5b-47fb-ae45-3485e34582d4";
    private readonly ZkProtocol     _protocol = new ZkProtocol();
    private          ZkPasswordHash _hash     = null;

    #endregion




    #region Methods Impl

    public ZkPasswordHash GetPasswordHashSet(string username)
    {
      if (_hash == null) _hash = _protocol.HashCredentials(username, password);
      return _hash;
    }

    #endregion
  }

  public class TcpZkTests : IDisposable
  {
    public TcpZkTests()
    {
      _tester    = new NetTesterV1();
      _ipAddress = IPAddress.Parse("127.0.0.1");

      _tcphost = new TcpHost(CreateEndPoint(), zkRepository: _repo);
      _tcphost.AddService<INetTesterV1>(_tester);
      _tcphost.Start();
    }

    public void Dispose()
    {
      _tcphost.Stop();
    }

    private readonly INetTesterV1       _tester;
    private readonly FakeZkRepository _repo = new FakeZkRepository();

    private readonly string username = "myuser@userdomain.com";
    private readonly string password = "cc3a6a12-0e5b-47fb-ae45-3485e34582d4";

    private readonly TcpHost   _tcphost;
    private readonly IPAddress _ipAddress;
    private const    int       Port = 8098;

    private IPEndPoint CreateEndPoint()
    {
      return new IPEndPoint(_ipAddress, Port);
    }

    private TcpZkEndPoint CreateZkClientEndPoint()
    {
      return new TcpZkEndPoint(username, password, new IPEndPoint(_ipAddress, Port));
    }

    [Fact]
    public async Task CalculateAsyncTest()
    {
      var rnd = new Random();

      var a = rnd.Next(0, 100);
      var b = rnd.Next(0, 100);

      using (var clientProxy = new TcpClient<INetTesterV1>(CreateZkClientEndPoint()))
      {
        var result = await clientProxy.Proxy.CalculateAsync(a, b);
        Assert.Equal(a + b, result);
      }
    }

    [Fact]
    public void ResponseParallelTest()
    {
      Random rnd = new Random(DateTime.Now.Millisecond);
      using (var clientProxy = new TcpClient<INetTesterV1>(CreateZkClientEndPoint()))
        Parallel.For(0, 12, (index, state) =>
        {
          const int count = 50;
          const int start = 0;

          var result = clientProxy.Proxy.Range(start, count);

          for (var i = start; i < count; i++)
          {
            int temp;
            if (result.TryGetValue(i, out temp))
            {
              if (i != temp) state.Break();
              Assert.Equal(i, temp);
            }
            else
            {
              state.Break();
              Assert.True(false);
            }
          }
        });
    }

    [Fact]
    public void ResponseZkTest()
    {
      using (var clientProxy = new TcpClient<INetTesterV1>(CreateZkClientEndPoint()))
      {
        const int count = 50;
        const int start = 0;

        var result = clientProxy.Proxy.Range(start, count);

        for (var i = start; i < count; i++)
        {
          int temp;
          if (result.TryGetValue(i, out temp))
            Assert.Equal(i, temp);
          else
            Assert.True(false);
        }
      }
    }

    [Fact]
    public void SimpleParallelZkTest()
    {
      var rnd = new Random();
      using (var clientProxy = new TcpClient<INetTesterV1>(CreateZkClientEndPoint()))
        Parallel.For(0, 12, (index, state) =>
        {
          var a = rnd.Next(0, 100);
          var b = rnd.Next(0, 100);

          var result = clientProxy.Proxy.Min(a, b);

          if (Math.Min(a, b) != result)
          {
            state.Break();
            Assert.Equal(Math.Min(a, b), result);
          }
        });
    }

    [Fact]
    public void SimpleZkTest()
    {
      var rnd = new Random();

      var a = rnd.Next(0, 100);
      var b = rnd.Next(0, 100);

      using (var clientProxy = new TcpClient<INetTesterV1>(CreateZkClientEndPoint()))
      {
        var result = clientProxy.Proxy.Min(a, b);

        Assert.Equal<int>(Math.Min(a, b), result);
      }
    }
  }
}
