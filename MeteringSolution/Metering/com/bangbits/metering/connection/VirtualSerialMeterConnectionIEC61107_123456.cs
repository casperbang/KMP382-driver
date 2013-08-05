using System;
using System.Text;

namespace com.bangbits.metering.connection
{
	/// <summary>
	/// VirtualDevice(new byte[]{ BINARY METER DATA SEEDING }):
	/// Handles virtual communication, where a device has hardwired state
	/// or gets request-reply pairs mocked up upon instantiation. A virtual
	/// device thus becomes a perfect reverse-engineering tool which mimics
	/// the actual device at a point in time, without mutating and withough
	/// requering a physical meter to be connected.
	/// </summary>
	public class VirtualSerialMeterConnectionIEC61107_123456 : SerialMeterConnection
	{
		Boolean open = false;

		byte[] meterData = new byte [] {};

		public VirtualSerialMeterConnectionIEC61107_123456 () /*: base("")*/
		{
		}

		new public void Dispose() 
		{
			Close();
    	}
				
		new public void Close()
		{
			if(open)
			{
				open = false;
			}
		}

		new public int ReadByte()
		{
			//return port.ReadByte();
			return 34;
		}
		
		new public string ReadLine()
		{
			//return port.ReadLine();
			return "noget";
		}



		private void AssertPortOpenness ()
		{
			if (!open) 
			{
				open = true;
			}
		}

	

		/// <summary>
		/// Sends the command, defaulting to active logging
		/// </summary>
		/// <returns>
		/// The command.
		/// </returns>
		/// <param name='command'>
		/// Command.
		/// </param>
	    new public byte[] SendCommand(byte[] command)
		{
			return SendCommand(command, true);
		}
		
		public byte[] SendCommand(byte[] request, bool log)
		{
			if(log)
			{
				LogOutput(request);
			}
			
			WriteLine(request);

			byte[] response = Encoding.ASCII.GetBytes("teeeeeeeeeest");
			
			if(log)
			{
				LogInput(response);
			}
			
			return response;
		}
		
		new protected void WriteLine(byte[] data)
		{
			AssertPortOpenness();
			
			port.Write(data, 0, data.Length);
			port.WriteLine("");
		}

	}
}

