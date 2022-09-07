using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] float spawnRate = 2;

    float elapsed = 0;

    Transform[] spawnPoints = null;

    private static SpawnManager _instance;
    public static SpawnManager Instance => _instance;

    List<EnemySO> enemies = new List<EnemySO>();

    void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnNewEnemyWave += NewEnemyWave;
        GameManager.Instance.OnNewStage += NewStage;
    }

    void NewEnemyWave()
    {
        foreach (GameManager.EnemySpawn info in GameManager.Instance.CurrentEnemySpawnInfo)
        {
            for (int i = 0; i < info.amount; i++)
            {
                enemies.Add(info.enemy);
            }
        }
    }

    void NewStage()
    {
        spawnPoints = GameManager.Instance.CurrentEnemySpawnsPoints;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed > spawnRate)
        {
            elapsed = 0;

            if (enemies.Count > 0)
            {
                int index = Random.Range(0, enemies.Count);
                EnemySO enemy = enemies[index];
                Instantiate(enemy.prefab, spawnPoints[Random.Range(0, spawnPoints.Length)]).SetParent(null);
                enemies.RemoveAt(index);
            }
        }
    }
}
