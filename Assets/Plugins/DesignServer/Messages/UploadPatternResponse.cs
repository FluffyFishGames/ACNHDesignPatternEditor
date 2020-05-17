using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DesignServer.Messages
{
	[MessageID(0x82)]
	public unsafe class UploadPatternResponse : Message
	{
		public Pattern Pattern;
		public UploadPatternResponse(Connection connection, byte[] data) : base(connection, data) { }
		public UploadPatternResponse(Pattern pattern) : base() { Pattern = pattern; }

#if SERVER
		public override byte[] WriteMessage()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryWriter writer = new BinaryWriter(stream);
				Pattern.Write(writer);
				return stream.ToArray();
			}
		}
#endif

		protected override unsafe void ParseMessage(BinaryData binaryData)
		{
			int offset = 6;
			Pattern = Pattern.Read(binaryData, ref offset);
		}
	}
}
