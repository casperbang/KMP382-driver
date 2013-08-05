using System;
using System.IO.Ports;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using com.bangbits.metering.logging;
using com.bangbits.metering.utils;
using com.bangbits.metering.connection;

/// <summary>
/// KMP382 protocol driver. Plain number literals are in decimal, hex literals have 0x /// 
/// prepended and ASCII characters or strings are enclosed in double-quotes (spaces in a quoted
/// string designate a group operator for readability and is NOT to be interpreted as anything else).
/// </summary>
namespace com.bangbits.metering.protocol
{
	/// <summary>
	/// Serial driver to KMP382 using the "Kamstrup Meter Protocol". This protocol is unique
	/// in being logically binary, but physically (mostly) relying on ASCII characters underneath. 
	/// Everything but the first frame byte (0x40) is dealth with in pairs, such that two separate 
	/// physical bytes, interpreted as ASCII characters, unite to form one logical byte. 
	/// The first ASCII character contain the high-order nibble and the second ASCII character 
	/// contains the low-order nibble, utilizing only the range 0x30 to 0x46 while skipping over 
	/// 0x40 (ASCII characters 0-1, A-F). Furthermore, a logical 8-bit checksum of everything following 
	/// the frame type, is appended in the same manner, taking up two physical bytes.
    /// 
    /// Example of transmitted request:
    /// 
    /// 40 46 45 39 30 37 32  "@ FE 90 72"
    ///  |   |     |     |
    ///  |   |     |     |_ Double byte, checksum, 37 32 (ASCII "7" and "2") becomes one byte 0x72/114
    ///  |   |     |_______ Double byte, payload byte #1, 39 30 (ASCII "9" and "0") becomes one byte 0x90/144
    ///  |   |_____________ Double byte, payload byte #2, 46 45 (ASCII "F" and "E") becomes one byte 0xfe/254
    ///  |_________________ Single byte, magic frame identifier 40 (ASCII"@")
    /// 
    /// Example of received response:
    /// 
    /// 
    /// Registers (according to spec):
    /// Aktiv energi A+
    /// Aktiv energi A+
    /// Reaktiv energi R+
    /// Reaktiv energi R+
    /// Aktiv energi A+ Tarif (T1-T4)
    /// Reaktiv energi R+ Tarif (T1-T4)
    /// Maks.-effekt P+maks. Tarif 1
    /// Maks.-effekt P+maks. Tarif 1 Klokkeslæt
    /// Maks.-effekt P+maks. Tarif 1 Dato
    /// Maks.-effekt P+maks. Tarif 2
    /// Maks.-effekt P+maks. Tarif 2 Klokkeslæt
    /// Maks.-effekt P+maks. Tarif 2 Dato
    /// Maks.-effekt P+maks.
    /// Maks.-effekt P+maks. Dato
    /// Maks.-effekt P+maks. Klokkeslæt
    /// Akkumuleret maks.-effekt P+maks. akk
    /// Dato
    /// Klokkeslæt
    /// Timetæller
    /// Antal debiteringsperioder
    /// Effekttærskeltæller
    /// 
	/// </summary>
	/// 
	/// <see cref="http://www.kamstrup.dk/media/14910"/>
	/// <see cref="http://kamstrup.nl/media/701/file.pdf"/>
	public class MeterProtocolKMP382 : IMeterProtocol
	{
		const int MAGICBYTE_LENGTH = 1;
		const int CHECKSUM_LENGTH = 2;

		//TODO Move up in hierachy?
		public const byte CR = 0x0A;
		public const byte LF = 0x0D;

		IMeterConnection device;
		ILoggingBridge logger;

		// Start of IMeterProtocol impl

		// Map a delegate to a list of registrations
		// ...or...
		// Map a list of registrations to a delegate


		public MeterProtocolKMP382 (IMeterConnection device) : this(device, new NoOpLoggingBridge())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="com.bangbits.metering.MeterProtocolKMP382"/> class.
		/// A Meter needs to have a MeterDriver passed to it, which is responsible for the physical connection.
		/// </summary>
		/// <param name='device'>
		/// Driver.
		/// </param>
		public MeterProtocolKMP382 (IMeterConnection device, ILoggingBridge logger)
		{
			this.device = device;
			this.logger = logger;
			logger.info("Instantiated MeterProtocolKMP382");
		}


		public void Dispose() 
		{
    	}

		// End of IMeterProtocol impl

			
		// First byte represents the raw unescaped frame type, whereas everything
		// else following it, is distributed onto two discrete "Kamstrup bytes".
		// The raw output also includes, at least, the two checksum "Kamstrup bytes".
		
