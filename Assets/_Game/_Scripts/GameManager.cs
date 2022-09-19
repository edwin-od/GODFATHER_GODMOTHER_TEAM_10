using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private KeyCode attackJoystick = KeyCode.JoystickButton2;
    [SerializeField] private KeyCode throwJoystick = KeyCode.JoystickButton3;

    public TMP_Text enemyCountText;
    public TMP_Text waveCountText;
    public TMP_Text scoreCountText;
    public TMP_Text multiplierCountText;

    int multiplier = 1;
    int score = 0;

    [SerializeField] Color uiCol1 = Color.white;
    [SerializeField] Color uiCol2 = Color.white;
    
    [SerializeField] public Slider healthBar;

    [SerializeField] Transform playerSpawn;
    [SerializeField] EnemySO playerSpawnType;
    
    [SerializeField] float playerAcceleration = 750;
    [SerializeField] float playerSpeed = 6;

    [SerializeField] float heartUIDimension = 50;
    
    [SerializeField] Stage[] stages;

    [SerializeField] LayerMask predictionLayers;

    [SerializeField] private GameObject deathSword;

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


    public CanvasAnim _AnimInstance;
    private bool endGame = false;    
    
    public void RemoveEnemy(EnemyController e)
    {
        if (e == currentPlayer)
        {
           _AnimInstance.End();
            StopEverything(true);
            endGame = true;
        }

        currentEnemies.Remove(e);
        UpdateEnemyCount();

        if (currentEnemies.Count == 1)
        {
            ++currentEnemyWave;
            SetNewWaveUI();
            if (currentEnemyWave >= stages[currentStage].enemyWaves.Length)
            {
                currentEnemyWave = 0;
                ++currentStage;
                SetNewWaveUI();

                if (currentStage >= stages.Length)
                {
                    // WIN
                    StopEverything(false);

                    return;
                }
                OnNewStage?.Invoke();
            }
            OnNewEnemyWave?.Invoke();
        }
    }

    void StopEverything(bool dead)
    {
        foreach (var e in currentEnemies)
        {
            if (e != currentPlayer) e.gameObject.SetActive(false);
        }
        if (dead)
        {
            Instantiate(deathSword, new Vector3(currentPlayer.transform.position.x, 2.59f, currentPlayer.transform.position.z), Quaternion.identity);
        }
        currentPlayer.gameObject.SetActive(false);
        //PauseGame();
    }

    public void PauseChase()
    {
        foreach (var e in currentEnemies)
        {
            PauseChase(e);
        }
    }

    public void PauseChase(EnemyController enemy)
    {
        if (enemy == currentPlayer) return;
        enemy.agent.enabled = false;
        enemy.rb.useGravity = false;
        enemy.pause = true;
    }

    private bool pause = false;

    public void PauseGame()
    {
        pause = true;
        SpawnManager.Instance.pause = true;
        PauseChase();
        foreach (var e in currentEnemies)
        {
            e.animator.speed = 0;
            e.pause = true;
        }
    }
    
    public void ResumeGame()
    {
        foreach (var e in currentEnemies)
        {
            e.animator.speed = 1;
            e.pause = false;
        }
        pause = false;
        SpawnManager.Instance.pause = false;
        ResumeChase();
    }

    public void ResumeChase()
    {
        foreach (var e in currentEnemies)
        {
            ResumeChase(e);
        }
    }
    
    public void ResumeChase(EnemyController enemy)
    {
        if (enemy == currentPlayer) return;
        enemy.agent.enabled = true;
        enemy.rb.useGravity = true;
        enemy.pause = false;
    }

    public EnemySpawn[] CurrentEnemySpawnInfo => stages[currentStage].enemyWaves[currentEnemyWave].enemies;
    public Transform[] CurrentEnemySpawnsPoints => stages[currentStage].spawnPoints;

    public AudioManager audioManager;

    public Action OnNewEnemyWave;
    public Action OnNewStage;

    [HideInInspector] public bool isPlayerTransition = false;
    [HideInInspector] public bool isPlayerCorrupted = false;
    
    public GameObject sword;
    private SwordBehaviour swordBehaviour;
    [SerializeField] private float swordSpeed;
    [HideInInspector] public Vector3 launchDir;
    private float launchLimit;

    int currentStage = 0;
    int currentEnemyWave = 0;
    
    private void Start()
    {
        swordBehaviour = sword.GetComponent<SwordBehaviour>();
        StartGame();
    }
    
    void StartGame()
    {
        score = 0;
        multiplier = 1;
        currentStage = 0;
        currentEnemyWave = 0;


        currentPlayer = Instantiate(playerSpawnType.prefab, playerSpawn).GetComponent<EnemyController>();
        currentPlayer.transform.SetParent(null);
        currentPlayer.transform.position = playerSpawn.position;
        ChangePlayer(currentPlayer);
        
        OnNewStage?.Invoke();
        OnNewEnemyWave?.Invoke();

        SetNewWaveUI();
    }

    int enemyWaveCounter = 0;
    void SetNewWaveUI()
    {
        enemyWaveCounter = 0;
        waveCountText.text = "wave " + (currentEnemyWave + 1) + ":";
        enemyCountText.text = " " + 0 + " / " + stages[currentStage].enemyWaves[currentEnemyWave].enemies.Sum((x) => x.amount);
    }

    void UpdateEnemyCount()
    {
        ++enemyWaveCounter;
        int tot = stages[currentStage].enemyWaves[currentEnemyWave].enemies.Sum((x) => x.amount);
        enemyCountText.text = " " + enemyWaveCounter + " / " + tot;
    }

    public void ChangePlayer(EnemyController newPlayer)
    {
        Debug.Log("Sword Arrived");

        swordBehaviour.SetColliderRadius(true);
        
        isPlayerTransition = false;
        
        currentPlayer.possessed = false;
        currentPlayer.agent.enabled = true;
        currentPlayer.col.isTrigger = true;
        if (currentPlayer.animator)
        {
            currentPlayer.Transition();
        }


        playerSpeed = newPlayer.enemySO.speed;

        currentPlayer = newPlayer;
        currentPlayer.agent.enabled = false;
        currentPlayer.col.isTrigger = false;
        currentPlayer.possessed = true;
        currentPlayer.SwitchMaterial(true);

        sword.transform.SetParent(currentPlayer.swordContainer);
        sword.transform.localPosition = new Vector3(0.00015f, -0.00061f, 0.00012f);
        sword.transform.localEulerAngles = new Vector3(7.469854f, 168.0391f, 164.1708f);
        
        if (currentPlayer.animator)
        {
            currentPlayer.Transition();
        }
        SpawnHPs();
    }

    public void LaunchLimit()
    {
        currentPlayer.SwitchMaterial(true);
        
        swordBehaviour.SetColliderRadius(true);
        isPlayerTransition = false;
        sword.transform.SetParent(currentPlayer.swordContainer);
        sword.transform.localPosition = new Vector3(0.00015f, -0.00061f, 0.00012f);
        sword.transform.localEulerAngles = new Vector3(7.469854f, 168.0391f, 164.1708f);

        currentPlayer.ApplyDamage(0.5f, true);
    }

    void TogglePause()
    {
        //if (pause) ResumeGame();
        //else PauseGame();
    }

    private bool cancelThrow = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKey(KeyCode.JoystickButton7))
        {
            TogglePause();
        }

        if (endGame)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                _AnimInstance.Restart();
                endGame = false;
                Retry();
            }
            else if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Mouse2))
            {
                Exit();
            }
        }

        if (!pause)
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

            if (currentPlayer && !isPlayerTransition && !isPlayerCorrupted)
            {
                if (Input.GetKeyDown(throwJoystick) || Input.GetKeyDown(KeyCode.Mouse1))
                {
                    cancelThrow = false;
                    currentPlayer.prediction.gameObject.SetActive(true);
                }
                if (!cancelThrow && Input.GetKey(throwJoystick) || Input.GetKey(KeyCode.Mouse1))
                {
                    RaycastHit hit;
                    Physics.Raycast(new Ray(currentPlayer.transform.position, currentPlayer.transform.forward), out hit, predictionLayers);
                    currentPlayer.prediction.localScale = new Vector3(2, 2, (hit.transform ? hit.distance : 1000) * 2);
                }

                if (Input.GetKeyUp(throwJoystick) || Input.GetKeyUp(KeyCode.Mouse1))
                {
                    if (!cancelThrow)
                    {                        
                        currentPlayer.SwitchMaterial(false);
                        audioManager.PlayClip("Sword" + UnityEngine.Random.Range(1, 4).ToString());
                        swordBehaviour.SetColliderRadius(false);
                        currentPlayer.animator.SetTrigger("Attack");
                        isPlayerTransition = true;
                        launchDir = currentPlayer.transform.forward;
                        sword.transform.SetParent(null);
                        launchLimit = 0;

                        currentPlayer.prediction.gameObject.SetActive(false);
                    }

                    cancelThrow = false;
                }

                if (!swordBehaviour.swinging && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(attackJoystick)))
                {
                    cancelThrow = true;
                    currentPlayer.prediction.gameObject.SetActive(false);

                    currentPlayer.Attack();
                    swordBehaviour.swinging = true;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!pause)
        {
            if (currentPlayer && !isPlayerTransition && !isPlayerCorrupted)
            {
                bool forward = Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W);
                bool backward = Input.GetKey(KeyCode.S);
                bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q);
                bool right = Input.GetKey(KeyCode.D);
                Vector3 dir = (Vector3.forward * (Input.GetAxis("Vertical") + (forward ? 1 : backward ? -1 : 0))  + Vector3.right * (Input.GetAxis("Horizontal") +  + (right ? 1 : left ? -1 : 0))).normalized;
                currentPlayer.transform.rotation = Quaternion.LookRotation(dir.sqrMagnitude == 0 ? currentPlayer.transform.forward : dir);
                currentPlayer.rb.AddForce(dir * playerAcceleration, ForceMode.Force);
                if (currentPlayer.rb.velocity.magnitude > playerSpeed)
                {
                    currentPlayer.rb.velocity = currentPlayer.rb.velocity.normalized * playerSpeed;
                }
            }
        }
    }

    public void StopSwing()
    {

        swordBehaviour.swinging = false;
    }

    private void SpawnHPs()
    {
        UpdateHealthUI();
    }
    
    public void UpdateHealthUI()
    {
        healthBar.value = currentPlayer.currentHealth / currentPlayer.enemySO.hp;
    }

    private void Retry()
    {
        SceneManager.LoadScene("Menu");
    }

    private void Exit()
    {
        Application.Quit();
    }
}
