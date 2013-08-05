using System;

namespace com.bangbits.metering.logging
{
	public class NoOpLoggingBridge : ILoggingBridge
	{
		public void trace(string msg, params object[] args)
		{
		}

		public void debug(string msg, params object[] args)
		{
		}

		public void info(string msg, params object[] args)
		{
		}

		public void warn(string msg, params object[] args)
		{
		}

		public void error(string msg, params object[] args)
		{
		}
	}
}

