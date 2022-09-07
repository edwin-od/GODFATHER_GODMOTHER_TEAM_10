using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] KeyCode up = KeyCode.UpArrow;
    [SerializeField] KeyCode down = KeyCode.DownArrow;
    [SerializeField] KeyCode left = KeyCode.LeftArrow;
    [SerializeField] KeyCode right = KeyCode.RightArrow;
    
    [SerializeField] float playerSpeed = 5;
    
    [SerializeField] Stage[] stages;

    [System.Serializable]
    class EnemySpawn
    {
        public EnemySO enemy;
        public int amount = 1;
    }

    public enum Dynamic
    {
        Type1,
        Type2,
        Type3
    }
    
    [System.Serializable]
    class Stage
    {
        public EnemyStage[] enemyStages;
    }

    [System.Serializable]
    class EnemyStage
    {
        public EnemySpawn[] enemies;
    }
   
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    void Awake()
    {
        _instance = this;
    }

    Transform[] currentEnemies;
    
    
}
