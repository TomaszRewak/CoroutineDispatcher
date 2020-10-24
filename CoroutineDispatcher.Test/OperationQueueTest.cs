using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoroutineDispatcher.Test
{
	[TestClass]
	public partial class OperationQueueTest
	{
		[TestMethod]
		public void NewQueueHasNoElements()
		{
			AssertCount(0);
			AssertNone();
		}

		[TestMethod]
		public void EmptyQueueCannotBeDequeued()
		{
			AssertFailDequeue();
		}

		[TestMethod]
		public void QueuedOperationsAreCountedIn()
		{
			Enqueue(DispatchPriority.Medium);
			Enqueue(DispatchPriority.Medium);

			AssertCount(2);
			AssertAny();
		}

		[TestMethod]
		public void QueueCountsOnlyOperationsAboveMinimumPriority()
		{
			Enqueue(DispatchPriority.Low);
			Enqueue(DispatchPriority.Low);
			Enqueue(DispatchPriority.Medium);
			Enqueue(DispatchPriority.Medium);
			Enqueue(DispatchPriority.Medium);
			Enqueue(DispatchPriority.High);

			AssertCount(1, DispatchPriority.High);
			AssertCount(4, DispatchPriority.Medium);
			AssertCount(6, DispatchPriority.Low);
		}

		[TestMethod]
		public void OperateionsAreDequeuedAccordingToThePriority()
		{
			Enqueue(DispatchPriority.High, 0);
			Enqueue(DispatchPriority.Low, 1);
			Enqueue(DispatchPriority.Medium, 2);

			AssertDequeue(0);
			AssertDequeue(2);
			AssertDequeue(1);
		}

		[TestMethod]
		public void DequeuedOperationsAreRemovedFromTheQueue()
		{
			Enqueue(DispatchPriority.Medium, 0);
			Enqueue(DispatchPriority.Medium, 1);

			AssertDequeue(0);

			AssertCount(1);
		}

		[TestMethod]
		public void FullyDequeuedQueueIsEmpty()
		{
			Enqueue(DispatchPriority.Medium, 0);
			Enqueue(DispatchPriority.Medium, 1);

			AssertDequeue(0);
			AssertDequeue(1);

			AssertNone();
		}

		[TestMethod]
		public void QueueUsesStableSortingWithinSinglePriority()
		{
			Enqueue(DispatchPriority.Low, 0);
			Enqueue(DispatchPriority.Medium, 1);
			Enqueue(DispatchPriority.Medium, 2);
			Enqueue(DispatchPriority.High, 3);
			Enqueue(DispatchPriority.Medium, 4);
			Enqueue(DispatchPriority.High, 5);
			Enqueue(DispatchPriority.Medium, 6);
			Enqueue(DispatchPriority.Low, 7);

			AssertDequeue(3);
			AssertDequeue(5);

			AssertDequeue(1);
			AssertDequeue(2);
			AssertDequeue(4);
			AssertDequeue(6);

			AssertDequeue(0);
			AssertDequeue(7);
		}
	}
}
