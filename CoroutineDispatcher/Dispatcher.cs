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

		public void Run()
		{
			_current = this;
			SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
			_synchronizationContext.Run();
			_current = null;
		}

		public static void Spawn()
		{

		}

		public void Invoke()
		{

		}

		//public T Invoke<T>()
		//{

		//}

		public void InvokeAsync(Func<ValueTask> operation)
		{
			operation();
		}

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
