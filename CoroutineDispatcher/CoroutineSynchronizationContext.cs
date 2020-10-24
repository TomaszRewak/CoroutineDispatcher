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

		private bool _running;
		private CancellationTokenSource _waitToken;

		internal void Start()
		{
			_running = true;

			while (_running)
			{
				ExecuteAvailableOperations();
				WaitForPendingOperations();
			}
		}

		internal void Execute()
		{
			_running = true;
			ExecuteAvailableOperations();
		}

		internal void Stop()
		{
			_running = false;
			_waitToken?.Cancel();
		}

		private void ExecuteAvailableOperations()
		{
			FlushTimerQueue();

			while (_running && _operationQueue.TryDequeue(out var operation))
			{
				operation();
				FlushTimerQueue();
			}
		}

		private void WaitForPendingOperations()
		{
			if (!_running)
				return;

			_waitToken = new CancellationTokenSource();

			if (_timerQueue.Next is DateTime next)
				_waitToken.CancelAfter(next - DateTime.UtcNow);

			_waitToken.Token.WaitHandle.WaitOne();
			_waitToken = null;
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			Send(DispatchPriority.High, () => d(state)).Wait();
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
			Post(DispatchPriority.High, () => d(state));
		}

		internal void Post(DispatchPriority priority, Action operation)
		{
			_operationQueue.Enqueue(priority, operation);
			_waitToken?.Cancel();
		}

		internal void PostDelayed(DateTime dateTime, DispatchPriority priority, Action action)
		{
			_timerQueue.Enqueue(dateTime, priority, action);
			_waitToken?.Cancel();
		}

		public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			throw new NotImplementedException();
			//return base.Wait(waitHandles, waitAll, millisecondsTimeout);
		}

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
