using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;


public class RoadProcessing
{
    private LevelData CurrentLevelData;
    private GameObject RoadChunkPrefab;
    private float NextSpawnZPosition = 0f;
    private int ChunkMaxCapacity = 3;
    private int ChunkHowManyWhenTriggered = 1;
    
    private float ChunkLength = 0f; 
    private float RoadBoundaryX = 0f; 
    
    // Toplam ağırlık (weighted spawn için cache)
    private float TotalWeight = 0f;
    
    private Queue<GameObject> ActiveChunksQueue;
    
    // Her chunk'a ait spawn edilmiş objeleri tutar (SetParent kullanmadan)
    private Dictionary<GameObject, List<GameObject>> ChunkObjects = new Dictionary<GameObject, List<GameObject>>();

    // ==================== CONSTRUCTOR ====================

    public RoadProcessing(LevelData LevelDataInput)
    {
        CurrentLevelData = LevelDataInput;
        RoadChunkPrefab = CurrentLevelData.RoadChunk;

        ActiveChunksQueue = new Queue<GameObject>();

        // 1. Sınırları ve uzunluğu hesapla
        CalculateRoadBoundaries();
        
        // 2. Toplam ağırlığı hesapla (obje yerleştirme için)
        CalculateTotalWeight();
        
        // 3. Başlangıç yol parçalarını oluştur
        InitialChunkSpawner();
        
        // 4. RoadTrigger event'ine abone ol
        SubscribeToTrigger();
    }

    // ==================== EVENT YÖNETİMİ ====================
    
    private void SubscribeToTrigger()
    {
        RoadTrigger.OnCarPassedTrigger += ChunkChangePlacesWhenTriggered;
    }

    /// <summary>
    /// Event aboneliğini kaldırır. GameManager sahne kapanırken bunu çağırmalıdır.
    /// </summary>
    public void Cleanup()
    {
        RoadTrigger.OnCarPassedTrigger -= ChunkChangePlacesWhenTriggered;
    }

    // ==================== HESAPLAMALAR ====================

    private void CalculateRoadBoundaries()
{
    Transform FirstChild = RoadChunkPrefab.transform.GetChild(0);
    BoxCollider ChildCollider = FirstChild.GetComponent<BoxCollider>();

    if (ChildCollider != null)
    {
        // STEP 1: Calculate scales for Z-axis (Length) considering the entire hierarchy
        float ChildScaleZ = FirstChild.transform.localScale.z;
        float ParentScaleZ = RoadChunkPrefab.transform.localScale.z;
        
        // Multiplying collider size with both child and parent local scales
        ChunkLength = ChildCollider.size.z * ChildScaleZ * ParentScaleZ;
        
        // STEP 2: Calculate scales for X-axis (Width) cleanly and correctly in one go
        float ChildScaleX = FirstChild.transform.localScale.x;
        float ParentScaleX = RoadChunkPrefab.transform.localScale.x;
        
        float ActualWidth = ChildCollider.size.x * ChildScaleX * ParentScaleX;
        
        // STEP 3: Calculate boundaries with the safety margin
        RoadBoundaryX = (ActualWidth / 2f) * 0.975f;
        
        Debug.Log($"ChunkLength: {ChunkLength}, RoadBoundaryX: {RoadBoundaryX}");
    }
    else
    {
        Debug.LogError("FATAL ERROR: RoadChunk Prefab lacks a child gameobject with a BoxCollider! Falling back to default values.");
        ChunkLength = 100f;
        RoadBoundaryX = 3.2f;
    }
}

    private void CalculateTotalWeight()
    {
        TotalWeight = 0f;
        if (CurrentLevelData.SpawnableObjects == null) return;
        
        foreach (var Spawn in CurrentLevelData.SpawnableObjects)
        {
            TotalWeight += Spawn.Weight;
        }
    }

    // ==================== CHUNK SPAWN ====================

    private void InitialChunkSpawner()
    {
        for (int Index = 0; Index < ChunkMaxCapacity; Index++)
        {
            SpawnAndEnqueueChunk();
        }
    }

