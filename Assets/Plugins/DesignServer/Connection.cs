using DesignServer.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DesignServer
{
	public class Connection
	{
#if SERVER
		public readonly uint ID;
#else
		public readonly Client Client;
#endif
		private TcpClient TcpClient;
		private BinaryWriter Writer;
		private BinaryReader Reader;
		private NetworkStream Stream;
		private static byte[] HeloBytes = new byte[] { 0xFF, 0xFE, 0x01, 0xFF };
		private Thread ReaderThread;
		private bool Closed = false;
		private const int MAX_MESSAGE_LENGTH = 100000;
		private Dictionary<byte, Action<Message>> MessageCallbacks = new Dictionary<byte, Action<Message>>();
		private byte CurrentTransferID = 0;

		public Connection(TcpClient tcpClient,
#if SERVER
			uint id
#else
			Client client
#endif
			)
		{
			TcpClient = tcpClient;
#if SERVER
			ID = id;
#else
			Client = client;
#endif
			Stream = TcpClient.GetStream();
			Writer = new BinaryWriter(Stream);
			Reader = new BinaryReader(Stream);

			Writer.Write(HeloBytes);
			Writer.Flush();

			var b = Reader.ReadBytes(HeloBytes.Length);
			for (int j = 0; j < HeloBytes.Length; j++)
			{
				if (HeloBytes[j] != b[j])
				{
					Client.Error = "Version outdated.";
					throw new Exception("Protocol wasn't followed. Helo bytes mismatched.");
				}
			}
			Client.IsConnected = true;

			ReaderThread = new Thread(() =>
			{
				MemoryStream currentMessage = new MemoryStream(MAX_MESSAGE_LENGTH);

				while (!Closed)
				{
					try
					{
						if (!tcpClient.Connected)
							break;
						byte[] buffer = new byte[1024];
						int c = Reader.Read(buffer, 0, buffer.Length);
						if (c == 0)
							break;
						currentMessage.Write(buffer, 0, c);
						int messageLength = 0;
						if (currentMessage.Length >= 4)
							messageLength = BitConverter.ToInt32(currentMessage.GetBuffer(), 0);
						if (messageLength > MAX_MESSAGE_LENGTH)
							Close();
						else if (currentMessage.Length >= messageLength)
						{
							byte[] message = new byte[messageLength];
							System.Array.Copy(currentMessage.GetBuffer(), message, messageLength);
							Task.Run(() => {
								try
								{
									var msg = Message.ParseMessage(this, message);
									if (MessageCallbacks.ContainsKey(msg.TransferID))
									{
										MessageCallbacks[msg.TransferID](msg);
										MessageCallbacks.Remove(msg.TransferID);
									}
								}
								catch (System.Exception e)
								{
									UnityEngine.Debug.LogException(e);
								}
							});
							if (currentMessage.Length > messageLength)
							{
								byte[] remainder = new byte[currentMessage.Length - messageLength];
								System.Array.Copy(currentMessage.GetBuffer(), messageLength, remainder, 0, currentMessage.Length - messageLength);
								currentMessage = new MemoryStream();
								currentMessage.Write(remainder, 0, remainder.Length);
							}
							else
							{
								currentMessage = new MemoryStream();
							}
						}
						
					}
					catch (Exception e)
					{
						Closed = true;
					}
				}
			});
			ReaderThread.Start();
		}

		public unsafe void SendMessage(Message msg, System.Action<Message> callback = null)
		{
			if (callback != null)
			{
				if (CurrentTransferID == 255)
					CurrentTransferID = 0;
				else 
					CurrentTransferID++;
				if (CurrentTransferID == 0)
				{
					for (byte i = 0; i < 128; i++)
						MessageCallbacks.Remove(i);
				}
				else if (CurrentTransferID == 128)
				{
					for (byte i = 128; i < 255; i++)
						MessageCallbacks.Remove(i);
				}
				MessageCallbacks.Add(CurrentTransferID, callback);
				msg.TransferID = CurrentTransferID;

			}
			var data = msg.WriteMessage();
			if (data != null)
			{
				byte[] message = new byte[data.Length + 6];
				fixed (byte* messagePtr = &message[0])
				{
					*((int*) (messagePtr)) = (int) message.Length;
					*((byte*) (messagePtr + 4)) = (byte) msg.ID;
					*((byte*) (messagePtr + 5)) = (byte) msg.TransferID;
				}

				System.Buffer.BlockCopy(data, 0, message, 6, data.Length);
				Writer.Write(message);
				Writer.Flush();
			}
		}

		public void Close()
		{
			try
			{
				TcpClient.Close();
			}
			catch (Exception e)
			{

			}
			TcpClient.Dispose();
#if SERVER
			Server.ConnectionClosed(this.ID);
#else 
			Client.Close();
#endif
		}
	}
}
