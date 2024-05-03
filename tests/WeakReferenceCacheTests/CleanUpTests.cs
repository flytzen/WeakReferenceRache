using System;
using System.Threading;
using Shouldly;
using WeakReferenceCache;
using Xunit;

namespace WeakReferenceCacheTests;

public class CleanUpTests
{
    [Fact]
    public void CanCleanUp()
    {
        int cacheHits = 0;
        int cacheMisses = 0;
        var sud = new WeakReferenceCache<object>(TimeSpan.FromMilliseconds(500));
        sud.CacheHit += () => Interlocked.Increment(ref cacheHits);
        sud.CacheMiss += () => Interlocked.Increment(ref cacheMisses);
        var key = "mykey";
        var item1 = sud.GetOrCreate(key, () => new object());
        Thread.Sleep(TimeSpan.FromMilliseconds(1000));
        GC.Collect();
        var item2 = sud.GetOrCreate(key, () => new object());

        item2.ShouldNotBe(item1);
        cacheMisses.ShouldBe(2);

    }
}