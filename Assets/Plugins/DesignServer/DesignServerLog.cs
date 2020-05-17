using System;
using System.Collections.Generic;
using System.Text;

namespace DesignServer
{
	public class DesignServerLog
	{
		public static void Log(string message)
		{
#if SERVER
			System.Console.WriteLine(message);
#else 
			UnityEngine.Debug.Log(message);
#endif
		}
	}
}
