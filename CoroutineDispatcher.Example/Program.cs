using System;
using System.Threading.Tasks;

namespace CoroutineDispatcher.Example
{
	class Program
	{
		static void Main(string[] args)
		{
			var dispatcher = new Dispatcher();

			dispatcher.Dispatch(AsyncAction2);
			dispatcher.PushFrame();
		}

		static void NormalAction()
		{

		}

		static async ValueTask AsyncAction1()
		{
			Dispatcher.Current.Dispatch(DispatchPriority.High, NormalAction);
			Dispatcher.Current.Dispatch(DispatchPriority.Medium, NormalAction);

			await Dispatcher.Yield(DispatchPriority.High);

			Dispatcher.Current.Dispatch(NormalAction);
		}

		static async ValueTask AsyncAction2()
		{
			await AsyncAction1();
			await Task.Yield();
			await AsyncAction1();
		}
	}
}
