using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GameManager : MonoBehaviour
{
    [SerializeField] float playerSpeed = 6;
    
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
        
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    [SerializeField] EnemyController currentPlayer;
    public EnemyController Player => currentPlayer;
    EnemyController[] currentEnemies;
    bool isPlayerTransition = false;

    private void Update()
    {
        if (currentPlayer && !isPlayerTransition)
        {
            Vector3 dir = (Vector3.forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal")).normalized * Time.deltaTime * playerSpeed;
            currentPlayer.transform.position += dir;
            currentPlayer.transform.rotation = Quaternion.LookRotation(dir.magnitude == 0 ? currentPlayer.transform.forward : dir.normalized);
        }
        
    }
}
