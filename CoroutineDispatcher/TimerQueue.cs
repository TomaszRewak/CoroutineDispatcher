using System;
using System.Collections.Generic;

namespace CoroutineDispatcher
{
	internal sealed class TimerQueue
	{
		private readonly object _lock = new object();
		private readonly SortedDictionary<DateTime, List<(DispatchPriority Priority, Action Operation)>> _operations = new SortedDictionary<DateTime, List<(DispatchPriority Priority, Action Operation)>>();

		public DateTime? Next
		{
			get
			{
				lock (_lock)
				{
					var enumerator = _operations.GetEnumerator();
					return enumerator.MoveNext()
						? enumerator.Current.Key
						: (DateTime?)null;
				}
			}
		}

		public void Enqueue(DateTime timestamp, DispatchPriority priority, Action operation)
		{
			lock (_lock)
			{
				if (_operations.TryGetValue(timestamp, out var stampedOperations))
					stampedOperations.Add((priority, operation));
				else
					_operations.Add(timestamp, new List<(DispatchPriority, Action)> { (priority, operation) });
			}
		}

		public bool TryDequeue(out List<(DispatchPriority Priority, Action Operation)> operations)
		{
			lock (_lock)
			{
				var enumerator = _operations.GetEnumerator();
				if (enumerator.MoveNext() && enumerator.Current.Key <= DateTime.UtcNow)
				{
					operations = enumerator.Current.Value;
					_operations.Remove(enumerator.Current.Key);

					return true;
				}
			}

			operations = default;
			return false;
		}
	}
}
