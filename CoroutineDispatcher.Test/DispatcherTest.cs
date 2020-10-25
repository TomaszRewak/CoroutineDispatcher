using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher.Test
{
	[TestClass]
	public partial class DispatcherTest
	{
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
			Dispatch(() =>
			{
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
			Dispatch(DispatchPriority.Low, () =>
			{
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
			Dispatch(DispatchPriority.Medium, async () =>
			{
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

		[TestMethod]
		public void SetsSynchronizationContextDuringExecution()
		{
			Dispatch(() =>
			{
				Assert.IsInstanceOfType(SynchronizationContext.Current, typeof(CoroutineSynchronizationContext));
			});

			Execute();
		}

		[TestMethod]
		public void RestoresSynchronizationContextAfterExecution()
		{
			Execute();
			Assert.IsNotInstanceOfType(SynchronizationContext.Current, typeof(CoroutineSynchronizationContext));
		}

		[TestMethod, ExpectedException(typeof(DispatcherException))]
		public async Task YieldingDispatcherOutsideTheExecutionCycleThrowsAnException()
		{
			await Dispatcher.Yield(DispatchPriority.Medium);
		}

		[TestMethod]
		public void ExecutionIsContinuedOnTheMainThreadAfterAwaitingSecondThread()
		{
			Dispatch(async () =>
			{
				AssertCall(1);
				await Task.Run(() => AssertSecondThreadCall(2));
				AssertCall(3);
				Stop();
			});

			Start();

			AssertTotalCalls(3);
		}

		[TestMethod]
		public void OtherOperationsAreExecutedWhileAwaitingSecondThread()
		{
			Dispatch(DispatchPriority.High, async () =>
			{
				Dispatch(DispatchPriority.Low, () => AssertCall(2));
				AssertCall(1);
				await Task.Run(() => Thread.Sleep(50));
				AssertCall(3);
				Stop();
			});

			Start();

			AssertTotalCalls(3);
		}

		[TestMethod]
		public void ScheduledOperationsAreNotExecutedBeforeTheTimeout()
		{
			Schedule(TimeSpan.FromMilliseconds(100), () => AssertCall(3));
			Dispatch(async () =>
			{
				AssertCall(1);
				await Task.Delay(50);
				AssertCall(2);
				await Task.Delay(100);
				AssertCall(4);
				Stop();
			});

			Start();

			AssertTotalCalls(4);
		}

		[TestMethod]
		public void MultipleScheduledOperatinosAreExecutedAccordingToTheirTimeout()
		{
			Schedule(TimeSpan.FromMilliseconds(300), () => AssertCall(7));
			Schedule(TimeSpan.FromMilliseconds(100), () => AssertCall(3));
			Schedule(TimeSpan.FromMilliseconds(200), () => AssertCall(5));
			Dispatch(async () =>
			{
				AssertCall(1);
				await Task.Delay(50);
				AssertCall(2);
				await Task.Delay(100);
				AssertCall(4);
				await Task.Delay(100);
				AssertCall(6);
				await Task.Delay(100);
				AssertCall(8);
				Stop();
			});

			Start();

			AssertTotalCalls(8);
		}

		[TestMethod]
		public void OperationsScheduledInThePastAreQueuedRightAway()
		{
			Dispatch(DispatchPriority.Medium, () =>
			{
				AssertCall(1);
				Dispatch(DispatchPriority.High, () => AssertCall(3));
				Dispatch(DispatchPriority.Low, () => AssertCall(5));
				Schedule(TimeSpan.FromSeconds(-1), DispatchPriority.Medium, () => AssertCall(4));
				AssertCall(2);
			});

			Execute();

			AssertTotalCalls(5);
		}

		[TestMethod]
		public void StoppingStopsAlsoContinuousExecution()
		{
			Dispatch(() => AssertCall(1));
			Dispatch(() => AssertCall(2));
			Dispatch(() =>
			{
				AssertCall(3);
				Stop();
			});
			Dispatch(() => AssertCall(4));

			Execute();

			AssertTotalCalls(3);
		}

		[TestMethod]
		public void ScheduledOperationsAreIncludedDuringContinuousExecutionWithCorrectPriority()
		{
			Schedule(TimeSpan.FromMilliseconds(100), DispatchPriority.Medium, () => throw new InvalidOperationException());
			Schedule(TimeSpan.FromMilliseconds(200), DispatchPriority.High, () => Stop());
			void Loop() { Dispatch(DispatchPriority.High, Loop); }
			Loop();

			Execute();
		}

		[TestMethod]
		public void InvokingBlockTheCallingThread()
		{
			Dispatch(() =>
			{
				AssertCall(1);
				Task.Run(() => Invoke(() => AssertSecondThreadCall(3)));
				Thread.Sleep(100);
				Dispatch(() =>
				{
					AssertCall(4);
					Stop();
				});
				AssertCall(2);
			});

			Start();
			AssertTotalCalls(4);
		}

		[TestMethod]
		public void InvokedOperationCanRetrunAValue()
		{
			Dispatch(() =>
			{
				AssertCall(1);
				Task.Run(() =>
				{
					AssertSecondThreadCall(3);
					Assert.AreEqual(123, Invoke(() =>
					{
						AssertCall(4);
						return 123;
					}));
					AssertSecondThreadCall(5);
					Stop();
				});
				AssertCall(2);
			});

			Start();
			AssertTotalCalls(5);
		}

		[TestMethod]
		public void AsyncInvokeDoesNotBlockTheCallingThread()
		{
			var second = Dispatcher.Spawn();

			Dispatch(async () =>
			{
				Dispatch(() => AssertCall(2));
				AssertCall(1);
				await second.InvokeAsync(() =>
				{
					Thread.Sleep(50);
					AssertSecondThreadCall(3);
				});
				AssertCall(4);
				Stop();
			});

			Start();
			AssertTotalCalls(4);
		}

		[TestMethod]
		public void AwaitCallbackIsDispatchedWithHighPriority()
		{
			Dispatch(async () =>
			{
				void Loop() { Dispatch(DispatchPriority.High, Loop); }
				Loop();

				await Task.Delay(100);
				Stop();
			});

			Start();
		}

		[TestMethod]
		public void InvokingTheSameThreadExecutesTheOperationRightAway()
		{
			Dispatch(() =>
			{
				AssertCall(1);
				Dispatch(DispatchPriority.High, () => AssertCall(5));
				Invoke(DispatchPriority.Low, () => AssertCall(2));
				Assert.AreEqual(123, Invoke(DispatchPriority.Low, () =>
				{
					AssertCall(3);
					return 123;
				}));
				AssertCall(4);
			});

			Execute();
			AssertTotalCalls(5);
		}
	}
}
