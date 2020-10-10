using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoroutineDispatcher
{
	internal sealed class OperationQueue
	{
		private readonly Queue<Operation>[] _queuedOperations = new Queue<Operation>[]
		{
			new Queue<Operation>(),
			new Queue<Operation>(),
			new Queue<Operation>()
		};

		public void Enqueue(DispatchPriority priority, Operation operation)
		{
			_queuedOperations[(int)priority].Enqueue(operation);
		}

		public bool TryDequeue(out Operation operation)
		{
			return TryDequeue(DispatchPriority.Low, out operation);
		}

		public bool TryDequeue(DispatchPriority priority, out Operation operation)
		{
			for (var i = DispatchPriority.High; i >= priority; i--)
			{
				if (_queuedOperations.Any())
				{
					operation = _queuedOperations[(int)i].Dequeue();
					return true;
				}
			}

			operation = default;
			return false;
		}
	}
}
