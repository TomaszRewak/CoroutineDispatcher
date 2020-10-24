using System;
using System.Collections.Generic;

namespace CoroutineDispatcher
{
	internal sealed class OperationQueue
	{
		private readonly object _lock = new object();
		private readonly Queue<Action>[] _queuedOperations = new Queue<Action>[]
		{
			new Queue<Action>(),
			new Queue<Action>(),
			new Queue<Action>()
		};

		public void Enqueue(DispatchPriority priority, Action operation)
		{
			lock (_lock)
			{
				_queuedOperations[(int)priority].Enqueue(operation);
			}
		}

		public bool TryDequeue(out Action operation)
		{
			lock (_lock)
			{
				for (var priority = DispatchPriority.High; priority >= DispatchPriority.Low; --priority)
				{
					var queue = _queuedOperations[(int)priority];
					if (queue.Count > 0)
					{
						operation = queue.Dequeue();
						return true;
					}
				}
			}

			operation = default;
			return false;
		}

		public int Count(DispatchPriority minPriority)
		{
			int count = 0;

			lock (_lock)
			{
				for (var priority = DispatchPriority.High; priority >= minPriority; --priority)
					count += _queuedOperations[(int)priority].Count;
			}

			return count;
		}

		public bool Any(DispatchPriority minPriority)
		{
			return Count(minPriority) > 0;
		}
	}
}
