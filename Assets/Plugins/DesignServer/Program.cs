#if SERVER
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DesignServer
{
	class Program
	{

		static void Main(string[] args)
		{
			Server server = new Server();
		}
	}
}
#endif