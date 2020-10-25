using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CoroutineDispatcher
{
	public readonly struct YieldTask
	{
		private readonly DispatchPriority _minPriority;

		public YieldTask(DispatchPriority minPriority)
		{
			_minPriority = minPriority;
		}

		public YieldTaskAwaiter GetAwaiter()
		{
			return new YieldTaskAwaiter(_minPriority);
		}
	}

	public readonly struct YieldTaskAwaiter : INotifyCompletion
	{
		private readonly DispatchPriority _minPriority;
		
		public YieldTaskAwaiter(DispatchPriority minPriority)
		{
			_minPriority = minPriority;
		}

		public bool IsCompleted
		{
			get
			{
				if (!(SynchronizationContext.Current is CoroutineSynchronizationContext context))
					throw new DispatcherException("Awaiting Dispatcher.Yield outside of CoroutineSynchronizationContext");

				return !context.HasQueuedTasks(_minPriority);
			}
		}

		public void OnCompleted(Action continuation)
		{
			if (!(SynchronizationContext.Current is CoroutineSynchronizationContext context))
				throw new DispatcherException("Awaiting Dispatcher.Yield outside of CoroutineSynchronizationContext");

			context.Post(_minPriority, continuation);
		}

		public void GetResult()
		{ }
	}
}
