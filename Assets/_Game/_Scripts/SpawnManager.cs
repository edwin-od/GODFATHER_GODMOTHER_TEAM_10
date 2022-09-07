using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    Transform[] spawnPoints = null;
    GameManager.EnemySpawn[] enemySpawnsInfo = null;

    private static SpawnManager _instance;
    public static SpawnManager Instance => _instance;

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
        enemySpawnsInfo = GameManager.Instance.CurrentEnemySpawnInfo;
    }

    void NewStage()
    {
        spawnPoints = GameManager.Instance.CurrentEnemySpawnsPoints;
    }
}
