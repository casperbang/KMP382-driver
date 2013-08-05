using System;
using System.IO.Ports;
using System.Text;
using com.bangbits.metering.logging;
using com.bangbits.metering.utils;

namespace com.bangbits.metering.connection
{
	/// <summary>
	/// SerialMeterConnection is an implementation of IMeterConnection for binary communication over
	/// a serial port. You supply the class the name of the serial port upon instantiation and
	/// it then provides a single (overloaded, synchronous) request-response method called 
	/// SendCommand(...). Due to this class often being used in association with optical
	/// synchronous interfaces, echo/crosstalk detection and filtering is build into the class
	/// for compensation purposes.
	/// 
	/// </summary>
	public class SerialMeterConnection: IMeterConnection
	{
		private ILoggingBridge logger = new NoOpLoggingBridge();
	 	protected SerialPort port = new SerialPort();

		public const byte CR = 0x0A;
		public const byte LF = 0x0D;

		int lineNo = 0;

		public SerialMeterConnection ()
		{

			CrosstalkCompensationEnabled = false;

			DtrEnable = false;
			RtsEnable = true;


			/*
			ReadTimeout = 700;
			WriteTimeout = 500;
			NewLine = Encoding.ASCII.GetString(new byte[]{LF});
			*/

			/*
			logger.info(
				"Instantiated SerialMeterConnection with parameters {0}{1}{2}@{3}", 
				DataBits,
				StopBits,
				Handshake,
				BaudRate);
			*/
		}

		/*
		public SerialMeterConnection (string portName) : this(portName, new NoOpLoggingBridge())
		{
		}

		// The name of the serial port is the only mandatory setting
		public SerialMeterConnection (string portName, ILoggingBridge logger)
		{
			port = new SerialPort();
			PortName = portName;
			CrosstalkCompensationEnabled = false;
			BaudRate = 300;
        	Parity = Parity.Even;
        	DataBits = 7;
        	StopBits = StopBits.One;
        	Handshake = Handshake.None;
			Encoding = new ASCIIEncoding();
			DtrEnable = false;
			RtsEnable = true;
			ReadTimeout = 700;
			WriteTimeout = 500;
			NewLine = Encoding.ASCII.GetString(new byte[]{LF});
			this.logger = logger;
		}
		*/

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="com.bangbits.metering.connection.SerialMeterConnection"/> has crosstalk
		/// compensation enabled. Crosstalk compensation can be used if you, during synchronous operation, receive feedback from
		/// your own transmission. Ideally you should reduce the sensetivity of the receiver and/or the transmission output, but
		/// it can also be handled by software, with a little overhead.
		/// </summary>
		/// <value>
		/// <c>true</c> if crosstalk compensation is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool CrosstalkCompensationEnabled
	    {
	        get;
			set;
		}

		public int LineNo {
			get 
			{
				return lineNo;
			}
		}

		// SerialPort delegation
		public string PortName
	    {
	        get
			{
				return port.PortName;
			}
			
			set
			{
				port.PortName = value;
			}
		}
		
		public int BaudRate
	    {
	        get
			{
				return port.BaudRate;
			}
			
			set
			{
				port.BaudRate = value;
			}
		}
		
		public Parity Parity
	    {
	        get
			{
				return port.Parity;
			}
			
			set
			{
				port.Parity = value;
			}
		}

		public int DataBits
	    {
	        get
			{
				return port.DataBits;
			}
			
			set
			{
				port.DataBits = value;
			}
		}
		
		public StopBits StopBits
	    {
	        get
			{
				return port.StopBits;
			}
			
			set
			{
				port.StopBits = value;
			}
		}
		
		public Handshake Handshake
	    {
	        get
			{
				return port.Handshake;
			}
			
			set
			{
				port.Handshake = value;
			}
		}

		public Encoding Encoding
	    {
	        get
			{
				return port.Encoding;
			}
			
			set
			{
				port.Encoding = value;
			}
		}

		public bool DtrEnable
	    {
	        get
			{
				return port.DtrEnable;
			}
			
			set
			{
				port.DtrEnable = value;
			}
		}

		public bool RtsEnable
	    {
	        get
			{
				return port.RtsEnable;
			}
			
			set
			{
				port.RtsEnable = value;
			}
		}

		public int ReadTimeout
	    {
	        get
			{
				return port.ReadTimeout;
			}
			
			set
			{
				port.ReadTimeout = value;
			}
		}
		
		public int WriteTimeout
	    {
	        get
			{
				return port.WriteTimeout;
			}
			
			set
			{
				port.WriteTimeout = value;
			}
		}
		
		public string NewLine
	    {
	        get
			{
				return port.NewLine;
			}
			
			set
			{
				port.NewLine = value;
			}
		}

		public ILoggingBridge Logger
	    {
	        get
			{
				return this.logger;
			}
			
			set
			{
				this.logger = value;
			}
		}

		public int ReadByte()
		{
			return port.ReadByte();
		}
		
		public string ReadLine()
		{
			lineNo++;

			return port.ReadLine();
		}
		
		public void Dispose() 
		{
			Close();
    	}
				
		public void Close()
		{
			if(IsOpen)
			{
				port.Close();
			}
		}

		public bool IsOpen
		{
			get
			{
				return (port != null && port.IsOpen );
			}
		}

		private void AssertPortOpenness ()
		{
			if (!port.IsOpen) 
			{
				try
				{
					port.Open ();

					logger.info(
						"Opened SerialMeterConnection with parameters {0}{1}{2}@{3}", 
						DataBits,
						StopBits,
						Handshake,
						BaudRate);
				} 
				catch (System.IO.IOException exception)
				{
					throw new MeterException("Permission denied. Check permissions for " + PortName +
					                         ", fix with 'usermod -a -G dialout <USER_NAME>'");
				}

				System.Threading.Thread.Sleep (100);
				
				if (!port.IsOpen) {
					throw new MeterException("Failed opening port " + PortName );
				}
			}
		}

		public byte[] SendCommand(byte[] request)
		{
			WriteLine(request);
		
			byte[] response = ReadLineAsBinary();

			// Are we dealing with echo/crosstalk
			if(CrosstalkCompensationEnabled && response.ContentEquals(request))
			{
				// Ignore previous response and fetch new one
				response = ReadLineAsBinary();
				logger.warn("Echo/crosstalk detected! Software compensation is being utilized, but for" +
					"optimal performance, try lowering receiver sensitivity and/or transmitter output power.");
			}
			
			return response;
		}

		protected byte[] ReadLineAsBinary()
		{
			byte[] response = Encoding.ASCII.GetBytes(ReadLine());

			LogInput(response);

			return response;
		}

		protected void WriteLine(byte[] request)
		{
			AssertPortOpenness();

			LogOutput(request);

			port.Write(request, 0, request.Length);
			port.WriteLine("");
		}

		protected void LogInput(byte[] response)
		{
			Log("-> ", response);
		}
		
		protected void LogOutput(byte[] request)
		{
			Log("<- ", request);
		}

		protected virtual void Log(string text, byte[] data)
		{
			foreach(byte b in data)
			{
				text += " " + b.ToHex();
			}
	
			logger.info( text + "\t" + Encoding.ASCII.GetString(data));
		}
	}
}

