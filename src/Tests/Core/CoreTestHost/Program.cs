﻿using CoreTestCommon;
using ServiceWire;
using ServiceWire.TcpIp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoreTestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new Logger(logLevel: LogLevel.Debug);
            var stats = new Stats();

            var addr = new[] { "127.0.0.1", "8098" }; //defaults
            if (null != args && args.Length > 0)
            {
                var parts = args[0].Split(':');
                if (parts.Length > 1) addr[1] = parts[1];
                addr[0] = parts[0];
            }

            var ip = addr[0];
            var port = Convert.ToInt32(addr[1]);
            var ipEndpoint = new IPEndPoint(IPAddress.Any, port);

            var useCompression = false;
            var compressionThreshold = 131072; //128KB

            var tester = new NetTester();
            var mytester = new MyTester();

            var tcphost = new TcpHost(ipEndpoint, logger, stats);
            tcphost.UseCompression = useCompression;
            tcphost.CompressionThreshold = compressionThreshold;
            tcphost.AddService<INetTester>(tester);
            tcphost.AddService<IMyTester>(mytester);

            var valTypes = new ValTypes();
            tcphost.AddService<IValTypes>(valTypes);

            tcphost.Open();

            Console.WriteLine("Press Enter to stop the dual host test.");
            Console.ReadLine();

            tcphost.Close();

            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();
        }
    }

    public class ValTypes : IValTypes
    {
        public decimal GetDecimal(decimal input)
        {
            return input += 456.44m;
        }

        public bool OutDecimal(decimal val)
        {
            val = 45.66m;
            return true;
        }
    }

    public class NetTester : INetTester
    {
        public Guid GetId(string source, double weight, int quantity, DateTime dt)
        {
            return Guid.NewGuid();
        }

        public TestResponse Get(Guid id, string label, double weight, out long quantity)
        {
            quantity = 42;
            return new TestResponse { Id = id, Label = "Hello, world.", Quantity = quantity };
        }

        public List<string> GetItems(Guid id)
        {
            var list = new List<string>();
            list.Add("42");
            list.Add(id.ToString());
            list.Add("Test");
            return list;
        }

        public long TestLong(out long id1, out long id2)
        {
            id1 = 23;
            id2 = 24;
            return 25;
        }
    }

    public class MyTester : IMyTester
    {
        private string longLabel = string.Empty;
        private const int totalKilobytes = 140;
        private Random rand = new Random(DateTime.Now.Millisecond);

        public MyTester()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < totalKilobytes; i++)
            {
                for (int k = 0; k < 1024; k++) sb.Append(((char)rand.Next(32, 126)));
            }
            longLabel = sb.ToString();
        }


        public Guid GetId(string source, double weight, int quantity)
        {
            return Guid.NewGuid();
        }

        public TestResponse Get(Guid id, string label, double weight, out int quantity)
        {
            quantity = 44;
            return new TestResponse { Id = id, Label = longLabel, Quantity = quantity };
        }

        public List<string> GetItems(Guid id, int[] vals)
        {
            var list = new List<string>();
            list.Add("42");
            list.Add(id.ToString());
            list.Add("MyTest");
            list.Add(longLabel);
            return list;
        }
    }
}
