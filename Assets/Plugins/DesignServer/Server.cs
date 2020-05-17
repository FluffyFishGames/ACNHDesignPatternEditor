#if SERVER
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DesignServer
{

	public class Server
	{
		private static Dictionary<uint, Connection> Connections = new Dictionary<uint, Connection>();
		private static uint CurrentID = 0;

		public Server()
		{
			var endpoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), Configuration.Port);
			TcpListener listener = new TcpListener(endpoint);
			listener.Start();
			System.Console.WriteLine("Listening for incoming connections on port " + Configuration.Port);
			while (true)
			{
				var client = listener.AcceptTcpClient();
				System.Console.WriteLine("New inbound connection");
				CurrentID++;
				Connections.Add(CurrentID, new Connection(client, CurrentID));
			}
		}

		public static void ConnectionClosed(uint ID)
		{
			Connections.Remove(ID);
		}
	}

}
#endif