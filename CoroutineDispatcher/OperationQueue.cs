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

		public bool TryDequeue(out Action operation, out DispatchPriority operationPriotiry)
		{
			return TryDequeue(DispatchPriority.Low, out operation, out operationPriotiry);
		}

		public bool TryDequeue(DispatchPriority minPriotirty, out Action operation, out DispatchPriority operationPriority)
		{
			for (var i = DispatchPriority.High; i >= minPriotirty; i--)
			{
				if (_queuedOperations.Any())
				{
					operation = _queuedOperations[(int)i].Dequeue();
					operationPriority = i;
					return true;
				}
			}

			operation = default;
			operationPriority = default;
			return false;
		}
	}
}