    /// <summary>
    /// Yeni bir chunk oluşturur, objeleri yerleştirir ve kuyruğa ekler.
    /// </summary>
    private GameObject SpawnAndEnqueueChunk()
    {
        Vector3 SpawnPosition = new Vector3(0, 0, NextSpawnZPosition);
        
        GameObject NewChunk = GameObject.Instantiate(RoadChunkPrefab, SpawnPosition, Quaternion.identity);
        NewChunk.name = "RoadChunk_" + NextSpawnZPosition;
        
        PlaceObstacles(NewChunk);
        PlaceCoins(NewChunk);
        PlaceBarriers(NewChunk);
        
        ActiveChunksQueue.Enqueue(NewChunk);
        NextSpawnZPosition += ChunkLength;
        
        return NewChunk;
    }

    // ==================== TRIGGER → CHUNK GERİ DÖNÜŞÜMÜ ====================

    /// <summary>
    /// RoadTrigger tetiklendiğinde çağrılır.
    /// En arkadaki chunk'ı en öne taşır, temizler, yeniden doldurur.
    /// </summary>
    public void ChunkChangePlacesWhenTriggered()
    {
        for (int i = 0; i < ChunkHowManyWhenTriggered; i++)
        {
            if (ActiveChunksQueue.Count == 0)
            {
                Debug.LogWarning("RoadProcessing: Kuyrukta taşınacak chunk kalmadı!");
                return;
            }

            // 1. En eski chunk'ı kuyruktan çıkar
            GameObject OldestChunk = ActiveChunksQueue.Dequeue();

            // 2. Üzerindeki eski objeleri temizle
            ClearChunkChildren(OldestChunk);

            // 3. En öne taşı
            OldestChunk.transform.position = new Vector3(0, 0, NextSpawnZPosition);
            OldestChunk.name = "RoadChunk_" + NextSpawnZPosition;

            // 4. Yeni engeller, coinler ve bariyerler yerleştir
            PlaceObstacles(OldestChunk);
            PlaceCoins(OldestChunk);
            PlaceBarriers(OldestChunk);

            // 5. Kuyruğun sonuna tekrar ekle
            ActiveChunksQueue.Enqueue(OldestChunk);

            // 6. Sonraki spawn pozisyonunu güncelle
            NextSpawnZPosition += ChunkLength;
        }
    }

    private void ClearChunkChildren(GameObject Chunk)
    {
        // SetParent yerine Dictionary kullandığımız için buradan temizle
        if (ChunkObjects.TryGetValue(Chunk, out List<GameObject> Objects))
        {
            foreach (GameObject Obj in Objects)
            {
                if (Obj != null) GameObject.Destroy(Obj);
            }
            Objects.Clear();
        }
    }

    // ==================== ENGEL YERLEŞTİRME ====================

    /// <summary>
    /// Chunk üzerine LevelData'daki ağırlıklı listeye göre rastgele engeller yerleştirir.
    /// GameManager'daki PlaceObstacles mantığının chunk-bazlı versiyonu.
    /// </summary>
    private void PlaceObstacles(GameObject Chunk)
    {
        if (CurrentLevelData.SpawnableObjects == null || CurrentLevelData.SpawnableObjects.Count == 0)
            return;

        // Zorluk seviyesine göre chunk başına engel sayısı
        // GameManager'daki formül: 2000 / (100/Difficulty) = Difficulty * 20 toplam engel
        // Chunk başına: (Difficulty * 20) / ChunkMaxCapacity
        int ObstacleCount = Mathf.Max(1, (CurrentLevelData.Difficulty * 20) / ChunkMaxCapacity);

        float ChunkStartZ = Chunk.transform.position.z;
        float ChunkEndZ = ChunkStartZ + ChunkLength;

        // Bu chunk için liste yoksa oluştur
        if (!ChunkObjects.ContainsKey(Chunk))
            ChunkObjects[Chunk] = new List<GameObject>();

        // Chunk başında arabanın hemen altına denk gelen bölgede obje doğurmamak için güvenli mesafe
        float SpawnSafeZoneOffset = 10f;
        float SafeStartZ = ChunkStartZ + SpawnSafeZoneOffset;

        // Chunk sonunda doğan obje yol dışına sarkmasın diye küçük bir güvenli pay.
        float ChunkEndSafeZoneOffset = 5f; 
        float SafeEndZ = ChunkEndZ - ChunkEndSafeZoneOffset;

        // Güvenli alan kalmadıysa çık
        if (SafeStartZ >= SafeEndZ) return;
        // check the SafeStartZ and SafeEndZ values
        Debug.Log($"[PlaceObstacles] Z Spawn Limits: Min={SafeStartZ}, Max={SafeEndZ}");

        for (int i = 0; i < ObstacleCount; i++)
        {
            var SelectedData = GetRandomWeightedPrefab();
            if (SelectedData.Prefab == null) continue;

            float RandomX = Random.Range(-RoadBoundaryX, RoadBoundaryX);
            float RandomZ = Random.Range(SafeStartZ, SafeEndZ);
            float BaseYPosition = SelectedData.YValue;  // LevelData'daki YOffset kullan
            
            // Eğer objede Collider varsa pivot'undan alt tabana olan mesafeyi hesaplayıp Y eksenine ekler, böylece havada asılı doğmayı engeller
            float ColliderBottomOffset = 0f;
            Collider ObjCol = SelectedData.Prefab.GetComponentInChildren<Collider>();
            if (ObjCol != null)
            {
                ColliderBottomOffset = ObjCol.bounds.extents.y;
            }

            // Tam yere oturtmak için
            float YPosition = BaseYPosition + ColliderBottomOffset;

            Vector3 SpawnPosition = new Vector3(RandomX, YPosition, RandomZ);

            // Doğrudan world space'te spawn et — SetParent YOK, scale bozulmuyor
            GameObject SpawnedObstacle = GameObject.Instantiate(SelectedData.Prefab, SpawnPosition, Quaternion.identity);
            SpawnedObstacle.transform.localEulerAngles = new Vector3(0, 90, 0);
            ChunkObjects[Chunk].Add(SpawnedObstacle);
        }
    }

