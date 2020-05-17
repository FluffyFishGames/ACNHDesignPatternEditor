using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DesignServer.Messages
{
	[MessageID(0x02)]
	public unsafe class UploadPattern : Message
	{
		public Pattern Pattern;
		public UploadPattern(Connection connection, byte[] data) : base(connection, data) { }
		public UploadPattern(Pattern pattern) : base() { Pattern = pattern; }

#if SERVER
		protected override unsafe void ParseMessage(BinaryData binaryData)
		{
			int currentIndex = 6;
			Pattern = Pattern.Read(binaryData, ref currentIndex);
			Pattern.Save();

			var response = new UploadPatternResponse(Pattern);
			response.TransferID = this.TransferID;
			Connection.SendMessage(response);
		}
#endif

		public override byte[] WriteMessage()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryWriter writer = new BinaryWriter(stream);
				Pattern.Write(writer);
				return stream.ToArray();
			}
		}
	}
}