		/// <summary>
		/// <- 40 4645 3030 3032  "@ FE 00 02"
		///                          Com address?
		/// -> 48 4645 3030 3032 3031 3037 3031 4637  "H FE 00 02 01 07 01 F7"
		/// </summary>									 Com address?      Checksum
		protected byte[] HELLO = new byte[]
		{
			0x40, 	// Frame start marker
			0xfe,	// 254 is channel?!
			0x0		// ?
		};
		
		/// <summary>
		/// <- 40 4645 3031 3030 3031 3030 3243 4434  "@ FE 01 0001002C D4"
		/// 	                                      Com address    Checksum
		/// ->
		/// 48 4645 3031 3336 3338 3335 3333 3338 33   "H FE 01 36383533383"  20 bytes is MeterId (685382OK10) 
		/// 
 		/// 32 3446 3442 3331 3330 30 30 30 30 30 30 34   	  24F4B3130 0000004
 		/// 30 30 30 39 33 31 30 30 30 45 38 30 33 30 30 30   000931000E803000
 		/// 30 30 30 30 30 30 30 30 30 30 30 30 30 46 45 34   0000000000000FE4
 		/// 30 45 32 30 31 30 30 37 38 43 41 41 35 30 30 30   0E2010078CAA5000
 		/// 30 30 30 46 46 46 46 46 46 46 46 30 30 46 34 0D   000FFFFFFFF00F4.
		///                 Com address
		///                       MeterId (685382OK10)
		/// </summary>
		protected byte[] FETCH1 = new byte[] 
		{
			0x40, 	// Frame start marker
			0xfe, 	// 254 is channel?!
			0x01, 	
			0x00, 
			0x01, 
			0x00,
			// Response size: 1 - ff, works both with 0x90 and 0x2c, 0x0a is enough for metertype (0xdc needed for ID)
			0x2c 
		};
		
		// 0x48 0xfe 0x01 0x36 0xcb	H?6?
		

		/// <summary>
		/// <- 40 46 45 30 31 30 33 38 34 30 33 39 42 4443  "@ FE 01 03 84 03 9B DC"
		///                                                                   Checksum
		/// -> 48 46 45 30 31 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46
		/// 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46 46
		/// 46 46 46 46 46 46 46 46 31 39  "H FE 01 FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF 19"
		/// 																						Checksum
		/// </summary>
 		protected byte[] FETCH2 = new byte[] 
		{
			0x40, 	// Frame start marker
			0xfe, 	// 254 is channel?!
			0x01, 	
			0x03, 
			0x84,
			0x03, 
			0x9B
			
		};
		

		/// <summary>
		/// <- 40 46 45 39 30 37 32  "@ FE 90 72"
		///                             Channel
		///                                ConversationId?
		///                                   Checksum
		/// -> 48 46 45 39 30 30 30 30 30 30 39 30 34 30 30 30 30 30 30 30 30 30
		/// 30 30 30 43 32 31 35 30 30 31 46 42 43 45 30 30 30 30 30 30 30 30 31
		/// 30 42 42 "H FE 90 00002348 00000000 0000DECD 000DC841 00000617 00A5CA78 0000000000000000 10001093 8F"
		///                   Energi   Current  Hours    
		/// </summary>
		protected byte[] FETCH3 = new byte[] 
		{
			0x40, 	// Frame start marker
			0xfe, 	// 254 is channel?!
			0x90,
			0x72
		};
		
		
		/// <summary>
		/// <- 40 46 45 39 35 30 30 36 44 0D "@ FE 95 00 6D"
		///                                     Channel  Checksum
		///                                        ConversationId?
		/// -> 48 46 45 39 35 30 31 30 30 30 30 32 33 34 38 30 30 30 30 32 33 34 
		/// 37 30 30 30 30 30 30 30 31 39 36 0D "H FE 95 01 00002348 00002347 00000001 96"
		///                                                 Energi   Tarif1   Tarif2   Checksum
		///                                           ConversationId
		/// 
		/// </summary>
		protected byte[] FETCH4 = new byte[]
		{
			0x40,
			0xFE,
			0x95,
			0x00,
			0x6D
		};
			
		
		
