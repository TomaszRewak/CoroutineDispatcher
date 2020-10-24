using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	public class Dispatcher
	{
		[ThreadStatic]
		private static Dispatcher _current = null;
		public static Dispatcher Current => _current;

		private readonly CoroutineSynchronizationContext _synchronizationContext = new CoroutineSynchronizationContext();

		public void Start()
		{
			var oldSynchronizationContext = SynchronizationContext.Current;

			try
			{
				_current = this;
				SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

				_synchronizationContext.Start();
			}
			finally
			{
				_current = null;
				SynchronizationContext.SetSynchronizationContext(oldSynchronizationContext);
			}
		}

		public void Execute()
		{
			var oldSynchronizationContext = SynchronizationContext.Current;

			try
			{
				_current = this;
				SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

				_synchronizationContext.Execute();
			}
			finally
			{
				_current = null;
				SynchronizationContext.SetSynchronizationContext(oldSynchronizationContext);
			}
		}

		public void Stop()
		{
			_synchronizationContext.Stop();
		}

		public static Dispatcher Spawn()
		{
			var dispatcher = new Dispatcher();
			var thread = new Thread(() => {
				dispatcher.Start();
			});
			thread.Start();

			return dispatcher;
		}

		public void Invoke(Action operation)
		{
			Invoke(DispatchPriority.Medium, operation);
		}

		public void Invoke(DispatchPriority priority, Action operation)
		{
			_synchronizationContext.Send(priority, operation);
		}

		public T Invoke<T>(Func<T> operation) => InvokeAsync(operation).Result;
		public T Invoke<T>(DispatchPriority priority, Func<T> operation)
		{
			return InvokeAsync(priority, operation).Result;
		}

		public Task InvokeAsync(Action operation) => InvokeAsync(DispatchPriority.Medium, operation);
		public Task InvokeAsync(DispatchPriority priority, Action operation)
		{
			return _synchronizationContext.Send(priority, operation);
		}

		public Task<T> InvokeAsync<T>(Func<T> operation) => InvokeAsync(DispatchPriority.Medium, operation);
		public Task<T> InvokeAsync<T>(DispatchPriority priority, Func<T> operation)
		{
			return _synchronizationContext.Send(priority, operation);
		}

		public void Dispatch(Action operation) => Dispatch(DispatchPriority.Medium, operation);
		public void Dispatch(DispatchPriority priority, Action operation)
		{
			_synchronizationContext.Post(priority, operation);
		}

		public void Dispatch(Func<ValueTask> operation) => Dispatch(DispatchPriority.Medium, operation);
		public void Dispatch(DispatchPriority priority, Func<ValueTask> operation)
		{
			_synchronizationContext.Post(priority, () => operation());
		}

		public void Dispatch(Func<Task> operation) => Dispatch(DispatchPriority.Medium, operation);
		public void Dispatch(DispatchPriority priority, Func<Task> operation)
		{
			_synchronizationContext.Post(priority, () => operation());
		}

		public void Schedule(TimeSpan timeSpan, Action operation) => Schedule(timeSpan, DispatchPriority.Medium, operation);
		public void Schedule(TimeSpan timeSpan, DispatchPriority priority, Action operation)
		{
			_synchronizationContext.PostDelayed(DateTime.UtcNow + timeSpan, priority, operation);
		}

		public static YieldTask Yield(DispatchPriority priority)
		{
			return new YieldTask(priority);
		}
	}
}
