using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoroutineDispatcher.Test
{
	public partial class TimerQueueTest
	{
		private TimerQueue _queue;
		private List<int> _dequeuedValues;

		[TestInitialize]
		public void Setup()
		{
			_queue = new TimerQueue();
			_dequeuedValues = new List<int>();
		}

		private void Enqueue(DateTime dateTime, DispatchPriority priority, Action action)
		{
			_queue.Enqueue(dateTime, priority, action);
		}

		private void Enqueue(DateTime dateTime, DispatchPriority priority, int value = 0)
		{
			_queue.Enqueue(dateTime, priority, () => _dequeuedValues.Add(value));
		}

		private void AssertDequeue(int count)
		{
			Assert.AreEqual(count, _queue.Dequeue().Count);
		}

		private void AssertDequeue(int[] values)
		{
			_dequeuedValues.Clear();
			foreach (var (_, action) in _queue.Dequeue())
				action();

			CollectionAssert.AreEquivalent(values, _dequeuedValues);
		}
	}
}
