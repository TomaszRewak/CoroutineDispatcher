using System;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	public class Dispatcher
	{
		[ThreadStatic]
		private static Dispatcher _current;
		public static Dispatcher Current => _current;

		private readonly OperationQueue _operationQueue;

		public void Start()
		{

		}

		public static void Spawn()
		{

		}

		public void Invoke()
		{

		}

		public void PushFrame()
		{
			if (Current._operationQueue.TryDequeue(out var operation))
			{

			}
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

		public static ValueTask Yield(DispatchPriority priority = DispatchPriority.Medium)
		{
			if (!Current._operationQueue.TryDequeue(priority, out var operation))
				return new ValueTask();

			return operation.Invoke();
		}
	}
}
