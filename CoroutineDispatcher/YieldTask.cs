using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CoroutineDispatcher
{
	public readonly struct YieldTask
	{
		private readonly DispatchPriority _priority;

		public YieldTask(DispatchPriority priority)
		{
			_priority = priority;
		}

		public YieldTaskAwaiter GetAwaiter()
		{
			return new YieldTaskAwaiter(_priority);
		}
	}

	public readonly struct YieldTaskAwaiter : INotifyCompletion
	{
		private readonly DispatchPriority _priority;
		
		public YieldTaskAwaiter(DispatchPriority priority)
		{
			_priority = priority;
		}

		public bool IsCompleted
		{
			get
			{
				if (!(SynchronizationContext.Current is CoroutineSynchronizationContext context))
					throw new InvalidOperationException("Awaiting Dispatcher.Yield aoutside of CoroutineSynchronizationContext");

				return !context.HasQueuedTasks(_priority);
			}
		}

		public void OnCompleted(Action continuation)
		{
			if (!(SynchronizationContext.Current is CoroutineSynchronizationContext context))
				throw new InvalidOperationException("Awaiting Dispatcher.Yield aoutside of CoroutineSynchronizationContext");

			context.Post(_priority, continuation);
		}

		public void GetResult()
		{ }
	}
}
