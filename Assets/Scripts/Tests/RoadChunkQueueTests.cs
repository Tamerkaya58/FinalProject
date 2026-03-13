using NUnit.Framework;
using UnityEngine;

public class RoadChunkQueueTests
{
    private RoadChunkQueue QueueInstance;
    private GameObject TestChunkOne;
    private GameObject TestChunkTwo;
    private GameObject TestChunkThree;
    private const int MaxCapacityLimit = 3;

    [SetUp]
    public void SetUpTestEnvironment()
    {
        QueueInstance = new RoadChunkQueue(MaxCapacityLimit);
        
        // Creating 3 distinct chunks to properly test the queue limits.
        TestChunkOne = new GameObject("Test_Chunk_First");
        TestChunkTwo = new GameObject("Test_Chunk_Second");
        TestChunkThree = new GameObject("Test_Chunk_Third");
    }

    [TearDown]
    public void CleanUpTestEnvironment()
    {
        // MERCILESS CLEANUP: If you create it, you must destroy it in tests.
        if (TestChunkOne != null) Object.DestroyImmediate(TestChunkOne);
        if (TestChunkTwo != null) Object.DestroyImmediate(TestChunkTwo);
        if (TestChunkThree != null) Object.DestroyImmediate(TestChunkThree);
    }

    [Test]
    public void EnqueueChunk_WhenThreeChunksAdded_ShouldReachMaxCapacity()
    {
        // 1 & 2: Set capacity to 3 and add 3 prefabs
        QueueInstance.EnqueueChunk(TestChunkOne);
        QueueInstance.EnqueueChunk(TestChunkTwo);
        QueueInstance.EnqueueChunk(TestChunkThree);

        // Verification
        Assert.IsTrue(QueueInstance.IsAtCapacity(), "System failed: The queue should be at max capacity (3) right now.");
    }

    [Test]
    public void DequeueAndDestroyOldestChunk_WhenAtMaxCapacity_ShouldRemoveFirstAddedChunk()
    {
        
    }
}