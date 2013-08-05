using System;
using System.IO.Ports;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using com.bangbits.metering.connection;

namespace com.bangbits.metering.protocol
{
	/// <summary>
	/// This uses default IEC1107 protocol, to extract data from legacy meters:
	/// http://kamstrup.com/media/2105/IEC61107_comprotocol.pdf
	/// 
	/// OBIS codes:
	/// http://www.dlms.com/documentation/listofstandardobiscodesandmaintenanceproces/index.html
	/// 
	/// /KAM 685-382-OK-10		// Make and model
	///	0.0(12345678)			// Customer no.
	///	1.20(0009032*kWh)		// Energy registre
	///	1.20.1(0009031*kWh)		// Tarif 1
	///	1.20.2(0000001*kWh)		// Tarif 2
	///	1.31(0057342*h)			// Hour counter
	///	1.26(0000000)			// Pulse input
	///	1.6(000001,5*kW)		// Actual peek
	///	1.6*1(000002,8)!		// Last months peek
	/// </summary>
	public class MeterProtocolIEC61107 : IMeterProtocol
	{
		const byte STX = 0x02;
		const byte ETX = 0x03;
		
		const byte CR = 0x0A;
		const byte LF = 0x0D;
	
		const String DOWNLOAD_METERDATA = "/?!";

		IMeterConnection connection;

		public MeterProtocolIEC61107 (IMeterConnection connection)
		{
			this.connection = connection;
		}
		
		public void Dispose() 
		{
    	}

		public bool Connected
		{
			get
			{
				return true;
			}
		}

		public Dictionary<string, string> Registrations
	    {
	        get
			{
				return Fetch ();
			}
		}


		private Dictionary<string, string> Fetch()
		{
			var results = new Dictionary<string, string>();
			
			byte[] reply = connection.SendCommand( Encoding.ASCII.GetBytes(DOWNLOAD_METERDATA));
			
			string stringReply = Encoding.ASCII.GetString(reply);
			
			results.Add("MAKE", stringReply.Substring(1,3));
			results.Add("MODEL", stringReply.Substring(5));
			
			if(connection.ReadByte() != STX)
			{
				throw new MeterException("STX expected!");
			}
			
			int checksum = 0;
			
			String input;
			int i = 2;
			do
			{
				input = connection.ReadLine();
				
				string[] parts = input.Split("(".ToCharArray());

				if(parts.Length == 2)
				{
					results.Add(parts[0], parts[1].Substring(0, parts[1].IndexOf(")")));
				}
				else
				{
					results.Add("#" + i, input);
				}
				
				foreach(char value in input)
				{
					checksum += value;
				}
				checksum += CR;
				i++;
			}
			while(!input.Contains("!")); // TODO: Replace by EndsWith(..)?
			
			int etx = connection.ReadByte();
			if(etx != ETX)
			{
				throw new MeterException("ETX expected!");
			}
			
			checksum += etx;
			checksum &= 0x7F; // Remove anything but the 7 LSB's
			
			if(connection.ReadByte() != checksum)
			{
				throw new MeterException("Invalid checksum!");
			}
			
			return results;
		}

	}
}

