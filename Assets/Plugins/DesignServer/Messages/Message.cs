using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DesignServer.Messages
{
	public unsafe class Message
	{
		private static Dictionary<byte, Type> MessageTypes = new Dictionary<byte, Type>();
		private static Dictionary<Type, byte> TypeIDs = new Dictionary<Type, byte>();
		protected Connection Connection;

		public byte TransferID;

		public byte ID
		{
			get
			{
				if (TypeIDs.ContainsKey(this.GetType()))
					return TypeIDs[this.GetType()];
				return (byte) 0x00;
			}
		}

		static Message()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (typeof(Message).IsAssignableFrom(type))
					{
						var attributes = type.GetCustomAttributes(typeof(MessageID), false);
						if (attributes.Length >= 1)
						{
							MessageTypes.Add((attributes[0] as MessageID).ID, type);
							TypeIDs.Add(type, (attributes[0] as MessageID).ID);
						}
					}
				}
			}
		}

		public static Message ParseMessage(Connection connection, byte[] data)
		{
			var id = data[4];
			var transferID = data[5];
			if (MessageTypes.ContainsKey(id))
			{
				return (Message) Activator.CreateInstance(MessageTypes[id], new object[] { connection, data });
			}
			return null;
		}

		public Message()
		{

		}

		public Message(Connection connection, byte[] data)
		{
			Connection = connection;
			TransferID = data[5];
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var ptr = (byte*) handle.AddrOfPinnedObject().ToPointer();
			BinaryData binaryData = new BinaryData();
			binaryData.Data = ptr;
			binaryData.Size = data.Length;
			ParseMessage(binaryData);
			handle.Free();
			data = null;
		}

		protected virtual void ParseMessage(BinaryData binaryData)
		{

		}

		public virtual byte[] WriteMessage()
		{
			return null;
		}
	}
}
