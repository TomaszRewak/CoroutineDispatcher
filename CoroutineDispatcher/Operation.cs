using System;
using System.Collections.Generic;
using System.Text;

namespace CoroutineDispatcher
{
	public readonly struct Operation
	{
		public readonly DispatchPriority Priority;
		public readonly Action Action;

		public Operation(DispatchPriority priority, Action action)
		{
			Priority = priority;
			Action = action;
		}
	}
}
