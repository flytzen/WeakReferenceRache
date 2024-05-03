namespace WeakReferenceCache;

internal class CacheItemEnvelope<T> where T: class
{
    private object locker = new();

    public CacheItemEnvelope()
    {
        this.LastAccessed = DateTime.UtcNow;
    }

    private WeakReference? WeakReference { get; set;  }

    private T? StrongReference { get; set; }

    internal DateTime LastAccessed { get; private set; } = DateTime.UtcNow;

    internal bool IsValid => this.WeakReference != null && this.WeakReference.IsAlive;

    // TODO: Some kind of clean up task that removes strong ref if necessary

    // // public bool TryGetReference([NotNullWhen(returnValue: true)] out T? item)
    // // {
    // //     this.LastAccessed = DateTime.UtcNow;

    // //     // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator
    // //     // Too clever? It was the editors fault for suggesting it
    // //     // If the strong reference is not null, return it. Otherwise assign the weak reference target to the strong reference and return it
    // //     // Both the strong and the weak reference may be null so null can still be returned.
    // //     item = (this.StrongReference ??= this.WeakReference?.Target as T);

    // //     return item != null;
    // // }

    public T GetOrCreate(Func<T> itemFactory, out bool newItemWasCreated, out bool retrievedFromWeakReference)
    {
        this.LastAccessed = DateTime.UtcNow;
        newItemWasCreated = false;
        retrievedFromWeakReference = false;
        bool strongReferenceIsNull = this.StrongReference is null; // Just used for statistics
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator
        // Too clever? It was the editors fault for suggesting it
        // If the strong reference is not null, return it. Otherwise assign the weak reference target to the strong reference and return it
        // Both the strong and the weak reference may be null so null can still be returned.
        var item = this.StrongReference ??= this.WeakReference?.Target as T;
        if (item != null)
        {
            // If the strong reference was null but the item now exists, it was found from the weak reference.
            // There are race conditions where this is not stricly true, but they are not overly important
            retrievedFromWeakReference = strongReferenceIsNull;
            return item;
        }

        lock (this.locker)
        {
            // Try to get it again, in case someone else created it first
            item = this.StrongReference ??= this.WeakReference?.Target as T;
            if (item != null)
            {
                return item;
            }

            // We need to create it
            newItemWasCreated = true;
            item = itemFactory();
            this.WeakReference = new(item);
            this.StrongReference = item;
            return item;
        }

    }
}