using System;
using System.Threading;
using Shouldly;
using WeakReferenceCache;
using Xunit;

namespace WeakReferenceCacheTests;

public class BasicCacheTests
{
    [Fact]
    public void CanCacheAndReceive()
    {
        var sud = new WeakReferenceCache<object>(TimeSpan.FromMinutes(20));
        var key = "mykey";
        var item1 = sud.GetOrCreate(key, () => new object());
        var item2 = sud.GetOrCreate(key, () => new object());

        item1.ShouldBeSameAs(item2);
    }

    [Fact]
    public void BasicHitAndMissCounters()
    {
        int cacheHits = 0;
        int cacheMisses = 0;
        var sud = new WeakReferenceCache<object>(TimeSpan.FromMinutes(20));
        sud.CacheHit += () => Interlocked.Increment(ref cacheHits);
        sud.CacheMiss += () => Interlocked.Increment(ref cacheMisses);
        var key1 = "mykey";
        var key2 = "mykey2";
        var item1 = sud.GetOrCreate(key1, () => new object());
        var item2 = sud.GetOrCreate(key1, () => new object());
        var item3 = sud.GetOrCreate(key2, () => new object());

        cacheHits.ShouldBe(1);
        cacheMisses.ShouldBe(2);

    }

    [Fact]
    public void CheckingShouldly1()
    {
        var item1 = new object();
        var sameAsItem1 = item1;
        item1.ShouldBeSameAs(sameAsItem1);
    }

    [Fact]
    public void CheckingShouldly2()
    {
        var item1 = new object();
        var item2 = new object();
        item1.ShouldNotBeSameAs(item2);

    }

}