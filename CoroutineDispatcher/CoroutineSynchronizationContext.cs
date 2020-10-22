using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	internal sealed class CoroutineSynchronizationContext : SynchronizationContext
	{
		private readonly OperationQueue _operationQueue = new OperationQueue();
		private readonly TimerQueue _timerQueue = new TimerQueue();
		private CancellationTokenSource _waitToken;

		internal void Start()
		{
			while(true)
			{
				_waitToken = null;

				Execute();

				_waitToken = new CancellationTokenSource();
				_waitToken.CancelAfter(_timerQueue.Next - DateTime.UtcNow);
				_waitToken.Token.WaitHandle.WaitOne();
			}
		}

		internal void Execute()
		{
			FlushTimerQueue();

			while (_operationQueue.TryDequeue(out var operation))
			{
				operation();
				FlushTimerQueue();
			}
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			Send(DispatchPriority.Medium, () => d(state)).Wait();
		}

		internal Task Send(DispatchPriority priority, Action operation)
		{
			var task = new Task(operation);
			Post(priority, () => task.RunSynchronously());
			return task;
		}

		internal Task<T> Send<T>(DispatchPriority priority, Func<T> operation)
		{
			var task = new Task<T>(operation);
			Post(priority, () => task.RunSynchronously());
			return task;
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			Post(DispatchPriority.Medium, () => d(state));
		}

		internal void Post(DispatchPriority priority, Action operation)
		{
			_operationQueue.Enqueue(priority, operation);
		}

		internal void PostDelayed(DateTime dateTime, DispatchPriority priority, Action action)
		{
			_timerQueue.Enqueue(dateTime, priority, action);
			_waitToken?.Cancel();
		}

		//public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		//{
		//	return base.Wait(waitHandles, waitAll, millisecondsTimeout);
		//}

		//public override void OperationStarted()
		//{
		//	base.OperationStarted();
		//}

		//public override void OperationCompleted()
		//{
		//	base.OperationCompleted();
		//}

		internal bool HasQueuedTasks(DispatchPriority priority)
		{
			return _operationQueue.Any(priority);
		}

		private void FlushTimerQueue()
		{
			while (_timerQueue.TryDequeue(out var priority, out var action))
				Post(priority, action);
		}
	}
}
