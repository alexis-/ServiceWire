namespace ServiceWireTests.Tests
{
  using System;
  using System.Threading.Tasks;
  using ServiceWire;
  using ServiceWire.Common;
  using TestServices;
  using Xunit;

  public abstract class CommonTests : IDisposable
  {
    #region Properties & Fields - Non-Public

    private readonly INetTesterV1 _testerV1;
    private readonly INetTesterV2 _testerV2;

    protected Host _host;

    #endregion




    #region Constructors

    protected CommonTests()
    {
      _testerV1 = new NetTesterV1(ReverseEndPointCallback);
      _testerV2 = new NetTesterV2(ReverseEndPointCallback);

      _host = CreateAndStartHost(0, _testerV1);
    }

    public void Dispose()
    {
      _host.Stop();
    }

    #endregion




    #region Methods Abs

    public abstract EndPointBase CreateEndPoint(int clientId);

    public abstract Host CreateHost(int hostId, ISerializer serializer = null);

    #endregion




    #region Methods

    private BaseClient<T> CreateDefaultClient<T>()
      where T : class
    {
      return CreateEndPoint(0).CreateClient<T>();
    }

    private Host CreateAndStartHost<T>(int hostId, T svc, ISerializer serializer = null)
      where T : class
    {
      var host = CreateHost(hostId, serializer);

      host.AddService<T>(svc);
      host.Start();

      return host;
    }

    private bool ReverseEndPointCallback(EndPointBase ep)
    {
      try
      {
        CalculateAsyncTest(ep.CreateClient<INetTesterV1>()).Wait();
        return true;
      }
      catch
      {
        return false;
      }
    }

    [Fact]
    public void SimpleVersioningTest()
    {
      var hostV2 = CreateAndStartHost(1, _testerV2);

      var rnd = new Random();
      var a   = rnd.Next(0, 100);
      var b   = rnd.Next(0, 100);

      var epV1 = CreateEndPoint(0);
      var epV2 = CreateEndPoint(1);

      using var clientV1ToV2 = epV2.CreateClient<INetTesterV1>();
      using var clientV2ToV2 = epV2.CreateClient<INetTesterV2>();

      var intRes = clientV2ToV2.Proxy.Max(a, b);
      Assert.Equal(Math.Max(a, b), intRes);

      intRes = clientV1ToV2.Proxy.Min(a, b);
      Assert.Equal(Math.Min(a, b), intRes);
      //Assert.Throws
    }

    [Theory]
    [InlineData(null)]
    public async Task CalculateAsyncTest(BaseClient<INetTesterV1> client)
    {
      var rnd = new Random();

      var a = rnd.Next(0, 100);
      var b = rnd.Next(0, 100);

      client ??= CreateDefaultClient<INetTesterV1>();

      var result = await client.Proxy.CalculateAsync(a, b);
      Assert.Equal(a + b, result);

      client.Dispose();
    }


    [Fact]
    public void ResponseParallelTest()
    {
      Parallel.For(0, 4, (_, state) =>
      {
        using (var client = CreateDefaultClient<INetTesterV1>())
        {
          const int count = 50;
          const int start = 0;

          var result = client.Proxy.Range(start, count);
          for (var i = start; i < count; i++)
          {
            if (result.TryGetValue(i, out int temp))
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
        }
      });
    }

    [Fact]
    public void ResponseTest()
    {
      using var client = CreateDefaultClient<INetTesterV1>();

      const int count = 50;
      const int start = 0;

      var result = client.Proxy.Range(start, count);

      for (var i = start; i < count; i++)
      {
        Assert.True(result.TryGetValue(i, out int temp));
        Assert.Equal(i, temp);
      }
    }

    [Fact]
    public void ResponseWithOutParameterNewtonsoftSerializerTest()
    {
      using var jsonHost = CreateAndStartHost(1, _testerV1, new NewtonsoftSerializer());

      using var client = CreateEndPoint(1).CreateClient<INetTesterV1>(new NewtonsoftSerializer());

      var result = client.Proxy.Get(Guid.NewGuid(), "SomeLabel", 45.65, out int quantity);

      Assert.NotEqual(default, result);
      Assert.Equal("MyLabel", result.Label);
      Assert.Equal(44, quantity);
    }

    [Fact]
    public void ResponseWithOutParameterTest()
    {
      using var client = CreateDefaultClient<INetTesterV1>();

      var result = client.Proxy.Get(Guid.NewGuid(), "SomeLabel", 45.65, out int quantity);

      Assert.NotEqual(default, result);
      Assert.Equal("MyLabel", result.Label);
      Assert.Equal(44, quantity);
    }

    [Fact]
    public void SerializeBaseEndPointTest()
    {
      var reverseEndPoint = CreateEndPoint(1);
      using var reverseHost = CreateAndStartHost(1, _testerV1);

      using var client = CreateDefaultClient<INetTesterV1>();
      var       result = client.Proxy.SetReverseEndPoint(reverseEndPoint);

      Assert.True(result);
    }

    [Fact]
    public void SimpleNewtonsoftSerializerTest()
    {
      using var jsonHost = CreateAndStartHost(1, _testerV1, new NewtonsoftSerializer());

      var rnd = new Random();

      var a = rnd.Next(0, 100);
      var b = rnd.Next(0, 100);

      using var client = CreateEndPoint(1).CreateClient<INetTesterV1>(new NewtonsoftSerializer());

      var result = client.Proxy.Min(a, b);

      Assert.Equal(Math.Min(a, b), result);
    }

    [Fact]
    public void SimpleParallelTest()
    {
      var rnd = new Random();

      Parallel.For(0, 4, (_, state) =>
      {
        var a = rnd.Next(0, 100);
        var b = rnd.Next(0, 100);

        using (var client = CreateDefaultClient<INetTesterV1>())
        {
          var result = client.Proxy.Min(a, b);

          if (Math.Min(a, b) != result)
          {
            state.Break();
            Assert.Equal(Math.Min(a, b), result);
          }
        }
      });
    }

    [Fact]
    public void SimpleProtoBufSerializerTest()
    {
      using var protoBufHost = CreateAndStartHost(1, _testerV1, new ProtoBufSerializer());

      var rnd = new Random();

      var a = rnd.Next(0, 100);
      var b = rnd.Next(0, 100);

      using var client = CreateEndPoint(1).CreateClient<INetTesterV1>(new ProtoBufSerializer());

      var result = client.Proxy.Min(a, b);

      Assert.Equal(Math.Min(a, b), result);
    }

    [Fact]
    public void SimpleTest()
    {
      var rnd = new Random();

      var a = rnd.Next(0, 100);
      var b = rnd.Next(0, 100);

      using (var client = CreateDefaultClient<INetTesterV1>())
      {
        var result = client.Proxy.Min(a, b);

        Assert.Equal(Math.Min(a, b), result);
      }
    }

    #endregion
  }
}
