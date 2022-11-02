using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using WeakReferenceCache;
using Xunit;

namespace WeakReferenceCacheTests;

public class ConcurrencyTests 
{
    public ConcurrencyTests()
    {
        ThreadPool.SetMinThreads(100, 100);
    }

    [Fact]
    public async void RunFactoryOnlyOnce()
    {
        int cacheHits = 0;
        int cacheMisses = 0;
        int factoryRan = 0;
        var sud = new WeakReferenceCache<object>(TimeSpan.FromMinutes(20));
        sud.CacheHit += () => Interlocked.Increment(ref cacheHits);
        sud.CacheMiss += () => Interlocked.Increment(ref cacheMisses);

        var tasks = Enumerable.Range(0, 100)
        .Select(_ => Task.Run(
            () => {
                sud.GetOrCreate("key", 
                    () => {
                        Thread.Sleep(10);
                        Interlocked.Increment(ref factoryRan);
                        return new object(); 
                    }); 
            }
            )).ToList();

        foreach (var task in tasks.ToList())   
        {
            await task;
        }
        
        cacheMisses.ShouldBe(1);
        factoryRan.ShouldBe(1);
        cacheHits.ShouldBe(99);
    }
}