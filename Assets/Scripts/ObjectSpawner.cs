using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private Transform minPos;
    [SerializeField] private Transform maxPos;

    [SerializeField] private int waveNumber;
    [SerializeField] private List<Wave> waves;

    [System.Serializable]
    public class Wave {
        public GameObject prefab;
        public float spawnTimer;
        public float spawnInterval;
        public int objectsPerWave;
        public int spawnedObjectCount;
    }

    void Update()
    {
        float difficulty = GameManager.Instance != null ? GameManager.Instance.GetDifficultyMultiplier() : 1f;
        waves[waveNumber].spawnTimer -= Time.deltaTime * GameManager.Instance.worldSpeed * difficulty;
        if (waves[waveNumber].spawnTimer <= 0){
            waves[waveNumber].spawnTimer += waves[waveNumber].spawnInterval;
            SpawnObject();
        }
        if (waves[waveNumber].spawnedObjectCount >= waves[waveNumber].objectsPerWave){
            waves[waveNumber].spawnedObjectCount = 0;
            waveNumber++;
            if (waveNumber >= waves.Count){
                waveNumber = 0;
            }
        }
    }

    private void SpawnObject(){
        Instantiate(waves[waveNumber].prefab, RandomSpawnPoint(), transform.rotation, transform);
        waves[waveNumber].spawnedObjectCount++;

        if (GameManager.Instance != null && GameManager.Instance.survivalTime > 30f && Random.value < 0.35f){
            Instantiate(waves[waveNumber].prefab, RandomSpawnPoint(), transform.rotation, transform);
        }
    }

    private Vector2 RandomSpawnPoint(){
        Vector2 spawnPoint;

        float screenRight = 9.17f;
        if (Camera.main != null){
            screenRight = Camera.main.transform.position.x + (Camera.main.orthographicSize * Camera.main.aspect);
        }

        float baseSpawnX = minPos != null ? minPos.position.x : 11f;
        // Spawn smoothly at least 3.5 units past right screen edge so enemies travel visibly into view
        spawnPoint.x = Mathf.Max(baseSpawnX, screenRight + 3.5f, 13.0f);

        float minY = minPos != null ? minPos.position.y : 4f;
        float maxY = maxPos != null ? maxPos.position.y : -5f;
        spawnPoint.y = Random.Range(Mathf.Min(minY, maxY), Mathf.Max(minY, maxY));

        return spawnPoint;
    }
}
