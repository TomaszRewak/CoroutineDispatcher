using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	internal sealed class CoroutineSynchronizationContext : SynchronizationContext
	{
		private readonly OperationQueue _queue;
		private DispatchPriority _currentPriority;

		public CoroutineSynchronizationContext()
		{
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			_queue.Enqueue(DispatchPriority.High, () => d(state));
		}

		public void Post(DispatchPriority priority, Action operation)
		{
			_queue.Enqueue(priority, operation);
		}

		public void ExecuteFrame()
		{
			if (_queue.TryDequeue(out var operation, out var priority))
			{
				_currentPriority = priority;
				operation();
			}
			else
			{

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
	}
}
