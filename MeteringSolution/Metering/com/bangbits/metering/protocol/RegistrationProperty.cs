using System;

namespace com.bangbits.metering.protocol
{
	/// <summary>
	/// NOT BEING USED (YET)
	/// </summary>
	public class RegistrationProperty //: IRegistrationProperty//, IRegistrationHandler
	{
		private string name;
		private Delegate fetcher;
		private Delegate extractor;
		//private Delegate validator;

		/// <summary>
		/// Initializes a new instance of the <see cref="com.bangbits.metering.RegistrationProperty"/> class.
		/// </summary>
		/// <param name='Name'>
		/// Common name of the property, i.e. TOTAL_ENERGY or TEMPERATURE.
		/// </param>
		/// <param name='extractor'>
		/// Extraction is a delegate to a method which will extract the given property into a value.
		/// </param>
		/// <param name='fetcher'>
		/// Fetch is a delagate to a method which will do the actual fetching of data from the meter.
		/// </param>
		/// <param name='validator'>
		/// Validator is a delagate to a method which will validate the entire data package sent from the meter.
		/// </param>
		public RegistrationProperty (string name, Delegate extractor, Delegate fetcher/*, Delegate validator*/)
		{
			this.name = name;
			this.extractor = extractor;
			this.fetcher = fetcher;
			/*this.validator = validator;*/
		}

		public string Name 
		{
			get
			{
				return this.name;
			}
		}

		public Delegate Extractor
		{
			get
			{
				return this.extractor;
			}
		}

		public Delegate Fetcher 
		{
			get
			{
				return this.fetcher;
			}
		}
		/*
		public Delegate Validator 
		{
			get
			{
				return this.validator;
			}
		}*/
	}
}

