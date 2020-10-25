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
		private DateTime _now;

		[TestInitialize]
		public void Setup()
		{
			_queue = new TimerQueue();
			_dequeuedValues = new List<int>();
			_now = DateTime.UtcNow;
		}

		private void Enqueue(TimeSpan timeSpan, DispatchPriority priority, Action action)
		{
			_queue.Enqueue(_now.Add(timeSpan), priority, action);
		}

		private void Enqueue(TimeSpan timeSpan, DispatchPriority priority, int value = 0)
		{
			_queue.Enqueue(_now.Add(timeSpan), priority, () => _dequeuedValues.Add(value));
		}

		private void AssertDequeue(int count)
		{
			Assert.IsTrue(_queue.TryDequeue(out var operations));
			Assert.AreEqual(count, operations.Count);
		}

		private void AssertFailDequeue()
		{
			Assert.IsFalse(_queue.TryDequeue(out var _));
		}

		private void AssertDequeue(int[] values)
		{
			Assert.IsTrue(_queue.TryDequeue(out var operations));

			_dequeuedValues.Clear();
			foreach (var (_, operation) in operations)
				operation();

			CollectionAssert.AreEquivalent(values, _dequeuedValues);
		}

		private void AssertNoNext()
		{
			Assert.IsNull(_queue.Next);
		}

		private void AssertNext(TimeSpan timeSpan)
		{
			Assert.AreEqual(_now.Add(timeSpan), _queue.Next);
		}
	}
}
