#if !SERVER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DesignServer
{
	public class Client
	{
		public Connection Connection;
		public bool IsConnected = false;
		public string Error;
		private TcpClient TCPClient;
		public Client(IPEndPoint server)
		{
			Thread t = new Thread(() =>
			{
				TCPClient = new TcpClient();
				try
				{
					TCPClient.Connect(server);
					if (TCPClient.Connected)
					{
						Connection = new Connection(TCPClient, this);
					}
					else
					{
						Error = "Could not connect!";
					}
				}
				catch (SocketException e)
				{
					if (e.SocketErrorCode == SocketError.TimedOut)
					{
						Error = "Timeout...";
					}
					else if (e.SocketErrorCode == SocketError.ConnectionAborted)
					{
						Error = "Connection lost...";
					}
					else if (e.SocketErrorCode == SocketError.ConnectionRefused)
					{
						Error = "Could not connect!";
					}
					else if (e.SocketErrorCode == SocketError.Interrupted)
					{
						Error = "Connection interrupted!";
					}
					else if (e.SocketErrorCode == SocketError.HostDown)
					{
						Error = "Host is down!";
					}
					else if (e.SocketErrorCode == SocketError.HostNotFound)
					{
						Error = "Host not found!";
					}
					else if (e.SocketErrorCode == SocketError.HostUnreachable)
					{
						Error = "Host unreachable!";
					}
					else if (e.SocketErrorCode == SocketError.NetworkDown)
					{
						Error = "Network down!";
					}
					else
					{
						Error = e.SocketErrorCode.ToString();
					}
					UnityEngine.Debug.Log(e.SocketErrorCode);
				}
				catch (Exception e)
				{
					UnityEngine.Debug.LogException(e);
					Error = e.Message;
				}
			});
			t.Start();
		}

		public void Close()
		{
			try
			{
				TCPClient.Close();
			}
			catch (Exception e)
			{

			}
			IsConnected = false;
		}
	}
}
#endif