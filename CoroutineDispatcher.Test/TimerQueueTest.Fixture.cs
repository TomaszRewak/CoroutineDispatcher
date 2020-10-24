using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoroutineDispatcher.Test
{
	public partial class TimerQueueTest
	{
		private TimerQueue _queue;
		private int _dequeuedValue;

		[TestInitialize]
		public void Setup()
		{
			_queue = new TimerQueue();
		}

		private void Enqueue(Action action)
		{
			_queue.Enqueue(DateTime.MinValue, DispatchPriority.Medium, action);
		}

		private void Enqueue(TimeSpan timeSpan, DispatchPriority priority, int value = 0)
		{
			_queue.Enqueue(DateTime.UtcNow.Add(timeSpan), priority, () => _dequeuedValue = value);
		}

		private void AssertDequeue()
		{
			Assert.IsTrue(_queue.TryDequeue(out var _, out var _));
		}

		private void AssertFailDequeue()
		{
			Assert.IsFalse(_queue.TryDequeue(out var _, out var _));
		}
	}
}