		protected byte[] LOGIN = new byte[] 
		{
			0x40,	// Frame start marker
			0xfe,	// 254 is channel?!
			0x92,	// ?
			0x0		// ?
		};		
		
		
		
		
		public IMeterConnection Device
		{
			get
			{
				return device;
			}
		}
		
		
		
	
		/// <summary>
		/// 
		/// Calculates an 8-bit LRC (Longitudinal Redundancy Check) checksum by adding all bytes up 
		/// (with overflow), XOR'ing and adding 1. This simplified implementation relies on standard 
		/// unsigned byte overflow of C#. Alternative implementations here: 
		/// http://en.wikipedia.org/wiki/Longitudinal_redundancy_check
		/// 
		/// </summary>
		/// <returns>
		/// The checksum.
		/// </returns>
		/// <param name='data'>
		/// Data.
		/// </param>
		protected virtual byte Checksum(byte[] data)
		{	
        	byte checksum = 0;

        	foreach(byte value in data)
        	{
				checksum += value;
          	}

          	return (byte)((checksum ^ 0xFF) + 1);
		}

		/// <summary>
		/// Sends a command to the serial port. The first byte is sent as is, but
		/// all other successive bytes are expanded to two "Kamstrup bytes".
		/// </summary>
		/// <returns>
		/// The command.
		/// </returns>
		/// <param name='data'>
		/// Data.
		/// </param>
		/// <param name='log'>
		/// Log.
		/// </param>
		public byte[] SendCommand(byte[] data)
		{
			byte[] payload = ToKamstrupBytes(data.Subset(MAGICBYTE_LENGTH));
			
			byte[] checksum = ToKamstrupPair( Checksum(data.Subset(MAGICBYTE_LENGTH) ));
			
			return device.SendCommand( data.Subset(0, MAGICBYTE_LENGTH).Merge(payload, checksum));  
		}

		/// <summary>
		/// Converts a 4-bit value to an 8-bit kamstrup value, with the actual values offset
		/// to 0x30 and jumping over 0x40:
	    /// 	0x0->0x30, 0x1->0x31, 0x2->0x32, 0x3->0x33, 
	    ///		0x4->0x34, 0x5->0x35, 0x6->0x36, 0x7->0x37, 
	    /// 	0x8->0x38, 0x9->0x39, 0x1a->0x41, 0x1b->0x42,
		/// 	0x1c->0x43, 0x1d->0x44, 0x1e->0x45, 0x1f->0x46  
		/// </summary>
		/// <returns>
		/// The kamstrup value as a byte
		/// </returns>
		/// <param name='value'>
		/// Value.
		/// </param>
		protected byte ToKamstrupValue(byte value)
		{
			Debug.Assert(value >= 0x0 && value <= 0xA);
			
			return (byte)(value + (value < 10 ? 0x30 : 0x37));
		}
		
		/// <summary>
		/// Converts an 8-bit Kamstrup value to a 4-bit value. The range 0x30 to 0x47, without 0x40, maps
		/// to the range 0 - 1f
		/// 
		/// 0x30->0x0	0x31->0x1	0x32->0x2	0x33->0x3	0x34->0x4	0x35->0x5	0x36->0x6	0x37->0x7
		/// 0x38->0x8	0x39->0x9	0x41->0x1a	0x42->0x1b	0x43->0x1c	0x44->0x1d	0x45->0x1e	0x46->0x1f
		/// </summary>
		/// <returns>
		/// The kamstrup value.
		/// </returns>
		/// <param name='value'>
		/// Value.
		/// </param>
		protected byte FromKamstrupValue(byte value)
		{
			Debug.Assert(value >= 0x30 && value <= 0x46 && value != 0x40);
			
			return (byte)(value - (value < 0x3a ? 0x30: 0x37));
		}
		
		protected byte[] ToKamstrupQuad(int value)
		{
			byte[] quad = new byte[4];
			
			quad[0] = ToKamstrupValue((byte)(value >> 12));
			quad[1] = ToKamstrupValue((byte)((value >> 8) & 0xf));
			quad[2] = ToKamstrupValue((byte)((value >> 4) & 0xf));
			quad[3] = ToKamstrupValue((byte)(value & 0xf));
		
			return quad;
		}
		
		protected int FromKamstrupQuad(byte[] quad)
		{
			Debug.Assert(quad.Length == 4);
			
			return (FromKamstrupValue(quad[0]) << 12)
				+ (FromKamstrupValue(quad[1]) << 8)
				+ (FromKamstrupValue(quad[2]) << 4)
				+ (FromKamstrupValue(quad[3]));
		}
		

