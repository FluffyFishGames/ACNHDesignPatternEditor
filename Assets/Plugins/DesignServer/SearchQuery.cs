using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if SERVER
using MySql.Data.MySqlClient;
#endif

namespace DesignServer
{
	public class SearchQuery
	{
		public string Phrase = "";
		public byte Type = 0xFF;
		public List<string> Tags = new List<string>();
		public string Creator = "";
		public int Page = 0;
		public string Code = "";
		public bool ProDesigns = false;
		private const int PerPage = 40;

		public class Results
		{
			public List<Pattern> Patterns = new List<Pattern>();
			public int Pages = 0;
			public int Count = 0;

			public void Write(BinaryWriter writer)
			{
				writer.Write((int) Count);
				writer.Write((int) Pages);
				writer.Write((byte) Patterns.Count);
				for (int i = 0; i < Patterns.Count; i++)
				{
					Patterns[i].Write(writer);
				}
			}

			public static Results Read(BinaryData binaryData, ref int offset)
			{
				var results = new SearchQuery.Results();
				results.Count = (int) binaryData.ReadU32(offset);
				offset += 4;
				results.Pages = (int) binaryData.ReadU32(offset);
				offset += 4;
				int count = binaryData.ReadU8(offset);
				offset += 1;
				for (int i = 0; i < count; i++)
				{
					results.Patterns.Add(Pattern.Read(binaryData, ref offset));
				}
				return results;
			}
		}

		public static SearchQuery Read(BinaryData binaryData, ref int offset)
		{
			var query = new SearchQuery();
			byte flags = binaryData.ReadU8(offset);
			bool searchByName = (flags & 0x01) == 0x01;
			bool searchByType = (flags & 0x02) == 0x02;
			bool searchByCreator = (flags & 0x04) == 0x04;
			bool searchByTags = (flags & 0x08) == 0x08;
			bool searchByCode = (flags & 0x10) == 0x10;
			query.ProDesigns = (flags & 0x20) == 0x20;
			offset += 1;
			query.Page = binaryData.ReadS32(offset);
			offset += 4;
			if (searchByName)
			{
				byte phraseLength = binaryData.ReadU8(offset);
				offset += 1;
				query.Phrase = binaryData.ReadString(offset, phraseLength);
				offset += phraseLength * 2;
			}
			if (searchByType)
			{
				query.Type = binaryData.ReadU8(offset);
				offset += 1;
			}
			if (searchByCreator)
			{
				byte creatorLength = binaryData.ReadU8(offset);
				offset += 1;
				query.Creator = binaryData.ReadString(offset, creatorLength);
				offset += creatorLength * 2;
			}
			if (searchByTags)
			{
				byte tagsCount = binaryData.ReadU8(offset);
				offset += 1;
				for (int i = 0; i < tagsCount; i++)
				{
					byte tagLength = binaryData.ReadU8(offset);
					offset += 1;
					query.Tags.Add(binaryData.ReadString(offset, tagLength));
					offset += tagLength * 2;
				}
			}
			if (searchByCode)
			{
				byte codeLength = binaryData.ReadU8(offset);
				offset += 1;
				query.Code = binaryData.ReadString(offset, codeLength);
				offset += codeLength * 2;
			}
			return query;
		}

