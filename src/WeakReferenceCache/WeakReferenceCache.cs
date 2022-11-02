namespace WeakReferenceCache;

public class WeakReferenceCache<T> where T : class
{
    private readonly TimeSpan rollingLifeTime;
    
    /// <summary>
    /// Only remove items from the dictionary if you can get a writer lock on this.
    /// </summary>
    private readonly ReaderWriterLockSlim itemRemovalLock = new();

    // TODO: Implement something to make strong references weak when they expire
    // TODO: Implement removal of no-longer-valid cache items (i.e. the weak reference is no longer valid)
    // TODO: Add telemetry points for use of weak references

    private readonly Dictionary<string, CacheItemEnvelope<T>> items = new();

    public event Action? CacheHit;
    public event Action? CacheMiss;

    /// <summary>
    /// Creates a new instance of the cache.
    /// NOTE: Each instance is completely separate - make sure you use a singleton!
    /// </summary>
    /// <param name="rollingLifeTime">How long items should keep a strong reference after they were last accessed.</param>
    public WeakReferenceCache(TimeSpan rollingLifeTime)
    {
        this.rollingLifeTime = rollingLifeTime;
    }

    /// <summary>
    /// Get an item from the cache or create a new one and store it.
    /// If your factory method is async, cache the Task instead of awaiting your factory method.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns>T</returns>
    public T GetOrCreate(string key, Func<T> factory)
    {
        this.itemRemovalLock.EnterReadLock();
        try
        {
            // Either get the cache item envelope with that key or add a new, empty envlope.
            if (!this.items.TryGetValue(key, out CacheItemEnvelope<T>? cacheItem))
            {
                this.items.TryAdd(key, new CacheItemEnvelope<T>());
                cacheItem = this.items[key];
            }

            var itemFromCache = cacheItem.GetOrCreate(factory, out bool newItemWasCreated);
            if (newItemWasCreated)
            {
                this.NotifyCacheMiss();
            } 
            else 
            {
                this.NotifyCacheHit();
            }

            return itemFromCache;
            
        }
        finally
        {
            this.itemRemovalLock.ExitReadLock();
        }
    }

    private void RemoveExpiredItems()
    {
        // If we can't get a write lock in a second, just come back to it on the next loop.
        if (this.itemRemovalLock.TryEnterWriteLock(1000))
        {
            try
            {
                throw new NotImplementedException();
            }
            finally
            {
                this.itemRemovalLock.ExitWriteLock();
            }
        }
    }

    private void NotifyCacheHit()
    {
        this.CacheHit?.Invoke();
    }

    private void NotifyCacheMiss()
    {
        this.CacheMiss?.Invoke();
    }

}
