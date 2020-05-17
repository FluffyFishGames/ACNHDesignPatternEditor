using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DesignServer.Messages
{
	[MessageID(0x81)]
	public unsafe class ListPatternsResponse : Message
	{
		public ListPatternsResponse(Connection connection, byte[] data) : base(connection, data) { }
		public ListPatternsResponse(SearchQuery.Results results) : base() { Results = results; }
		public SearchQuery.Results Results;

#if SERVER
		public override byte[] WriteMessage()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write((int) Results.Count);
				writer.Write((int) Results.Pages);
				writer.Write((byte) Results.Patterns.Count);
				for (int i = 0; i < Results.Patterns.Count; i++)
				{
					Results.Patterns[i].Write(writer);
				}
				return stream.ToArray();
			}
		}
#endif

		protected override unsafe void ParseMessage(BinaryData binaryData)
		{
			Results = new SearchQuery.Results();
			Results.Count = (int) binaryData.ReadU32(6);
			Results.Pages = (int) binaryData.ReadU32(10);
			int count = binaryData.ReadU8(14);
			int offset = 15;
			for (int i = 0; i < count; i++)
			{
				Results.Patterns.Add(Pattern.Read(binaryData, ref offset));
			}
		}
	}
}
