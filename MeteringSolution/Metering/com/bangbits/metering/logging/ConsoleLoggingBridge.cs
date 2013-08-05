using System;

namespace com.bangbits.metering.logging
{
	public class ConsoleLoggingBridge : ILoggingBridge
	{
		public void trace(string msg, params object[] args)
		{
			Console.WriteLine("TRACE " + GetTimestamp() + msg, args);
		}

		public void debug(string msg, params object[] args)
		{
			Console.WriteLine("DEBUG " + GetTimestamp() + msg, args);
		}

		public void info(string msg, params object[] args)
		{
			Console.WriteLine("INFO " + GetTimestamp() + msg, args);
		}

		public void warn(string msg, params object[] args)
		{
			Console.WriteLine("WARN " + GetTimestamp() + msg, args);
		}

		public void error(string msg, params object[] args)
		{
			Console.WriteLine("ERROR " + GetTimestamp() + msg, args);
		}

		public static String GetTimestamp()
		{
    		return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff ");
		}
	}
}