    // ==================== COIN YERLEŞTİRME ====================

    /// <summary>
    /// Chunk üzerine yolun ortasında coin şeridi yerleştirir.
    /// GameManager'daki PlaceCoins mantığının chunk-bazlı versiyonu.
    /// Coinler Z ekseni boyunca düzenli aralıklarla, X'te rastgele dizilir.
    /// </summary>
    private void PlaceCoins(GameObject Chunk)
    {
        if (CurrentLevelData.CoinPrefab == null) return;

        float ChunkStartZ = Chunk.transform.position.z;
        float ChunkEndZ = ChunkStartZ + ChunkLength;

        // Coin aralığı: her 20 birimde bir (GameManager'daki i += 20 mantığı)
        float CoinSpacing = 20f;

        // Bu chunk için liste yoksa oluştur
        if (!ChunkObjects.ContainsKey(Chunk))
            ChunkObjects[Chunk] = new List<GameObject>();

        for (float Z = ChunkStartZ; Z < ChunkEndZ; Z += CoinSpacing)
        {
            float RandomX = Random.Range(-RoadBoundaryX, RoadBoundaryX);
            Vector3 CoinPosition = new Vector3(RandomX, 1f, Z);

            // Doğrudan world space'te spawn et — SetParent YOK
            GameObject SpawnedCoin = GameObject.Instantiate(CurrentLevelData.CoinPrefab, CoinPosition, Quaternion.identity);
            SpawnedCoin.transform.localEulerAngles = new Vector3(0, 90, 90);
            ChunkObjects[Chunk].Add(SpawnedCoin);
        }
    }

    // ==================== BARIYER YERLEŞTİRME ====================

