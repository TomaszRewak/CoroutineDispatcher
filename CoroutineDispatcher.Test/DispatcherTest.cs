using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoroutineDispatcher.Test
{
	[TestClass]
	public partial class DispatcherTest
	{
		[TestInitialize]
		public void Setup()
		{
			_dispatcher = new Dispatcher();
		}

		[TestMethod]
		public void ExecutesQueuedOperations()
		{
			Dispatch(() => AssertCall(1));
			Dispatch(() => AssertCall(2));

			Execute();

			AssertTotalCalls(2);
		}

		[TestMethod]
		public void ExecutesQueuedOperationsAccordingToPriority()
		{
			Dispatch(DispatchPriority.Low, () => AssertCall(3));
			Dispatch(DispatchPriority.High, () => AssertCall(1));
			Dispatch(DispatchPriority.Medium, () => AssertCall(2));

			Execute();

			AssertTotalCalls(3);
		}

		[TestMethod]
		public void UsesStableSortingForPriorityLevels()
		{
			Dispatch(DispatchPriority.Low, () => AssertCall(4));
			Dispatch(DispatchPriority.High, () => AssertCall(1));
			Dispatch(DispatchPriority.Low, () => AssertCall(5));
			Dispatch(DispatchPriority.Low, () => AssertCall(6));
			Dispatch(DispatchPriority.Medium, () => AssertCall(3));
			Dispatch(DispatchPriority.High, () => AssertCall(2));

			Execute();

			AssertTotalCalls(6);
		}

		[TestMethod]
		public void SetsCurrentDispatcherDuringExecution()
		{
			Dispatch(() => Assert.AreEqual(_dispatcher, Dispatcher.Current));
			Execute();
		}

		[TestMethod]
		public void ClearsCurrentDispatcherAfterExecution()
		{
			Execute();
			Assert.IsNull(Dispatcher.Current);
		}

		[TestMethod]
		public void QueuesTasksDispatchedDuringExecution()
		{
			Dispatch(() => {
				AssertCall(1);
				Dispatch(() => AssertCall(3));
				Dispatch(() => AssertCall(4));
				AssertCall(2);
			});

			Execute();

			AssertTotalCalls(4);
		}

		[TestMethod]
		public void FinishesCurrentOperationBeforeExecutingHigherPriorityOperations()
		{
			Dispatch(DispatchPriority.Low, () => {
				AssertCall(1);
				Dispatch(DispatchPriority.High, () => AssertCall(3));
				AssertCall(2);
			});

			Execute();

			AssertTotalCalls(3);
		}

		[TestMethod]
		public void YieldsExecutionToHigherPriorityOperations()
		{
			Dispatch(DispatchPriority.Medium, async () => {
				AssertCall(1);
				Dispatch(DispatchPriority.Medium, () => AssertCall(4));
				Dispatch(DispatchPriority.High, () => AssertCall(3));
				Dispatch(DispatchPriority.Low, () => AssertCall(6));
				AssertCall(2);
				await Dispatcher.Yield(DispatchPriority.Medium);
				AssertCall(5);
			});

			Execute();

			AssertTotalCalls(6);
		}
	}
}
