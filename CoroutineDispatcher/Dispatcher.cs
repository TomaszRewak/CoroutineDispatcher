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
			_current = this;
			SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
			_synchronizationContext.Execute();
			_current = null;
		}

		public void Stop()
		{

		}

		public void Execute()
		{
			_current = this;
			SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
			_synchronizationContext.Execute();
			_current = null;
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
			_synchronizationContext.Send(operation);
		}

		//public T Invoke<T>()
		//{

		//}

		//public void InvokeAsync(Func<ValueTask> operation)
		//{
		//	operation();
		//}

		//public Task<T> InvokeAsync<T>()
		//{

		//}

		public void Dispatch(Action operation)
		{
			Dispatch(DispatchPriority.Medium, operation);
		}

		public void Dispatch(DispatchPriority priority, Action operation)
		{
			_synchronizationContext.Post(new Operation(priority, operation));
		}

		public void Dispatch(Func<ValueTask> operation)
		{
			Dispatch(DispatchPriority.Medium, operation);
		}

		public void Dispatch(DispatchPriority priority, Func<ValueTask> operation)
		{
			_synchronizationContext.Post(new Operation(priority, () => operation()));
		}

		public static YieldTask Yield(DispatchPriority priority)
		{
			return new YieldTask(priority);
		}
	}
}