    /// <summary>
    /// FollowTheCar.PlaceBarriers() mantığının chunk-bazlı versiyonu.
    /// Yolun tam sol ve sağ kenarına (RoadBoundaryX), Z boyunca 3 birim aralıklarla,
    /// %50 olasılıkla bariyer spawn eder. Bariyerler yola paralel durur.
    /// </summary>
    private void PlaceBarriers(GameObject Chunk)
    {
        if (CurrentLevelData.BarrierPrefab == null) return;

        // Bu chunk için liste yoksa oluştur
        if (!ChunkObjects.ContainsKey(Chunk))
            ChunkObjects[Chunk] = new List<GameObject>();

        float ChunkStartZ = Chunk.transform.position.z;
        float ChunkEndZ   = ChunkStartZ + ChunkLength;

        float LeftEdgeX  = -RoadBoundaryX;
        float RightEdgeX =  RoadBoundaryX;

        // Bariyer uzunluğunu prefab'ın BoxCollider'ından otomatik hesapla
        float BarrierLength = 1f; // fallback
        BoxCollider BarrierCol = CurrentLevelData.BarrierPrefab.GetComponent<BoxCollider>();
        if (BarrierCol != null)
            BarrierLength = BarrierCol.size.z * CurrentLevelData.BarrierPrefab.transform.localScale.z;

        // Aralık = bariyer boyu → bariyerler birbirine tam bitişik dizilir
        float BarrierSpacing = BarrierLength;

        // LevelData'dan gelen tek spawn ihtimali (0–1), sol ve sağ için ortak
        float SpawnChance = CurrentLevelData.BarrierSpawnChance;

        // Bariyerlerin Z ekseninde (ileri veya geri) taşmaması için son noktası bariyer boyu kadar içeriye çekilir
        float ChunkEndZSafe = ChunkEndZ - BarrierLength;

        // Ayrıca bariyerin başlangıç noktasını da kendi boyu kadar içeri çekerek yoldan taşmasını tamamen engelleyelim
        for (float Z = ChunkStartZ + (BarrierLength / 2f); Z < ChunkEndZSafe; Z += BarrierSpacing)
        {
            // SOL kenar için bağımsız zar at
            if (Random.value < SpawnChance)
            {
                Vector3 LeftPos = new Vector3(LeftEdgeX, 0.5f, Z);
                GameObject BarrierL = GameObject.Instantiate(CurrentLevelData.BarrierPrefab, LeftPos, Quaternion.identity);
                BarrierL.transform.localEulerAngles = new Vector3(0, 0, 0);
                ChunkObjects[Chunk].Add(BarrierL);
            }

            // SAĞ kenar için bağımsız zar at
            if (Random.value < SpawnChance)
            {
                Vector3 RightPos = new Vector3(RightEdgeX, 0.5f, Z);
                GameObject BarrierR = GameObject.Instantiate(CurrentLevelData.BarrierPrefab, RightPos, Quaternion.identity);
                BarrierR.transform.localEulerAngles = new Vector3(0, 0, 0);
                ChunkObjects[Chunk].Add(BarrierR);
            }
        }
    }

    // ==================== WEIGHTED RANDOM ====================

    /// <summary>
    /// LevelData'daki SpawnableObjects'ten ağırlıklı rastgele bir prefab seçer.
    /// GameManager'daki GetRandomWeightedPrefab'ın aynısı.
    /// </summary>
    private (GameObject Prefab, float YValue) GetRandomWeightedPrefab()
    {
        if (TotalWeight <= 0f) return (null, 0f);

        float RandomValue = Random.Range(0f, TotalWeight);
        float WeightSum = 0f;

        foreach (var Spawn in CurrentLevelData.SpawnableObjects)
        {
            WeightSum += Spawn.Weight;
            if (RandomValue <= WeightSum)
            {
                // YOffset: Inspector'dan elle ayarlanan zemin offset’i
                return (Spawn.Prefab, Spawn.YOffset);
            }
        }

        // Yuvarlama hatası fallback
        Debug.LogWarning("RoadProcessing: Weighted random fallback triggered.");
        var LastSpawn = CurrentLevelData.SpawnableObjects[CurrentLevelData.SpawnableObjects.Count - 1];
        return (LastSpawn.Prefab, LastSpawn.YOffset);
    }
}


public class RoadChunkQueue
{
    private Queue<GameObject> RoadQueue;
    private int _maxCapacity;
    public RoadChunkQueue(int MaxCapacity)
    {
        _maxCapacity = MaxCapacity;
        RoadQueue = new Queue<GameObject>();
    }
    public void EnqueueChunk(GameObject newChunk)
    {
        RoadQueue.Enqueue(newChunk);
    }
    public void DequeueAndDestroyOldestChunk()
    {
        if (RoadQueue.Count > 0)
        {
            GameObject OldestChunk = RoadQueue.Dequeue();
            GameObject.Destroy(OldestChunk);
        }
    }
    public bool IsAtCapacity()
    {
        return RoadQueue.Count >= _maxCapacity;
    }
    public void ClearAndDestroyAll() 
    { 
        while (RoadQueue.Count > 0)
        {
            GameObject ChunkToDestroy = RoadQueue.Dequeue();
            GameObject.Destroy(ChunkToDestroy);
        }
    }
}
