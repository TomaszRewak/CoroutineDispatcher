using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoroutineDispatcher.Test
{
	[TestClass]
	public partial class TimerQueueTest
	{
		[TestMethod]
		public void EmptyCollectionHasNothingToDequeue()
		{
			AssertFailDequeue();
		}

		[TestMethod]
		public void EnqueueingSingleOperationTwiceStoresItTwice()
		{
			Action operation = () => { };

			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, operation);
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, operation);

			AssertDequeue(2);
		}

		[TestMethod]
		public void DequeuingOperationsRemovesThem()
		{
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium);
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium);

			AssertDequeue(2);
			AssertFailDequeue();
		}

		[TestMethod]
		public void OnlyPastOperationsAreDequeued()
		{
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, 1);
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, 2);
			Enqueue(TimeSpan.FromSeconds(1), DispatchPriority.Medium, 3);
			Enqueue(TimeSpan.FromSeconds(1), DispatchPriority.Medium, 4);

			AssertDequeue(new[] { 1, 2 });
			AssertFailDequeue();
		}

		[TestMethod]
		public void DequeuingIsNotImpactedByEnqueuingOrder()
		{
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, 1);
			Enqueue(TimeSpan.FromSeconds(1), DispatchPriority.Medium, 2);
			Enqueue(TimeSpan.FromSeconds(1), DispatchPriority.Medium, 3);
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, 4);

			AssertDequeue(new[] { 1, 4 });
		}

		[TestMethod]
		public void OnlyTheOldestOperatinosAreDequeuedInASingleStep()
		{
			Enqueue(TimeSpan.FromSeconds(-2), DispatchPriority.Medium, 1);
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, 2);
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, 3);
			Enqueue(TimeSpan.FromSeconds(1), DispatchPriority.Medium, 4);

			AssertDequeue(new[] { 1 });
			AssertDequeue(new[] { 2, 3 });
			AssertFailDequeue();
		}

		[TestMethod]
		public void EmptyQueueHasNoNextDate()
		{
			AssertNoNext();
		}

		[TestMethod]
		public void NextReturnsTheEarliestDate()
		{
			Enqueue(TimeSpan.FromSeconds(-1), DispatchPriority.Medium);
			Enqueue(TimeSpan.FromSeconds(-4), DispatchPriority.Medium);
			Enqueue(TimeSpan.FromSeconds(-2), DispatchPriority.Medium);
			Enqueue(TimeSpan.FromSeconds(6), DispatchPriority.Medium);

			AssertNext(TimeSpan.FromSeconds(-4));
		}

		[TestMethod]
		public void NextIsUpdatedAfterDequeuing()
		{
			Enqueue(TimeSpan.FromSeconds(-4), DispatchPriority.Medium);
			Enqueue(TimeSpan.FromSeconds(-4), DispatchPriority.Medium);
			Enqueue(TimeSpan.FromSeconds(3), DispatchPriority.Medium);
			Enqueue(TimeSpan.FromSeconds(7), DispatchPriority.Medium);

			AssertDequeue(2);

			AssertNext(TimeSpan.FromSeconds(3));
		}
	}
}
