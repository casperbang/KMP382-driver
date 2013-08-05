using System;
using System.Collections.Generic;

namespace com.bangbits.metering.protocol
{
	/// <summary>
	/// An IMeterProtocol represents a logical meter version, encapsulating all protocol 
	/// aspects associated with communicating with the meter at the software level. There 
	/// might be several versions of a meter, which then requires each their own 
	/// implementation of IMeterProtocol. It is however likely, that a derived instance 
	/// can piggyback off an earlier version.
	/// </summary>
	public interface IMeterProtocol : IDisposable
	{
		/// <summary>
		/// Gets all the available registrations as a IEnumerable<Registration>. This can take
		/// some time, since every possible read-only property will be read typically using
		/// multiple calls.
		/// </summary>
		/// <value>
		/// The registrations.
		/// </value>
		Dictionary<string, string> Registrations
	    {
	        get;
		}
	}
}