		protected byte[] ToKamstrupOct(long value)
		{
			byte[] oct = new byte[8];
			
			oct[0] = ToKamstrupValue((byte)((value >> 28) & 0xf));
			oct[1] = ToKamstrupValue((byte)((value >> 24) & 0xf));
			oct[2] = ToKamstrupValue((byte)((value >> 20) & 0xf));
			oct[3] = ToKamstrupValue((byte)((value >> 16) & 0xf));
			oct[4] = ToKamstrupValue((byte)((value >> 12) & 0xf));
			oct[5] = ToKamstrupValue((byte)((value >> 8) & 0xf));
			oct[6] = ToKamstrupValue((byte)((value >> 4) & 0xf));
			oct[7] = ToKamstrupValue((byte)(value & 0xf));
			
			return oct;
		}		

		protected long FromKamstrupOct(byte[] oct)
		{
			Debug.Assert(oct.Length == 8);
			
			return 
				+ (FromKamstrupValue(oct[0]) << 28)
				+ (FromKamstrupValue(oct[1]) << 24)
				+ (FromKamstrupValue(oct[2]) << 20)
				+ (FromKamstrupValue(oct[3]) << 16)
				+ (FromKamstrupValue(oct[4]) << 12)
				+ (FromKamstrupValue(oct[5]) << 8)
				+ (FromKamstrupValue(oct[6]) << 4)
				+ (FromKamstrupValue(oct[7]));
		}
		
		
		
		protected byte[] ToKamstrupPair(byte value)
		{
			byte[] quad = new byte[2];
			
			quad[0] = ToKamstrupValue((byte)((value >> 4) & 0xf));
			quad[1] = ToKamstrupValue((byte)(value & 0xf));
		
			return quad;
		}
		
		protected byte FromKamstrupPair(byte[] pair)
		{
			Debug.Assert(pair.Length == 2);
			
			return FromKamstrupPair(pair, 0);
		}
		
		protected byte FromKamstrupPair(byte[] pair, int startOffset)
		{
			Debug.Assert(pair.Length > 1);
			
			return (byte)((FromKamstrupValue(pair[startOffset]) << 4) | (FromKamstrupValue(pair[startOffset+1])));
		}	
		
		protected byte[] ToKamstrupBytes(byte[] data)
		{
			byte[] kamstrupBytes = new byte[data.Length*2];
			
			for(int i = 0; i < data.Length; i++)
			{
				kamstrupBytes[i*2] = ToKamstrupValue((byte)((data[i] >> 4) & 0xf));
				kamstrupBytes[i*2+1] = ToKamstrupValue((byte)(data[i] & 0xf));
			}
			
			return kamstrupBytes;
		}
		
		protected byte[] FromKamstrupBytes(byte[] kamstrupBytes)
		{
			Debug.Assert(kamstrupBytes.EvenCount());
			
			byte[] bytes = new byte[kamstrupBytes.Length/2];
			
			for(int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = (byte)(
					FromKamstrupValue(kamstrupBytes[i*2]) << 4 | 
					FromKamstrupValue(kamstrupBytes[i*2+1]) 
					);
			}
			
			return bytes;
		}
		
		protected byte[] FromKamstrupResponse(byte[] kamstrupResponse)
		{
			Debug.Assert(kamstrupResponse.OddCount());
			
			byte[] payload = FromKamstrupBytes(kamstrupResponse.Subset(1));
			
			return kamstrupResponse.Subset(0, MAGICBYTE_LENGTH).Merge(payload);
		}
		

		public Dictionary<string, string> Registrations
	    {
	        get
			{
				return DictionaryExtentions.Merge( Fetch1(), Fetch3(), Fetch4() );
			}
		}

