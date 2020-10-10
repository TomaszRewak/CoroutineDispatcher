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

		public void Add(DispatchPriority priority, Action action)
		{
			_pendingTasks[(int)priority].Enqueue(action);
		}
	}
}
