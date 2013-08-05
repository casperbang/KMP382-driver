using System;
using System.IO.Ports;
using System.Text;
using com.bangbits.metering.logging;

namespace com.bangbits.metering.connection
{
	/// <summary>
	/// Serial meter connection for IEC61107/IEC62056-21, using settings of 7E1@300.
	/// 
	/// 300 baud amounts to a maximum of 37 byte/sec so read timeout
	/// is set to a relatively high 750ms.
	/// </summary>
	public class SerialMeterConnectionIEC61107: SerialMeterConnection
	{
		public SerialMeterConnectionIEC61107 (/*string portName, ILoggingBridge logger) : base(portName, logger*/)
		{
			BaudRate = 300;
        	Parity = Parity.Even;
        	DataBits = 7;
        	StopBits = StopBits.One;
        	Handshake = Handshake.None;
			Encoding = new ASCIIEncoding();
			DtrEnable = false;
			RtsEnable = true; 
			ReadTimeout = 1000;
			WriteTimeout = 1000;
		}
	}
}

