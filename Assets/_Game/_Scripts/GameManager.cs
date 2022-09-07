using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] Sprite fullHeart;
    [SerializeField] Sprite halfHeart;
    [SerializeField] Sprite emptyHeart;
    [SerializeField] Transform hpContainer;

    [SerializeField] Transform playerSpawn;
    [SerializeField] EnemySO playerSpawnType;
    
    [SerializeField] float playerSpeed = 6;
    
    [SerializeField] Stage[] stages;

    [System.Serializable]
    public class EnemySpawn
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
        public EnemyWave[] enemyWaves;
        public Transform[] spawnPoints;
    }

    [System.Serializable]
    class EnemyWave
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
    HashSet<EnemyController> currentEnemies = new HashSet<EnemyController>();
    public void AddEnemy(EnemyController e)
    {
        if (e != currentPlayer)
            currentEnemies.Add(e);
    }

    public void RemoveEnemy(EnemyController e)
    {
        if (e == currentPlayer)
        {
            // Game Over
        }

        currentEnemies.Remove(e);
        
        if (currentEnemies.Count == 0)
        {
            ++currentEnemyWave;
            if (currentEnemyWave >= stages[currentStage].enemyWaves.Length)
            {
                currentEnemyWave = 0;
                ++currentStage;

                if (currentStage >= stages.Length)
                {
                    // Game End
                    return;
                }
                OnNewStage?.Invoke();
            }
            OnNewEnemyWave?.Invoke();
        }
    }

    public EnemySpawn[] CurrentEnemySpawnInfo => stages[currentStage].enemyWaves[currentEnemyWave].enemies;
    public Transform[] CurrentEnemySpawnsPoints => stages[currentStage].spawnPoints;

    public Action OnNewEnemyWave;
    public Action OnNewStage;

    [HideInInspector] public bool isPlayerTransition = false;

    [SerializeField] private GameObject sword;
    [SerializeField] private float swordSpeed;
    private Vector3 launchDir;
    private float launchLimit;

    int currentStage = 0;
    int currentEnemyWave = 0;

    private Image[] hps;

    private void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        currentPlayer = Instantiate(playerSpawnType.prefab, playerSpawn).GetComponent<EnemyController>();
        currentPlayer.transform.SetParent(null);
        ChangePlayer(currentPlayer);

        OnNewStage?.Invoke();
        OnNewEnemyWave?.Invoke();
    }

    public void ChangePlayer(EnemyController newPlayer)
    {
        currentPlayer.tag = "Enemy";
        currentPlayer.possessed = false;
        currentPlayer = newPlayer;
        currentPlayer.possessed = true;
        currentPlayer.tag = "Player";
        isPlayerTransition = false;
        sword.transform.SetParent(currentPlayer.transform);
        sword.transform.localPosition = new Vector3(0, 0.4f, 0.6f);
        sword.transform.localEulerAngles = new Vector3(90, 0, 0);

        SpawnHPs();
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
    
    private void SpawnHPs()
    {
        for (int i = 0; i < hpContainer.childCount; ++i)
        {
            Destroy(hpContainer.GetChild(i).gameObject);
        }

        hps = new Image[currentPlayer.EnemySO.hp];
		
        for (int i = 0; i < currentPlayer.EnemySO.hp; ++i)
        {
            GameObject NewObj = new GameObject();
            Image NewImage = NewObj.AddComponent<Image>();
            NewImage.sprite = fullHeart;
            NewObj.GetComponent<RectTransform>().SetParent(hpContainer);
            NewObj.SetActive(true);
            hps[i] = NewImage;
        }

        UpdateHealthUI();
    }
    
    public void UpdateHealthUI()
    {
        if (hpContainer.childCount < currentPlayer.EnemySO.hp)
            SpawnHPs();
        
        bool isHalf = currentPlayer.currentHealth - Mathf.Floor(currentPlayer.currentHealth) > 0;
		
        int i = 0;
		
        for(; i < currentPlayer.currentHealth - (isHalf ? 1 : 0); ++i)
        {
            hps[i].sprite = fullHeart;
        }

        if (isHalf)
        {
            hps[i].sprite = halfHeart;
            ++i;
        }
        
        for(; i - currentPlayer.currentHealth - (isHalf ? 1 : 0) < currentPlayer.EnemySO.hp - currentPlayer.currentHealth - (isHalf ? 1 : 0); ++i)
        {
            hps[i].sprite = emptyHeart;
        }
    }
}
