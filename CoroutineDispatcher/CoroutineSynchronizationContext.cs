using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	internal sealed class CoroutineSynchronizationContext : SynchronizationContext
	{
		private readonly OperationQueue _queue = new OperationQueue();
		private DispatchPriority _currentPriority = DispatchPriority.High;

		public override void Post(SendOrPostCallback d, object state)
		{
			_queue.Enqueue(new Operation(_currentPriority, () => d(state)));
		}

		public void Post(Operation operation)
		{
			_queue.Enqueue(operation);
		}

		public void Run()
		{
			while (_queue.TryDequeue(out var operation))
			{
				_currentPriority = operation.Priority;
				operation.Action();
			}
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			base.Send(d, state);
		}

		public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			return base.Wait(waitHandles, waitAll, millisecondsTimeout);
		}

		public override void OperationStarted()
		{
			base.OperationStarted();
		}

		public override void OperationCompleted()
		{
			base.OperationCompleted();
		}

		internal bool HasQueuedTasks(DispatchPriority priority)
		{
			return _queue.Any(priority);
		}
	}
}
