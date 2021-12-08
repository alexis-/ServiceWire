﻿namespace ServiceWireTestClient1
{
  using System;
  using System.Configuration;
  using System.Diagnostics;
  using System.Net;
  using System.Threading;
  using System.Threading.Tasks;
  using ServiceWire.NamedPipes;
  using ServiceWire.TcpIp;
  using ServiceWireTestCommon;

  internal class Program
  {
    #region Methods

    private static void Main(string[] args)
    {
      Thread.Sleep(1000);

      var ip         = ConfigurationManager.AppSettings["ip"];
      var port       = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
      var ipEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
      for (int i = 0; i < 1; i++) RunTest(ipEndpoint, ip);

      Console.ReadLine();
    }

    private static void RunTest(IPEndPoint ipEndpoint, string ip)
    {
      using (var client = new TcpClient<IValTypes>(ipEndpoint))
      {
        decimal abc    = client.Proxy.GetDecimal(4.5m);
        bool    result = client.Proxy.OutDecimal(abc);
      }

      var tcpEndpoint = new TcpEndPoint(ipEndpoint, 80);

      //using (var client = new NetTcpTesterProxy(ipEndpoint))
      using (var client = new NetTcpTesterProxy(tcpEndpoint))
      {
        var  id            = client.GetId("test1", 3.314, 42, DateTime.Now);
        long q             = 3;
        var  response      = client.Get(id, "mirror", 4.123, out q);
        var  list          = client.GetItems(id);
        var  listFromAsync = client.GetItemsAsync(id).GetAwaiter().GetResult();
      }

      using (var client = new NetTcpMyTesterProxy(ipEndpoint))
      {
        var id       = client.GetId("test1", 3.314, 42);
        int q2       = 4;
        var response = client.Get(id, "mirror", 4.123, out q2);
        var list     = client.GetItems(id, new int[] { 3, 6, 9 });
      }

      var sw   = Stopwatch.StartNew();
      var from = 0;
      var to   = 200;
      Parallel.For(from, to, index =>
      {
        using (var client = new NetTcpTesterProxy(ipEndpoint))
          for (int i = 0; i < 10; i++)
          {
            var  id            = client.GetId("test1", 3.314, 42, DateTime.Now);
            long q             = 2;
            var  response      = client.Get(id, "mirror", 4.123, out q);
            var  list          = client.GetItems(id);
            var  listFromAsync = client.GetItemsAsync(id).GetAwaiter().GetResult();
          }

        using (var client = new NetTcpMyTesterProxy(ipEndpoint))
          for (int i = 0; i < 10; i++)
          {
            var id       = client.GetId("test1", 3.314, 42);
            int q2       = 6;
            var response = client.Get(id, "mirror", 4.123, out q2);
            var list     = client.GetItems(id, new int[] { 3, 6, 9 });
          }
      });
      sw.Stop();
      var msperop = sw.ElapsedMilliseconds / 12000.0;
      Console.WriteLine("tcp: {0}, {1}", sw.ElapsedMilliseconds, msperop);

      if ("127.0.0.1" == ip) //only run np tests when testing locally
      {
        var pipeName = "ServiceWireTestHost";
        using (var client = new NetNpTesterProxy(new NpEndPoint(pipeName)))
        {
          var  id            = client.GetId("test1", 3.314, 42, DateTime.Now);
          long q             = 2;
          var  response      = client.Get(id, "mirror", 4.123, out q);
          var  list          = client.GetItems(id);
          var  listFromAsync = client.GetItemsAsync(id).GetAwaiter().GetResult();
        }

        sw = Stopwatch.StartNew();
        Parallel.For(from, to, index =>
        {
          using (var client = new NetNpTesterProxy(new NpEndPoint(pipeName)))
            for (int i = 0; i < 10; i++)
            {
              var  id            = client.GetId("test1", 3.314, 42, DateTime.Now);
              long q             = 4;
              var  response      = client.Get(id, "mirror", 4.123, out q);
              var  list          = client.GetItems(id);
              var  listFromAsync = client.GetItemsAsync(id).GetAwaiter().GetResult();

              long id1;
              long id2;
              long id3 = client.TestLong(out id1, out id2);
            }

          using (var client = new NetNpMyTesterProxy(new NpEndPoint(pipeName)))
            for (int i = 0; i < 10; i++)
            {
              var id       = client.GetId("test1", 3.314, 42);
              int q2       = 4;
              var response = client.Get(id, "mirror", 4.123, out q2);
              var list     = client.GetItems(id, new int[] { 3, 6, 9 });
            }
        });
        sw.Stop();
        msperop = sw.ElapsedMilliseconds / 12000.0;
        Console.WriteLine("pip: {0}, {1}", sw.ElapsedMilliseconds, msperop);
        Thread.Sleep(2000);
      }
    }

    #endregion
  }
}
