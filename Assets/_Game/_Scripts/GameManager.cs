using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private KeyCode attackJoystick = KeyCode.JoystickButton2;
    [SerializeField] private KeyCode throwJoystick = KeyCode.JoystickButton3;
    
    [SerializeField] public Slider healthBar;

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


    public CanvasAnim _AnimInstance;
    private bool endGame = false;
    public Image gameOver;
    
    
    public void RemoveEnemy(EnemyController e)
    {
        if (e == currentPlayer)
        {
           _AnimInstance.End();
            StopEverything();
            endGame = true;
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
                    StopEverything();

                    return;
                }
                OnNewStage?.Invoke();
            }
            OnNewEnemyWave?.Invoke();
        }
    }

    void StopEverything()
    {
        foreach (var e in currentEnemies)
        {
            e.gameObject.SetActive(false);
        }
        currentPlayer.gameObject.SetActive(false);
        PauseGame();
    }

    public void PauseChase()
    {
        foreach (var e in currentEnemies)
        {
            if (e == currentPlayer) continue;
            e.agent.enabled = false;
            e.rb.useGravity = false;
        }
    }

    public void PauseChase(EnemyController enemy)
    {
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
            if (e == currentPlayer) continue;
            e.agent.enabled = true;
            e.rb.useGravity = true;
        }
    }
    
    public void ResumeChase(EnemyController enemy)
    {
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
    public Vector3 launchDir;
    private float launchLimit;

    int currentStage = 0;
    int currentEnemyWave = 0;

    private Image[] hps;
    
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
        score = 0;
        
        OnNewStage?.Invoke();
        OnNewEnemyWave?.Invoke();
    }

    public int score
    {
        get;

        set;
    }

    public void ChangePlayer(EnemyController newPlayer)
    {
        swordBehaviour.SetColliderRadius(true);
        
        isPlayerTransition = false;
        
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
        if (pause) ResumeGame();
        else PauseGame();
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
            if (Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                _AnimInstance.Restart();
                endGame = false;
                Retry();
            }
            else if (Input.GetKeyDown(KeyCode.Joystick1Button0))
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

    public TMP_Text scoreText;
    public void updateScore()
    {
        scoreText.text = score.ToString();
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
