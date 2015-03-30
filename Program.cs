using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using IOCPServer;

namespace IOCPServer
{
    class Test
    {
        static void Main(string[] args)
        {
            IOCPServer server = new IOCPServer(8088, 1024, @"webapps");
            server.Start();
            Console.WriteLine("服务器已启动....");
            System.Console.ReadLine();
        }
    }
}
