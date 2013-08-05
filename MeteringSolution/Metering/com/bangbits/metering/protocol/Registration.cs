using System;

namespace com.bangbits.metering.protocol
{
	/// <summary>
	/// NOT BEING USED (YET)
	/// </summary>
	public class Registration
	{
		private RegistrationProperty property;
		private RegistrationValue value;
		private DateTime dateTime;

		public Registration (RegistrationProperty property, RegistrationValue value)
		{
			this.property = property;
			this.value = value;
			this.dateTime = DateTime.Now;
		}

		public RegistrationProperty Property 
		{
			get 
			{
				return this.property;
			}
		}

		public RegistrationValue Value 
		{
			get 
			{
				return this.value;
			}
		}

		public DateTime DateTime 
		{
			get 
			{
				return this.dateTime;
			}
		}
	}
}

