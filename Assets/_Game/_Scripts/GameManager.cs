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
    [SerializeField] RectTransform hpContainer;

    [SerializeField] Transform playerSpawn;
    [SerializeField] EnemySO playerSpawnType;
    
    [SerializeField] float playerAcceleration = 750;
    [SerializeField] float playerSpeed = 6;

    [SerializeField] float heartUIDimension = 50;
    
    [SerializeField] Stage[] stages;

    [SerializeField] LayerMask predictionLayers;

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

    EnemyController currentPlayer;
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
            // LOSE
        }

        currentEnemies.Remove(e);
        
        if (currentEnemies.Count == 1)
        {
            ++currentEnemyWave;
            if (currentEnemyWave >= stages[currentStage].enemyWaves.Length)
            {
                currentEnemyWave = 0;
                ++currentStage;

                if (currentStage >= stages.Length)
                {
                    // WIN
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
    [HideInInspector] public bool isPlayerCorrupted = false;
    
    [SerializeField] private GameObject sword;
    private SwordBehaviour swordBehaviour;
    [SerializeField] private float swordSpeed;
    public Vector3 launchDir;
    private float launchLimit;

    int currentStage = 0;
    int currentEnemyWave = 0;

    private Image[] hps;

    //public LineRenderer aimLine;

    private void Start()
    {
        swordBehaviour = sword.GetComponent<SwordBehaviour>();
        StartGame();
    }
    
    void StartGame()
    {
        currentPlayer = Instantiate(playerSpawnType.prefab, playerSpawn).GetComponent<EnemyController>();
        currentPlayer.transform.SetParent(null);
        currentPlayer.transform.position = playerSpawn.position;
        ChangePlayer(currentPlayer);
        //aimLine = currentPlayer.GetComponent<LineRenderer>();
        
        OnNewStage?.Invoke();
        OnNewEnemyWave?.Invoke();
    }

    public void ChangePlayer(EnemyController newPlayer)
    {
        isPlayerTransition = false;
        
        //currentPlayer.tag = "Enemy";
        currentPlayer.possessed = false;
        currentPlayer.agent.enabled = true;
        currentPlayer.col.isTrigger = true;
        if (currentPlayer.animator)
        {
            currentPlayer.Transition();
        }

        currentPlayer = newPlayer;
        currentPlayer.agent.enabled = false;
        currentPlayer.col.isTrigger = false;
        currentPlayer.possessed = true;
        //currentPlayer.tag = "Player";
        
        //aimLine = currentPlayer.GetComponent<LineRenderer>();

        sword.transform.SetParent(currentPlayer.swordContainer);
        sword.transform.localPosition = new Vector3(0.00015f, -0.00061f, 0.00012f);
        sword.transform.localEulerAngles = new Vector3(7.469854f, 168.0391f, 164.1708f);
        
        if (currentPlayer.animator)
        {
            currentPlayer.Transition();
        }

        SpawnHPs();
    }

    void LaunchLimit()
    {
        isPlayerTransition = false;
        sword.transform.SetParent(currentPlayer.swordContainer);
        sword.transform.localPosition = new Vector3(0.00015f, -0.00061f, 0.00012f);
        sword.transform.localEulerAngles = new Vector3(7.469854f, 168.0391f, 164.1708f);
        //currentPlayer.tag = "Player";
    }

    private void Update()
    {
        if (isPlayerTransition)
        {
            sword.transform.position += launchDir.normalized * swordSpeed * Time.deltaTime;
            sword.transform.rotation *= Quaternion.Euler(-Time.deltaTime * 500, 0, 0);
            launchLimit += Time.deltaTime;
            
            if (launchLimit >= 2)
            {
                LaunchLimit();
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentPlayer && !isPlayerTransition && !isPlayerCorrupted)
        {
            Vector3 dir = (Vector3.forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal")).normalized;
            currentPlayer.transform.rotation = Quaternion.LookRotation(dir.sqrMagnitude == 0 ? currentPlayer.transform.forward : dir);
            currentPlayer.rb.AddForce(dir * playerAcceleration, ForceMode.Force);
            if (currentPlayer.rb.velocity.magnitude > playerSpeed)
            {
                currentPlayer.rb.velocity = currentPlayer.rb.velocity.normalized * playerSpeed;
            }

            if(Input.GetKey(KeyCode.JoystickButton3) || Input.GetKey(KeyCode.Mouse0))
            {
                RaycastHit hit;
                Physics.Raycast(new Ray(currentPlayer.transform.position, currentPlayer.transform.forward), out hit, predictionLayers);
                currentPlayer.prediction.gameObject.SetActive(true);
                currentPlayer.prediction.localScale = new Vector3(2, 2, (hit.transform ? hit.distance : 1000) * 2);

                //aimLine.positionCount = 2;
                //aimLine.SetPosition(0, currentPlayer.transform.position);
                //aimLine.SetPosition(1, ray.GetPoint(50));
                //aimLine.enabled = true;
                //LayerMask obstacle = LayerMask.GetMask("Obstacle");
                //if (Physics.Raycast(ray, out hit, obstacle))
                //{
                //    aimLine.positionCount = 3;
                //    aimLine.SetPosition(1, hit.point);
                //    Vector3 reflectVec = Vector3.Reflect(ray.direction, hit.normal);
                //    ray = new Ray(hit.point, reflectVec);
                //    //Physics.Raycast(ray, out hit);
                //    aimLine.SetPosition(2, ray.GetPoint(50));
                //}
            }

            if (Input.GetKeyUp(KeyCode.JoystickButton3) || Input.GetKeyUp(KeyCode.Mouse0))
            {
                //aimLine.enabled = false;
                isPlayerTransition = true;
                launchDir = currentPlayer.transform.forward;
                sword.transform.SetParent(null);
                launchLimit = 0;

                currentPlayer.prediction.gameObject.SetActive(false);
            }

            if (!swordBehaviour.swinging && (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.JoystickButton2)))
            {
                currentPlayer.Attack();
                swordBehaviour.swinging = true;
            }
        }
    }

    public void StopSwing()
    {
        swordBehaviour.swinging = false;
    }

    private void SpawnHPs()
    {
        hpContainer.sizeDelta = new Vector2(heartUIDimension * currentPlayer.EnemySO.hp, heartUIDimension);

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
            NewImage.rectTransform.SetParent(hpContainer);
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
