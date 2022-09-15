using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	[SerializeField] float waveStartDelay = 5;
	[SerializeField] float stageStartDelay = 5;
	[SerializeField] float spawnRate = 2;

	float elapsed = 0;

	Transform[] spawnPoints = null;

	private static SpawnManager _instance;
	public static SpawnManager Instance => _instance;

	List<EnemySO> enemies = new List<EnemySO>();
	public List<EnemySO> specialEnemies = new List<EnemySO>();

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
		elapsed = -waveStartDelay;
		foreach (GameManager.EnemySpawn info in GameManager.Instance.CurrentEnemySpawnInfo)
		{
			for (int i = 0; i < info.amount; i++)
			{
				if (Random.value > 0.35f)
				{
					enemies.Add(info.enemy);

				}
			}


		}
	}

	void NewStage()
	{
		spawnPoints = GameManager.Instance.CurrentEnemySpawnsPoints;
		elapsed = -stageStartDelay;
	}

	[HideInInspector] public bool pause = false;

	private void Update()
	{
		if (!pause)
		{
			if (enemies.Count > 0)
			{
				elapsed += Time.deltaTime;
				if (elapsed > spawnRate)
				{
					elapsed = 0;

					int index = Random.Range(0, enemies.Count);
					EnemySO enemy = enemies[index];
					int spInd = Random.Range(0, spawnPoints.Length);
					Transform enemyInstance;

					if (Random.value > 0.85f)
					{
						if (Random.value > 0.5f)
							enemyInstance = Instantiate(specialEnemies[1].prefab, spawnPoints[spInd]);
						else
							enemyInstance = Instantiate(specialEnemies[0].prefab, spawnPoints[spInd]);
					}
					else
					{
						enemyInstance = Instantiate(enemy.prefab, spawnPoints[spInd]);
					}

					enemyInstance.SetParent(null);
					enemyInstance.position = spawnPoints[spInd].position;
					enemies.RemoveAt(index);
				}
			}
			else if (elapsed != 0)
			{
				elapsed = 0;
			}
		}
	}
}
