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
		return a + b; // This executes on the dispatcher thread along with other operations
	});
	Console.Write(sum); // And here we are on a background thread again
});

dispatcher.Start();
```
