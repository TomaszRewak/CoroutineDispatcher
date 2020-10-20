using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoroutineDispatcher
{
	internal sealed class OperationQueue
	{
		private readonly Queue<Action>[] _queuedOperations = new Queue<Action>[]
		{
			new Queue<Action>(),
			new Queue<Action>(),
			new Queue<Action>()
		};

		public void Enqueue(DispatchPriority priority, Action operation)
		{
			_queuedOperations[(int)priority].Enqueue(operation);
		}

		public bool TryDequeue(out Action operation)
		{
			return TryDequeue(DispatchPriority.Low, out operation);
		}

		public bool TryDequeue(DispatchPriority minPriority, out Action operation)
		{
			for (var priority = DispatchPriority.High; priority >= minPriority; priority--)
			{
				var queue = _queuedOperations[(int)priority];
				if (queue.Any())
				{
					operation = queue.Dequeue();
					return true;
				}
			}

			operation = default;
			return false;
		}

		public bool Any(DispatchPriority minPriority)
		{
			for (var priority = DispatchPriority.High; priority >= minPriority; priority--)
			{
				var queue = _queuedOperations[(int)priority];
				if (queue.Any())
				{
					return true;
				}
			}

			return false;
		}
	}
}
