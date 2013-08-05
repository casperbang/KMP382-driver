using System;

namespace com.bangbits.metering.connection
{	
	/// <summary>
	/// An contractual abstraction over a physically connected meter, which is only 
	/// capable of raw binary request-response calls. This interface decribes the 
	/// basic commands an implementation of IMeterConnection needs to contain.
	/// </summary>
	public interface IMeterConnection : IDisposable
	{
		/// <summary>
		/// Gets a value indicating whether this <see cref="com.bangbits.metering.IMeterConnection"/> is open.
		/// </summary>
		/// <value>
		/// <c>true</c> if open; otherwise <c>false</c>.
		/// </value>
		bool IsOpen
		{
			get;
		}
		
		/// <summary>
		/// Close this instance (called by Dispose).
		/// </summary>
		void Close();
		
		byte[] SendCommand(byte[] command);

		int ReadByte();

		string ReadLine();
	}
}

