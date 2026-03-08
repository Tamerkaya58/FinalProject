using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs; // Yol parçasý prefablarýn listesi
    public Transform playerTransform; // Oyuncunun yerini takip etmek için

    private float spawnZ = 0.0f; // Yeni parçanýn ekleneceði Z koordinatý
    private float tileLength = 30.0f; // Her bir yol parçasýnýn uzunluðu (Metre cinsinden)
    private int amnTilesOnScreen = 5; // Ekranda ayný anda kaç parça görüneceði
    private List<GameObject> activeTiles = new List<GameObject>(); // Silmek için takip ettiðimiz liste

    void Start()
    {
        // Oyun baþýnda baþlangýç parçalarýný oluþtur
        for (int i = 0; i < amnTilesOnScreen; i++)
        {
            SpawnTile(0); // Ýlk birkaçý düz veya belirli bir tip olabilir
        }
    }

    void Update()
    {
        // Oyuncu ilerledikçe yeni parça ekle ve eskiyi sil
        // Mesafe kontrolü: Oyuncu son parçaya yaklaþtýysa
        if (playerTransform.position.z - 35 > (spawnZ - amnTilesOnScreen * tileLength))
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));
            DeleteTile();
        }
    }

    private void SpawnTile(int prefabIndex)
    {
        GameObject go = Instantiate(tilePrefabs[prefabIndex], transform.forward * spawnZ, transform.rotation);
        activeTiles.Add(go);
        spawnZ += tileLength;
    }

    private void DeleteTile()
    {
        Destroy(activeTiles[0]); // Listenin en baþýndaki (en eski) objeyi yok et
        activeTiles.RemoveAt(0); // Listeden de çýkar
    }
}
