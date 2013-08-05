using System;
using System.IO.Ports;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using com.bangbits.metering.logging;
using com.bangbits.metering.connection;
using com.bangbits.metering.protocol;

namespace com.bangbits.metering
{
	public class MainTest
	{
		/**
		 * Optimizations/improvements/ideas:
		 * - Add synthetic monetary factor (1 kWh = 2Kr etc.)
		 * - Add factory SPI so that we can query for meter provider and use a config file eventually
		 */
		public static void Main (string[] args)
		{
			//talk382_KMP();
			talk382_IEC();
		}


		// Talk to a serially connected 685-382 using its native propriatary KMP protocol
		static void talk382_KMP()
		{
			ILoggingBridge logger = new ConsoleLoggingBridge();

			try
			{
				using(var connection = new SerialMeterConnection{
					PortName = "/dev/ttyUSB0",
			        BaudRate = 1200,
			        Parity = Parity.Even,
			        DataBits = 8,
			        StopBits = StopBits.Two,
			        Handshake = Handshake.None,
					Encoding = new ASCIIEncoding(),
					ReadTimeout = 600,
					WriteTimeout = 600,
					NewLine = Encoding.ASCII.GetString(new byte[]{SerialMeterConnection.LF}),
					Logger = logger
				})
				{
					using(var protocol = new MeterProtocolKMP382(connection, logger))
					{
							foreach(var entry in protocol.Registrations)
							{
								Console.WriteLine (entry.Key + ": " + entry.Value);
							}
					}
				}
			}
			catch(MeterException me)
			{
				logger.error("Failed communicating with meter: " + me.Message);
			}
		}


		// Talk to a serially connected 685-382 using the limited but open IEC61107 protocol
		static void talk382_IEC ()
		{
			ILoggingBridge logger = new ConsoleLoggingBridge ();

			try
			{
				using(var connection = new SerialMeterConnection{
					PortName = "/dev/ttyUSB0",
			        BaudRate = 300,
			        Parity = Parity.Even,
			        DataBits = 7,
			        StopBits = StopBits.One,
			        Handshake = Handshake.None,
					DtrEnable = false,
					RtsEnable = true,
					Encoding = new ASCIIEncoding(),
					ReadTimeout = 700,
					WriteTimeout = 400,
					NewLine = Encoding.ASCII.GetString(new byte[]{SerialMeterConnection.CR}),
					Logger = new ConsoleLoggingBridge()
				})
				{
					using(var protocol = new MeterProtocolIEC61107(connection))
					{
						foreach(var entry in protocol.Registrations)
						{
							Console.WriteLine (entry.Key + ": " + entry.Value);
						}
					}
				}
			}
			catch(MeterException me)
			{
				logger.error("Failed communicating with meter: " + me.Message);
			}
		}

	}	
}
