using System;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	public class Dispatcher
	{
		[ThreadStatic]
		private static Dispatcher _current;
		public static Dispatcher Current => _current;

		public void Start()
		{

		}

		public static void Spawn()
		{

		}

		public void Invoke()
		{

		}

		public ValueTask PushFrame()
		{

		}

		//public T Invoke<T>()
		//{

		//}

		public async ValueTask InvokeAsync(Func<ValueTask> operation)
		{
			await operation();
		}

		//public Task<T> InvokeAsync<T>()
		//{

		//}

		public void Dispatch(Action operation)
		{

		}

		public void Dispatch(DispatchPriority priority, Action operation)
		{

		}

		public static ValueTask Yield(DispatchPriority priority = DispatchPriority.Medium)
		{
			return new ValueTask(new Task())
		}
	}
}
