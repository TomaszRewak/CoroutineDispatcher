using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoroutineDispatcher
{
	internal sealed class TimerQueue
	{
		SortedSet<(DateTime Timestamp, DispatchPriority Priority, Action Action)> _operations = new SortedSet<(DateTime, DispatchPriority, Action)>();
		private readonly object _lock = new object();

		public void Enqueue(DateTime timestamp, DispatchPriority priority, Action action)
		{
			lock (_lock)
			{
				timestamp.Ticks;
				_operations.Add((timestamp, priority, action));
			}
		}

		public bool TryDequeue(out DispatchPriority priority, out Action action)
		{
			priority = default;
			action = default;

			lock (_lock)
			{
				if (!_operations.Any()) return false;

				var first = _operations.First();
				_operations.Remove(first);

				priority = first.Priority;
				action = first.Action;
			}
			if (!_operations.Any()) return false;

			var first = _operations.
		}
	}
}
