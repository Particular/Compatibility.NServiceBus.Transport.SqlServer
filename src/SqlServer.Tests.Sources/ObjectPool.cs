using System;
using System.Collections.Concurrent;

class ObjectPool<T>(Func<T> objectGenerator)
{
    readonly ConcurrentBag<T> _objects = [];
    readonly Func<T> _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));

    public T Get() => _objects.TryTake(out T item) ? item : _objectGenerator();

    public void Return(T item) => _objects.Add(item);
}