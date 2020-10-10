using System;
using System.Threading.Tasks;

namespace CoroutineDispatcher.Example
{
	class Program
	{
		static async void Main(string[] args)
		{
			var dispatcher = new Dispatcher();

			await dispatcher.InvokeAsync(AsyncAction2);
		}

		static void NormalAction()
		{

		}

		static async ValueTask AsyncAction1()
		{
			Dispatcher.Current.Dispatch(DispatchPriority.Medium, NormalAction);
			Dispatcher.Current.Dispatch(DispatchPriority.Low, NormalAction);

			await Dispatcher.Yield(DispatchPriority.Medium);

			Dispatcher.Current.Dispatch(NormalAction);
		}

		static async ValueTask AsyncAction2()
		{
			await AsyncAction1();
			await Dispatcher.Yield();
			await AsyncAction1();
		}
	}
}
