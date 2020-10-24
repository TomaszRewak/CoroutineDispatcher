using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoroutineDispatcher.Test
{
	public partial class OperationQueueTest
	{
		private OperationQueue _queue;
		private int _dequeuedValue;

		[TestInitialize]
		public void Setup()
		{
			_queue = new OperationQueue();
		}

		private void Enqueue(DispatchPriority priority, int value = 0)
		{
			_queue.Enqueue(priority, () => _dequeuedValue = value);
		}

		private void AssertDequeue(int expectedValue)
		{
			Assert.IsTrue(_queue.TryDequeue(out var operation));
			operation();
			Assert.AreEqual(expectedValue, _dequeuedValue);
		}

		private void AssertFailDequeue()
		{
			Assert.IsFalse(_queue.TryDequeue(out var operation));
		}

		private void AssertCount(int count, DispatchPriority minPriority = DispatchPriority.Low)
		{
			Assert.AreEqual(count, _queue.Count(minPriority));
		}

		private void AssertAny(DispatchPriority minPriority = DispatchPriority.Low)
		{
			Assert.IsTrue(_queue.Any(minPriority));
		}

		private void AssertNone(DispatchPriority minPriority = DispatchPriority.Low)
		{
			Assert.IsFalse(_queue.Any(minPriority));
		}
	}
}
