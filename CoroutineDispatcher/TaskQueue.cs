using System;
using System.Collections.Generic;
using System.Text;

namespace CoroutineDispatcher
{
	internal sealed class TaskQueue
	{
		private readonly Queue<Action>[] _pendingTasks = new Queue<Action>[]
		{
			new Queue<Action>(),
			new Queue<Action>(),
			new Queue<Action>()
		};

		public void Add(Action action, DispatchPriority priority)
		{
			_pendingTasks[(int)priority].Enqueue(action);
		}
	}
}
