using System;
using System.Collections.Generic;
using System.Text;

namespace DesignServer.Messages
{
	public class MessageID : System.Attribute
	{
		public byte ID;
		public MessageID(byte messageID)
		{
			this.ID = messageID;
		}
	}
}
