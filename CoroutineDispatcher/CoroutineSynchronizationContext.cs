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
		private CancellationTokenSource _waitToken;

		internal void Start()
		{
			Execute();
		}

		internal void Execute()
		{
			while (_queue.TryDequeue(out var operation))
				operation();
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			Post(DispatchPriority.Medium, () => d(state));
		}

		public void Post(DispatchPriority priority, Action operation)
		{
			_queue.Enqueue(priority, operation);
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			Send(DispatchPriority.Medium, () => d(state)).Wait();
		}

		public Task Send(DispatchPriority priority, Action operation)
		{
			var task = new Task(operation);
			Post(priority, () => task.RunSynchronously());
			return task;
		}

		public Task<T> Send<T>(DispatchPriority priority, Func<T> operation)
		{
			var task = new Task<T>(operation);
			Post(priority, () => task.RunSynchronously());
			return task;
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
