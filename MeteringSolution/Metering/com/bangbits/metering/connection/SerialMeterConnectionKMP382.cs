using System;
using System.IO.Ports;
using System.Text;
using System.Diagnostics;
using com.bangbits.metering.logging;

namespace com.bangbits.metering.connection
{
	/// <summary>
	/// Serial meter connection for KMP382, using settings of 1200E82 and
	/// LF as line termination character.
	/// 
	/// 1200 baud amounts to a maximum of 150 byte/sec so read timeout
	/// is set to an average value of 400ms.
	/// </summary>
	public class SerialMeterConnectionKMP382: SerialMeterConnection
	{
		public SerialMeterConnectionKMP382 (/*string portName, ILoggingBridge logger) : base(portName, logger*/)
		{
	        BaudRate = 1200;
	        Parity = Parity.Even;
	        DataBits = 8;
	        StopBits = StopBits.Two;
	        Handshake = Handshake.None;
			Encoding = new ASCIIEncoding();
			DtrEnable = true;
			RtsEnable = false;
			ReadTimeout = 600;
			WriteTimeout = 600;
			NewLine = Encoding.ASCII.GetString(new byte[]{SerialMeterConnection.LF});
		}
	}
}