		public Dictionary<string, string> Fetch1 ()
		{
			byte[] responseComplete = SendCommand (FETCH1);
			
			byte[] responseBody = responseComplete.Subset (MAGICBYTE_LENGTH, 
					responseComplete.Length - MAGICBYTE_LENGTH - CHECKSUM_LENGTH);
			
			byte responseChecksum = FromKamstrupPair (responseComplete.Subset (
				responseComplete.Length - CHECKSUM_LENGTH, CHECKSUM_LENGTH)
			);

			byte calculatedChecksum = Checksum (FromKamstrupBytes (responseBody));			

			if (calculatedChecksum == responseChecksum) 
			{
				logger.info ("Checksum validated: " + calculatedChecksum );
			} 
			else 
			{
				throw new MeterException("Invalid checksum. Calculated value of " + calculatedChecksum + " does not match response checksum value of " + responseChecksum);
			}

			Dictionary<string, string> result = new Dictionary<string, string>();
			
			result.Add("METER_TYPE", Encoding.ASCII.GetString( FromKamstrupBytes(responseBody.Subset(4,20)) ));
			
			result.Add(
				"METER_ID", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(62, 8).ReverseInPairs() 
						), NumberStyles.HexNumber
					)
				).ToString()
			);

			result.Add(
				"CUSTOMER_NO", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(70, 8).ReverseInPairs() 
						), NumberStyles.HexNumber
					)
				).ToString()
			);
			
			
			// EXPERIMENTAL 

			result.Add("TEST1", UInt32.Parse(Encoding.ASCII.GetString( responseBody.Subset(24,10).ReverseInPairs()), NumberStyles.HexNumber ).ToString());
			result.Add("TEST2", UInt32.Parse(Encoding.ASCII.GetString( responseBody.Subset(34,6).ReverseInPairs()), NumberStyles.HexNumber ).ToString());
			result.Add("TEST3", UInt32.Parse(Encoding.ASCII.GetString( responseBody.Subset(40,4).ReverseInPairs()), NumberStyles.HexNumber ).ToString());

			return result;
		}

		public void Fetch2()
		{
			byte[] responseComplete = SendCommand(FETCH2);

			byte[] responseBody = responseComplete.Subset (MAGICBYTE_LENGTH, 
					responseComplete.Length - MAGICBYTE_LENGTH - CHECKSUM_LENGTH);
			
			byte responseChecksum = FromKamstrupPair (responseComplete.Subset (
				responseComplete.Length - CHECKSUM_LENGTH, CHECKSUM_LENGTH)
			);

			byte calculatedChecksum = Checksum (FromKamstrupBytes (responseBody));			

			if (calculatedChecksum == responseChecksum) 
			{
				logger.info ("Checksum validated: " + calculatedChecksum );
			} 
			else 
			{
				throw new MeterException("Invalid checksum. Calculated value of " 
				                         + calculatedChecksum 
				                         + " does not match response checksum value of " 
				                         + responseChecksum);
			}
		}
		
		public Dictionary<string, string> Fetch3()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			byte[] responseComplete = SendCommand(FETCH3);
			
			byte[] responseBody = responseComplete.Subset (MAGICBYTE_LENGTH, 
					responseComplete.Length - MAGICBYTE_LENGTH - CHECKSUM_LENGTH);
			
			byte responseChecksum = FromKamstrupPair (responseComplete.Subset (
				responseComplete.Length - CHECKSUM_LENGTH, CHECKSUM_LENGTH)
			);

			byte calculatedChecksum = Checksum (FromKamstrupBytes (responseBody));			

			if (calculatedChecksum == responseChecksum) 
			{
				logger.info ("Checksum validated: " + calculatedChecksum );
			} 
			else 
			{
				throw new MeterException("Invalid checksum. Calculated value of " + calculatedChecksum + " does not match response checksum value of " + responseChecksum);
			}

			result.Add(
				"TOTAL_ENERGY", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(4, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);
			result.Add(
				"ENERGY_LOAD_WH", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(12, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);
			result.Add(
				"OPERATING_HOURS", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(20, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);			
			result.Add(
				"RESET_COUNTER", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(28, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);
			result.Add(
				"PEAK_POWER", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(36, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);
			result.Add(
				"CUSTOMER_NO_2", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(44, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);
			
			result.Add(
				"PRODUCTION_NO", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(68, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);			

			return result;
		}
		

		public Dictionary<string, string> Fetch4()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			byte[] responseComplete = SendCommand(FETCH4);
			
			byte[] responseBody = responseComplete.Subset (MAGICBYTE_LENGTH, 
					responseComplete.Length - MAGICBYTE_LENGTH - CHECKSUM_LENGTH);
			
			byte responseChecksum = FromKamstrupPair (responseComplete.Subset (
				responseComplete.Length - CHECKSUM_LENGTH, CHECKSUM_LENGTH)
			);

			byte calculatedChecksum = Checksum (FromKamstrupBytes (responseBody));			

			if (calculatedChecksum == responseChecksum) 
			{
				logger.info ("Checksum validated: " + calculatedChecksum );
			} 
			else 
			{
				throw new MeterException("Invalid checksum. Calculated value of " + calculatedChecksum + " does not match response checksum value of " + responseChecksum);
			}

			result.Add(
				"ENERGY_TOTAL2", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(6, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);
			result.Add(
				"ENERGY_TARIF1", (
					UInt32.Parse(  
						Encoding.ASCII.GetString( 
							responseBody.Subset(14, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);
			result.Add(
				"ENERGY_TARIF2", (
					UInt32.Parse(
						Encoding.ASCII.GetString( 
							responseBody.Subset(22, 8) 
						), NumberStyles.HexNumber
					)
				).ToString()
			);

			return result;
		}

	}
}

