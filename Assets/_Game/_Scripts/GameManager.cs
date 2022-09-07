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
    public bool isPlayerTransition = false;

    [SerializeField] private GameObject sword;
    [SerializeField] private float swordSpeed;
    private Vector3 launchDir;
    private float launchLimit;

    public void ChangePlayer(EnemyController newPlayer)
    {
        currentPlayer.tag = "Enemy";
        currentPlayer = newPlayer;
        currentPlayer.tag = "Player";
        isPlayerTransition = false;
        sword.transform.SetParent(currentPlayer.transform);
        sword.transform.localPosition = new Vector3(0, 0.4f, 0.6f);
        sword.transform.localEulerAngles = new Vector3(90, 0, 0);
    }

    void LaunchLimit()
    {
        isPlayerTransition = false;
        sword.transform.SetParent(currentPlayer.transform);
        sword.transform.localPosition = new Vector3(0, 0.4f, 0.6f);
        sword.transform.localEulerAngles = new Vector3(90, 0, 0);
        currentPlayer.tag = "Player";
    }

    private void Update()
    {
        if (currentPlayer && !isPlayerTransition)
        {
            Vector3 dir = (Vector3.forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal")).normalized * Time.deltaTime * playerSpeed;
            if (Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                isPlayerTransition = true;
                launchDir = currentPlayer.transform.forward;
                sword.transform.SetParent(null);
                launchLimit = 0;
            }
            currentPlayer.transform.position += dir;
            currentPlayer.transform.rotation = Quaternion.LookRotation(dir.magnitude == 0 ? currentPlayer.transform.forward : dir.normalized);
        }
        if (isPlayerTransition)
        {
            sword.transform.position += launchDir.normalized * swordSpeed * Time.deltaTime;
            launchLimit += Time.deltaTime;
            if (launchLimit >= 2)
            {
                LaunchLimit();
            }
        }
    }
}
