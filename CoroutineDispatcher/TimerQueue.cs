using System;
using System.Collections.Generic;

namespace CoroutineDispatcher
{
	internal sealed class TimerQueue
	{
		private readonly object _lock = new object();
		private readonly SortedSet<(DateTime Timestamp, DispatchPriority Priority, Action Action, int Index)> _operations = new SortedSet<(DateTime, DispatchPriority, Action, int)>();

		private int _operationIndex;

		public DateTime? Next
		{
			get
			{
				lock (_lock)
				{
					var enumerator = _operations.GetEnumerator();
					return enumerator.MoveNext()
						? enumerator.Current.Timestamp
						: (DateTime?)null;
				}
			}
		}

		public void Enqueue(DateTime timestamp, DispatchPriority priority, Action action)
		{
			lock (_lock)
			{
				unchecked { _operationIndex++; }
				_operations.Add((timestamp, priority, action, _operationIndex));
			}
		}

		public bool TryDequeue(out DispatchPriority priority, out Action action)
		{
			lock (_lock)
			{
				var enumerator = _operations.GetEnumerator();
				if (enumerator.MoveNext() && enumerator.Current.Timestamp <= DateTime.UtcNow)
				{
					priority = enumerator.Current.Priority;
					action = enumerator.Current.Action;

					_operations.Remove(enumerator.Current);

					return true;
				}
				else
				{
					priority = default;
					action = default;
					return false;
				}
			}
		}
	}
}
