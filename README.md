# CoroutineDispatcher

`CoroutineDispatcher` is a light framework for running multiple tasks asynchronously on a single thread.

### Why?

With heavily multi-threaded applications it can be difficult to manage data synchronization and ownership. Performing all related operations on a single thread can help in achieving cleaner and better organized code. In some cases it can even result it better performing applications. 

Of course it doesn’t mean that one has to be limited to just a single thread. The CoroutineDispatcher can be used to spawn multiple independent task queues that can communicate with each other by dispatching tasks on one another. 

### Inspiration

The `CoroutineDispatcher` is strongly inspired by the `System.Windows.Dispatcher` used within the WPF. Just instead of using the default windows message pump, it relays on a basic operation queue that is consumed within a relatively simple execution loop.

### What is it?

The heart of the `CoroutineDispatcher` is the `Dispatcher` class. You can think of it as a fancy priority queue that stores tasks to be run. By starting the `Dispatcher` you initialize an infinite loop that processes queued operations and waits for new ones once the list is empty.

Tasks can be scheduled beforehand or during execution (both from the thread on which the dispatcher is currently running as well as other threads).

On top of that simple concept the CoroutineDispatcher provides a handful of useful abstractions that that make it possible to use it within real world applications.

### How to use it?

Simply create a `Dispatcher`, queue (or not) some tasks and run it.

```csharp
var dispatcher = new Dispatcher();

dispatcher.Dispatch(() =>
{
	Console.Write(1);
	dispatcher.Dispatch(DispatchPriority.High, () => Console.Write(3));
	dispatcher.Dispatch(DispatchPriority.Low, () => Console.Write(5));
	Console.Write(2);
});
dispatcher.Dispatch(DispatchPriority.High, () => Console.Write(4));

dispatcher.Start(); // Prints "12345" and waits for new operations 
```

The `Dispatche` method is the simples of the task queueing operations. It’s a basic “fire and forget” where the calling thread is not interested in the result of the execution. 

An alternative to the `Dispatch` method is the `Invoke` method. It also queue the task on the dispatcher, but stops the calling thread until the execution is over. It allows us not only to synchronize operations, but also easily acquire results of computations.

```csharp
var dispatcher = new Dispatcher();

Task.Run(() => {
	int a = 2, b = 2; // This executes on a separate thread
	var sum = dispatcher.Invoke(() => {
		return a + b; // This executes on the dispatcher thread along with other operations (a and b are captured in a closure)
	});
	Console.Write(sum); // And here we are on a background thread again
});

dispatcher.Start();
```

(Please excuse the amount of lambdas used in those examples, but it's easier to create simple and compact snippets this way)

The `CoroutineDispatcher` (of course) also provides support for dispatching asynchronous operations.

```csharp
var dispatcher1 = new Dispatcher();
var dispatcher2 = Dispatcher.Spawn(); // Creates and starts a dispatcher on a new thread

dispatcher1.Dispatch(async () =>
{
	var r = 2.0;
	// While we await the result, the dispatcher1 is free to execute other pending operations
	var v = await dispatcher2.InvokeAsync(() => Math.PI * r * r);
	// The continuation of the awaited operation is properly dispatched back on the dispatcher1
	Console.Write(v);
});
dispatcher1.Dispatch(() => { });

dispatcher1.Start();
```

You can even intentionally yield execution back to the dispatcher if you are worried that other tasks might get starved during a long running operation.

```csharp
var dispatcher = new Dispatcher();

dispatcher.Dispatch(async () =>
{
	Console.Write(1);
	dispatcher.Dispatch(DispatchPriority.Low, () => Console.Write(4));
	dispatcher.Dispatch(DispatchPriority.High, () => Console.Write(2));
	Thread.Sleep(1000);

	// Will allow for execution of pending tasks with at least medium priority before continuing
	await Dispatcher.Yield(DispatchPriority.Medium);

	Thread.Sleep(1000);
	Console.Write(3);
});

dispatcher.Start(); // Will print "1234"
```

The last but not least trick is task scheduling with a delay. Those can be used to plan a one-off event or create a recurring operation.

```csharp
var dispatcher = new Dispatcher();

dispatcher.Schedule(TimeSpan.FromSeconds(30), () => Console.Write("At least 30 seconds have passed"));

dispatcher.Start();
```

Once you are bored you can also simply call the `Stop()` method. It will not terminate the currently executed task, but will prevent the dispatcher from consuming any new ones. But don’t warry. Queued tasks are not lost. Calling `Start()` again will resume the processing from where it was left on.

And from the most essential basics - that's it. Maybe not much, but for many usecases more then enough.

# `CoroutineDispatcher.Dispatcher`

### `Dispatcher Dispatcher.Current { get; }`

Returns a `Dispatcher` currently running on the calling thread. By using it you don't have to pass around the reference to your dispatcher.

### `void Dispatcher.Start()`

Starts an infinite loop on the current thread to process all queued and scheduled operations. Once the queue of operations is empty, waits for new ones to arrive.

### `void Dispatcher.Stop()`

Stops the current execution. If called from within an operation currently executed by the dispatcher, the operation itself will not be terminated, but no new operations will be acquired from the operation queue once the current operation finishes or yields.

### `void Dispatcher.Execute()`

Processes all queued operations until the active operation queue is empty. Performed execution will include all due scheduled operations, but will not wait for operations scheduled for after the active operation queue has been emptied nor the callbacks of awaited operations that did not finish yet.

### `bool Dispatcher.CheckAccess()`

Returns `true` if this instance of the `Dispatcher` is currently running on the calling thread.

### `static Dispatcher Dispatcher.Spawn()`

Creates a new `Dispatcher` and starts it on a new thread.

### `... Dispatcher.Invoke(...)`
###### `void Invoke(Action operation)`
###### `T Invoke<T>(Func<T> operation)`
###### `void Invoke(DispatchPriority priority, Action operation)`
###### `T Invoke<T>(DispatchPriority priority, Func<T> operation)`

Adds the `operation` to the operation queue and stops the current thread until it's executed.

```
dispatcher1      dispatcher2      thread_pool
     |                |                ‖     
     ∩                |                ‖     
     ‖  d2.Invoke()   ∩                ‖     
     ╙-------┐        ‖                ‖     
     .       |        U                ‖     
     .       └--------╖   d2.Invoke()  ‖     
     .                ‖       ┌--------╜     
     .                ‖       |        .     
     ╓----------------╜       |        .     
     ‖                ∩       |        .     
     U                U       |        .     
     |                ╓-------┘        .     
     ∩                ‖                .     
     ‖                ╙----------------╖     
     U                |                ‖     
```

# Contributions

I'm open to tickets and contributions.
