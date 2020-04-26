using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Logger
{
	public static Level LogLevel = Level.WARNING;

	private static Thread WriterThread;
	private static string Cache;
	private static object CacheLock = new object();
	private static bool Closing = false;
	private static string Path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "log.txt");
	
	static Logger()
	{
		System.IO.File.Delete(Path);
		WriterThread = new Thread(() => {
			while (true)
			{
				if (Closing)
					break;
				string cache = null;
				lock (CacheLock)
				{
					cache = Cache;
					Cache = "";
				}
				System.IO.File.AppendAllText(Path, cache);
				Thread.Sleep(1000);
			}
		});
		WriterThread.Start();
		UnityEngine.Application.wantsToQuit += Application_wantsToQuit;
	}

	private static bool Application_wantsToQuit()
	{
		lock (CacheLock)
		{
			Closing = true;
			System.IO.File.AppendAllText(Path, Cache);
		}
		return true;
	}

	public enum Level
	{
		TRACE = 0,
		DEBUG = 1,
		INFO = 2,
		WARNING = 3,
		ERROR = 4,
		EXCEPTION = 5
	}

	public static void Log(Level level, string text)
	{
		if (level >= LogLevel)
		{
			var msg = "[" + level.ToString() + "] " + text + "\r\n";
			lock (CacheLock)
			{
				Cache += msg;
#if UNITY_EDITOR
				if (level == Level.WARNING)
					UnityEngine.Debug.LogWarning(msg);
				else if (level == Level.ERROR || level == Level.EXCEPTION)
					UnityEngine.Debug.LogError(msg);
				else
					UnityEngine.Debug.Log(msg);
#endif
			}
		}
	}
}

