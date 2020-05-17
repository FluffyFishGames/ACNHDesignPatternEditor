#if SERVER
using MySql.Data.MySqlClient;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DesignServer
{
	public class Pattern
	{
		public static int CurrentID = 0;
		private static string CodeRoundRobin = "ZM35ETYDJ9HPGQX8VAON762WCKU4BRSF";
		private static ulong CodeCipher = 0x188FED19C19E8312;
		private static int[] Shifts = new int[] { 3, 15, 14, 0, 19, 22, 27, 21, 11, 25, 30, 24, 12, 13, 31, 1, 28, 2, 18, 4, 20, 5, 7, 23, 8, 9, 26, 10, 16, 29, 6, 17 };

		public uint ID;
		public string Name;
		public string Creator;
		public string[] Tags;
		public string Code;
		public byte Type;
		public byte[] Bytes;

		static Pattern()
		{
#if SERVER
			var connection = new MySqlConnection(Configuration.DatabaseConnector);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "SELECT MAX(id) FROM designs";
			CurrentID = (int) (uint) command.ExecuteScalar();
			command.Dispose();
			connection.Close();
			connection.Dispose();
#endif
		}

		public static Pattern Read(BinaryData binaryData, ref int offset)
		{
			var pattern = new Pattern();
			pattern.ID = binaryData.ReadU32(offset);
			offset += 4;
			byte codeLength = binaryData.ReadU8(offset);
			offset += 1;
			pattern.Code = binaryData.ReadString(offset, codeLength);
			offset += codeLength * 2;
			byte nameLength = binaryData.ReadU8(offset);
			offset += 1;
			pattern.Name = binaryData.ReadString(offset, nameLength);
			offset += nameLength * 2;
			byte creatorLength = binaryData.ReadU8(offset);
			offset += 1;
			pattern.Creator = binaryData.ReadString(offset, creatorLength);
			offset += creatorLength * 2;

			byte tagsCount = binaryData.ReadU8(offset);
			offset += 1;
			List<string> tags = new List<string>();
			for (int i = 0; i < tagsCount; i++)
			{
				byte tagLength = binaryData.ReadU8(offset);
				offset += 1;
				string tag = binaryData.ReadString(offset, tagLength);
				offset += tagLength * 2;
				tags.Add(tag);
			}

			pattern.Tags = tags.ToArray();

			pattern.Type = binaryData.ReadU8(offset);
			offset += 1;
			ushort len = binaryData.ReadU16(offset);
			offset += 2;
			pattern.Bytes = binaryData.ReadBytes(offset, len);
			offset += len;
			return pattern;
		}

		public static string GenerateCode(uint ID)
		{
			ulong res = 0;
			int demoninator = 1;
			for (int j = 0; j < 32; j++)
			{
				res += (ulong) (((ID & demoninator) >> j) << ((Shifts[j]) * 2)) ^ (CodeCipher * ID);
				demoninator *= 2;
			}

			string finalCode = "";
			for (int j = 0; j < 12; j++)
			{
				var index = (int) ((res >> (j * 6)) & 0x3F) % CodeRoundRobin.Length;
				finalCode += CodeRoundRobin[index];
				if (j == 3 || j == 7)
					finalCode += "-";
			}

			return finalCode;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(ID);
			var codeBytes = System.Text.Encoding.Unicode.GetBytes(Code);
			writer.Write((byte) (codeBytes.Length / 2));
			writer.Write(codeBytes);
			var nameBytes = System.Text.Encoding.Unicode.GetBytes(Name);
			writer.Write((byte) (nameBytes.Length / 2));
			writer.Write(nameBytes);
			var creatorBytes = System.Text.Encoding.Unicode.GetBytes(Creator);
			writer.Write((byte) (creatorBytes.Length / 2));
			writer.Write(creatorBytes);
			writer.Write((byte) Tags.Length);
			for (int j = 0; j < Tags.Length; j++)
			{
				var tagBytes = System.Text.Encoding.Unicode.GetBytes(Tags[j]);
				writer.Write((byte) (tagBytes.Length / 2));
				writer.Write(tagBytes);
			}
			writer.Write(Type);
			if (Bytes == null)
			{
				writer.Write((ushort) 0);
			}
			else
			{
				writer.Write((ushort) Bytes.Length);
				writer.Write(Bytes);
			}
		}


#if SERVER
		public static Pattern Read(MySql.Data.MySqlClient.MySqlDataReader reader)
		{
			var pattern = new Pattern()
			{
				ID = reader.GetUInt32("ID"),
				Name = reader.GetString("Name"),
				Creator = reader.GetString("Creator"),
				Tags = reader.GetString("Tags").Split(',', StringSplitOptions.RemoveEmptyEntries),
				Code = reader.GetString("Code"),
				Type = reader.GetByte("Type"),
			};

			string folder = System.IO.Path.Combine(pattern.Code.Substring(0, 2), pattern.Code.Substring(2, 2));
			if (System.IO.Directory.Exists(folder))
			{
				string file = System.IO.Path.Combine(folder, pattern.Code + ".acnh");
				if (System.IO.File.Exists(file))
					pattern.Bytes = System.IO.File.ReadAllBytes(file);
			}
			return pattern;
		}

		public void Save()
		{
			if (ID > 0) return;
			ID = (uint) Interlocked.Increment(ref CurrentID);
			Code = GenerateCode(ID);
			var connection = new MySqlConnection(Configuration.DatabaseConnector);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "INSERT INTO designs(ID, Name, Creator, Type) VALUES(?ID, ?Name, ?Creator, ?Type)";
			command.Parameters.AddWithValue("?ID", this.ID);
			command.Parameters.AddWithValue("?Creator", this.Creator);
			command.Parameters.AddWithValue("?Name", this.Name);
			command.Parameters.AddWithValue("?Type", this.Type);
			command.Parameters.AddWithValue("?Code", this.Code);
			command.ExecuteNonQuery();
			command.Dispose();

			for (var i = 0; i < Tags.Length; i++)
			{
				command = connection.CreateCommand();
				command.CommandText = "INSERT INTO tags(DesignID, Tag) VALUES(?DesignID, ?Tag)";
				command.Parameters.AddWithValue("?DesignID", ID);
				command.Parameters.AddWithValue("?Tag", Tags[i]);
				command.ExecuteNonQuery();
				command.Dispose();
			}

			connection.Close();
			connection.Dispose();

			string folder = System.IO.Path.Combine(Code.Substring(0, 2), Code.Substring(2, 2));
			if (!System.IO.Directory.Exists(folder))
				System.IO.Directory.CreateDirectory(folder);
			System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder, Code + ".acnh"), Bytes);
		}
#endif
	}
}
