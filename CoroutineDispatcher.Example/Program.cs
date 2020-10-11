using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher.Example
{
	class Program
	{
		static void Main(string[] args)
		{
			var dispatcher = new Dispatcher();

			dispatcher.Dispatch(AsyncAction2);
			dispatcher.Run();
		}

		static void NormalAction()
		{
			Console.WriteLine($"NormalAction {Thread.CurrentThread.ManagedThreadId}");
		}

		static async ValueTask AsyncAction1()
		{
			Console.WriteLine($"AsyncAction1 {Thread.CurrentThread.ManagedThreadId}");

			Dispatcher.Current.Dispatch(DispatchPriority.High, NormalAction);
			Dispatcher.Current.Dispatch(DispatchPriority.Medium, NormalAction);
			Dispatcher.Current.Dispatch(DispatchPriority.Low, NormalAction);

			await Dispatcher.Yield(DispatchPriority.Medium);
		}

		static async ValueTask AsyncAction2()
		{
			Console.WriteLine($"AsyncAction2 {Thread.CurrentThread.ManagedThreadId}");

			await AsyncAction1();
			await Task.Yield();
			await AsyncAction1();
		}
	}
}
