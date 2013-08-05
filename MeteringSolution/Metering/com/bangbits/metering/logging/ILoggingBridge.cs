using System;

namespace com.bangbits.metering.logging
{
	/// <summary>
	/// A generic logging bridge which is used to:
	/// - Provide a simple default implementation (ConsoleLoggingBridge) of logging without a 3'rd dependency
	/// - Offer an easy mechanish to bridge with an existing established logging framework (log4net, Android etc.).
	/// 
	/// Levels borrowed from http://stackoverflow.com/questions/7839565/logging-levels-logback-rule-of-thumb-to-assign-log-levels
	/// </summary>
	public interface ILoggingBridge
	{
		/// <summary>
		/// Ultra fine trace logging. This is usually disabled during normal development and certainly
		/// in the production environment. It's used for live troubleshooting during development, as
		/// its highly detailed grainability can be thought as similar to stepping through code with a 
		/// debugger. There's virtually no limit to the amount of detail being logged here, but if this
	    /// chatty and noisy level is left enabled in the production environment, the developer has to 
		/// compensate by bringing beer or cake to fellow developers! 
		/// 
		/// Examples of what to include here, are SQL statements, bind variables, detailed iteration and
		/// algorithm flow, object graphs etc. (When the question is "how is it going on?") 
		/// 
		/// This level is sometimes also called FINER.
		/// </summary>
		/// <param name='msg'>
		/// Message.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		void trace(string msg, params object[] args);

		/// <summary>
		/// Fine logging. This is usually enabled both during normal development and when deployed to
		/// the production environment. Only course grained essential life cycle events gets logged here, 
		/// otherwise it's recommended to use the trace level.
		/// 
		/// Examples of what to include here, are setup parameters, user info, thread identification and 
		/// general context information. (When the question is "what is going on?")
		/// 
		/// This level is sometimes also called FINE.
		/// </summary>
		/// <param name='msg'>
		/// Message.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		void debug(string msg, params object[] args);

		/// <summary>
		/// General informative logging. This is usually enabled both in production environment as well 
		/// as in developerment environment, as it offers general course grainability of what's going
		/// on both during normal development and when analyzing a log file.
		/// 
		/// Examples of what to include here, are calls to discrete abstraction layers (call to DB), 
		/// session events (successful login/logout) and system lifecycle events (start/stop of services).
		/// </summary>
		/// <param name='msg'>
		/// Message.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		void info(string msg, params object[] args);

		/// <summary>
		/// Warning logging. This is where unexpected but recoverable error conditions go. The user
		/// may notice a problem, but the system does not crash and can continue to service the user
		/// at least to some degree. These logging events may be found in a production environment log 
		/// file and would be a strong indicator of scenarios which could be handled better.
		/// 
		/// Examples of what to include here, are runtime assertions.
		/// </summary>
		/// <param name='msg'>
		/// Message.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		void warn(string msg, params object[] args);

		/// <summary>
		/// Error logging. This is where unrecoverable errors go, where the user is affected and which 
		/// typically requring human intervention and/or bug fixing to make things good again. These
		/// logging events should obviously not be found in a production environment log file.
		/// 
		/// </summary>
		/// <param name='msg'>
		/// Message.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		void error(string msg, params object[] args);
	}
}