		public void Write(BinaryWriter writer)
		{
			byte flags = 0;
			if (Phrase != null && Phrase != "") flags |= 0x01;
			if (Type != 0xFF) flags |= 0x02;
			if (Tags.Count > 0) flags |= 0x04;
			if (Creator != null && Creator != "") flags |= 0x08;
			if (Code != null && Code != "") flags |= 0x10;
			if (ProDesigns) flags |= 0x20;

			writer.Write(flags);
			writer.Write(Page);
			if (Phrase != null && Phrase != "")
			{
				byte[] phraseBytes = System.Text.Encoding.Unicode.GetBytes(Phrase);
				writer.Write((byte) (phraseBytes.Length / 2));
				writer.Write(phraseBytes);
			}
			if (Type != 0xFF)
			{
				writer.Write(Type);
			}
			if (Tags.Count > 0)
			{
				writer.Write((byte) Tags.Count);
				for (int i = 0; i < Tags.Count; i++)
				{
					byte[] tagBytes = System.Text.Encoding.Unicode.GetBytes(Tags[i]);
					writer.Write((byte) (tagBytes.Length / 2));
					writer.Write(tagBytes);
				}
			}
			if (Creator != null && Creator != "")
			{
				byte[] creatorBytes = System.Text.Encoding.Unicode.GetBytes(Creator);
				writer.Write((byte) (creatorBytes.Length / 2));
				writer.Write(creatorBytes);
			}
			if (Code != null && Code != "")
			{
				byte[] codeBytes = System.Text.Encoding.Unicode.GetBytes(Code);
				writer.Write((byte) (codeBytes.Length / 2));
				writer.Write(codeBytes);
			}
		}

#if SERVER
		public Results GetResults()
		{
			var results = new Results();
			var connection = new MySqlConnection(Configuration.DatabaseConnector);
			connection.Open();
			var command = connection.CreateCommand();
			List<(string, object)> @params = new List<(string, object)>();
			string sql = "SELECT designs.*, GROUP_CONCAT(DISTINCT tags.Tag) as Tags FROM designs";
			if (Tags.Count > 0)
			{
				sql += " LEFT JOIN tags AS tagSearch ON (tagSearch.DesignID = designs.ID AND tagSearch.Tag IN (";
				for (int i = 0; i < Tags.Count; i++)
				{
					sql += (i > 0 ? "," : "") + "?Tag" + i;
					@params.Add(("?tag" + i, Tags[i]));
				}
				sql += "))";
			}
			sql += " LEFT JOIN tags AS tags ON (tags.DesignID = designs.ID)";
			if ((Phrase != null && Phrase != "") ||
				(Type > 0x00) || 
				(Creator != null && Creator != ""))
			{
				sql += " WHERE";
				sql += " designs.type " + (ProDesigns ? " > 0" : " = 0");
				if (Phrase != null && Phrase != "")
				{
					sql += " AND designs.name LIKE ?phrase";
					@params.Add(("?phrase", "%" + this.Phrase + "%"));
				}
				if (Type != 0xFF)
				{
					sql += " AND designs.type = ?type";
					@params.Add(("?type", this.Type));
				}
				if (Creator != null && Creator != "")
				{
					sql += " AND designs.creator LIKE ?creator";
					@params.Add(("?creator", this.Creator));
				}
				if (Code != null && Code != "")
				{
					sql += " AND designs.code = ?code";
					@params.Add(("?code", this.Code));
				}
			}

			sql += " GROUP BY designs.ID";
			if (Tags.Count > 0)
				sql += " HAVING COUNT(DISTINCT tagSearch.Tag) = " + Tags.Count;

			sql += " LIMIT " + (Page * PerPage) + ", " + PerPage;
			System.Console.WriteLine(sql);
			command.CommandText = sql;
			foreach (var item in @params)
				command.Parameters.AddWithValue(item.Item1, item.Item2);

			var reader = command.ExecuteReader();
			while (reader.Read())
			{
				results.Patterns.Add(Pattern.Read(reader));
			}
			reader.Close();
			reader.Dispose();
			command.Dispose();

			sql = "SELECT COUNT(t.ID) as c FROM (SELECT designs.ID FROM designs";
			if (Tags.Count > 0)
			{
				sql += " LEFT JOIN tags AS tagSearch ON (tagSearch.DesignID = designs.ID AND tagSearch.Tag IN (";
				for (int i = 0; i < Tags.Count; i++)
				{
					sql += (i > 0 ? "," : "") + "?Tag" + i;
				}
				sql += "))";
			}
			if ((Phrase != null && Phrase != "") ||
				(Type > 0x00) ||
				(Creator != null && Creator != ""))
			{
				sql += " WHERE";
				sql += " designs.type " + (ProDesigns ? " > 0" : " = 0");
				if (Phrase != null && Phrase != "")
					sql += " designs.name LIKE ?phrase";
				if (Type != 0xFF)
					sql += " AND designs.type = ?type";
				if (Creator != null && Creator != "")
					sql += " AND designs.creator LIKE ?creator";
				if (Code != null && Code != "")
					sql += " AND designs.code = ?code";
			}

			if (Tags.Count > 0)
			{
				sql += " GROUP BY designs.ID";
				sql += " HAVING COUNT(tagSearch.DesignID) = " + Tags.Count;
			}
			sql += ") AS t";

			System.Console.WriteLine(sql);
			command = connection.CreateCommand();
			command.CommandText = sql;
			foreach (var item in @params)
				command.Parameters.AddWithValue(item.Item1, item.Item2);

			var count = (int) (long) command.ExecuteScalar();
			command.Dispose();
			connection.Close();
			connection.Dispose();

			results.Count = count;
			results.Pages = ((count - 1) / PerPage) + 1;
			return results;
		}
#endif
	}
}
