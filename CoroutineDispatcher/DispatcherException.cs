using System;
using System.Collections.Generic;
using System.Text;

namespace CoroutineDispatcher
{
	public class DispatcherException : Exception
	{
		public DispatcherException(string message) : base(message)
		{ }
	}
}
