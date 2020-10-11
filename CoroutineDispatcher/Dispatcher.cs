using System;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	public class Dispatcher
	{
		[ThreadStatic]
		private static Dispatcher _current;
		public static Dispatcher Current => _current;

		private readonly CoroutineSynchronizationContext _synchronizationContext;
		private readonly OperationQueue _operationQueue;

		public Dispatcher()
		{
			_synchronizationContext = new CoroutineSynchronizationContext(this);
		}

		public void Start()
		{

		}

		public void PushFrame()
		{
			if (_operationQueue.TryDequeue(out var operation))
			{
				var task = operation.Invoke();
				task.AsTask().
			}
		}

		private void ExecuteFrame()
		{
			if (!_operationQueue.TryDequeue(out var operation)) return;

			var task = operation.Invoke();

			if (task.IsCompleted) return;

			task.AsTask().ContinueWith()
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
			_operationQueue.Enqueue(priority, new Operation(operation));
		}

		public void Dispatch(Func<ValueTask> operation)
		{

		}

		public static ValueTask Yield(DispatchPriority priority)
		{
			if (!Current._operationQueue.TryDequeue(priority, out var operation))
				return new ValueTask();

			return operation.Invoke();
		}
	}
}
