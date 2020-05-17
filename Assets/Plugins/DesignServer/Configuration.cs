#if SERVER
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesignServer
{
	public class Configuration
	{
		public static string DatabaseConnector;
		public static int Port;

		static Configuration()
		{
			var config = JObject.Parse(System.IO.File.ReadAllText("Configuration.json"));
			Port = int.Parse(config["Port"]?.ToString() ?? "9801");
			DatabaseConnector = config["Database"]?.ToString() ?? "";
		}
	}
}
#endif