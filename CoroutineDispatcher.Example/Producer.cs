using System;

namespace CoroutineDispatcher.Example
{
	internal sealed class Producer
	{
		private readonly Consumer _consumer;
		private readonly Dispatcher _dispatcher;

		private int _counter;

		public Producer(Consumer consumer)
		{
			_consumer = consumer;
			_dispatcher = Dispatcher.Spawn(Initialize);
		}

		public void Dispose()
		{
			_dispatcher.Stop();
		}

		public void StartProducing(string item, TimeSpan interval)
		{
			Log($"  Starting production {item}");
			_dispatcher.Dispatch(() => Produce(item, interval));
		}

		private void Produce(string item, TimeSpan interval)
		{
			_counter += 1;

			Log($"∩   [start] Producing {item} no {_counter}");

			_consumer.Consume($"{item}_{_counter}");
			_dispatcher.Schedule(interval, () => Produce(item, interval));

			Log($"U   [end] Producing {item} no {_counter}");
		}

		private void Initialize()
		{
			Log($"  Initializing producer");
		}

		private static void Log(string text) => Program.Log(2, text);

	}
}
