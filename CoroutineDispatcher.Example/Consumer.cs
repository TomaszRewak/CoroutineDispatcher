using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher.Example
{
	internal sealed class Consumer
	{
		private readonly Dispatcher _dispatcher = Dispatcher.Spawn();
		private readonly HashSet<string> _items = new HashSet<string>();

		public void Dispose()
		{
			_dispatcher.Stop();
		}

		public void Consume(string item) =>
		_dispatcher.Dispatch(async () =>
		{
			Log($"  [start] Consuming {item}");

			_items.Add(item);

			if (await CheckOnTheServer(item))
			{
				await Process(item);
			}
			else
			{
				_dispatcher.Dispatch(DispatchPriority.Low, () => Remove(item));
			}

			Log($"  [end] Consuming {item}");
		});

		public int GetCount() =>
		_dispatcher.Invoke(() =>
		{
			Log($"Getting count");
			return _items.Count;
		});

		private async Task<bool> CheckOnTheServer(string item)
		{
			Log($"    [start] Checking on server {item}");
			await Task.Delay(TimeSpan.FromSeconds(4));
			var result = new Random().Next() % 2 == 0;
			Log($"    [end] Checking on server {item} ({result})");

			return result;
		}

		private async Task Process(string item)
		{
			Log($"∩   [step 1] Processing {item}");
			Thread.Sleep(1000);
			Log($"U   [step 2] Processing {item}");

			await Dispatcher.Yield(DispatchPriority.Medium);

			Log($"∩   [step 3] Processing {item}");
			Thread.Sleep(1000);
			Log($"U   [step 4] Processing {item}");
		}

		private void Remove(string item)
		{
			Log($"    Removing {item}");
		}

		private static void Log(string text) => Program.Log(1, text);
	}
}
