using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DesignServer.Messages
{
	[MessageID(0x01)]
	public unsafe class ListPatterns : Message
	{
		public SearchQuery Query;
		public ListPatterns(Connection connection, byte[] data) : base(connection, data) { }
		public ListPatterns() : base() { }

		public override byte[] WriteMessage()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryWriter writer = new BinaryWriter(stream);
				Query.Write(writer);
				return stream.ToArray(); 
			}
		}

#if SERVER
		protected override unsafe void ParseMessage(BinaryData binaryData)
		{
			int offset = 6;
			Query = SearchQuery.Read(binaryData, ref offset);

			var msg = new ListPatternsResponse(Query.GetResults());
			msg.TransferID = this.TransferID;
			Connection.SendMessage(msg);
		}
#endif
	}
}
