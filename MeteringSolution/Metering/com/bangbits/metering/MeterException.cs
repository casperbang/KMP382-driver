using System;

namespace com.bangbits.metering
{
	/// <summary>
	/// Common meter driver exception type.
	/// </summary>
	public class MeterException : Exception
	{
		public MeterException(string msg) : base(msg)
		{
		}
	}
}

