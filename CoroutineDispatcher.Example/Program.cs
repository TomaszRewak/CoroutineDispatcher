using System;
using System.Threading;

namespace CoroutineDispatcher.Example
{
	class Program
	{
		static void Main()
		{
			Log("Starting");

			var consumer = new Consumer();
			var producer = new Producer(consumer);

			producer.StartProducing("car", TimeSpan.FromSeconds(5));
			producer.StartProducing("computer", TimeSpan.FromSeconds(3));

			var mainDispatcher = new Dispatcher();
			mainDispatcher.Dispatch(async () =>
			{
				while (true)
				{
					var key = Console.ReadKey();

					switch (key.Key)
					{
						case ConsoleKey.C:
							Log("C received");
							Dispatcher.Current.Dispatch(DispatchPriority.High, () =>
							{
								Log($"Counting");
								Log($"Count = {consumer.GetCount()}");
							});
							Log("C dispatcher");
							break;
						case ConsoleKey.P:
							Log("P received");
							Dispatcher.Current.Dispatch(DispatchPriority.Low, () =>
							{
								Log($"Ping");
							});
							Log("P dispatcher");
							break;
						case ConsoleKey.E:
							Log("E pressed");
							Dispatcher.Current.Stop();
							return;
					}

					await Dispatcher.Yield(DispatchPriority.Medium);
				}
			});

			Log("Starting main dispatcher");
			mainDispatcher.Start();
			Log("Stopped main dispatcher");

			Log("Flushing what's left in the main dispatcher");

			mainDispatcher.Execute();

			Log("Stopping consumers");

			producer.Dispose();
			consumer.Dispose();
		}

		private static void Log(string text) => Log(0, text);
		public static void Log(int indent, string text)
		{
			Console.WriteLine($"{new string('\t', indent * 8)} [thread {Thread.CurrentThread.ManagedThreadId}] {text}");
		}
	}
}
