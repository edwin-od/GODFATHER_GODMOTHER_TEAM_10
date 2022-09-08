using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public EnemySO enemySO;
    public EnemySO EnemySO => enemySO;

    [HideInInspector] public bool possessed = false;

    public NavMeshAgent agent;

    private bool playerMoving = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        Init(enemySO);
    }

    public void Init(EnemySO enemySO)
    {
        this.enemySO = enemySO;
        
        currentHealth = enemySO.hp;

        GameManager.Instance.AddEnemy(this);
        
        GameManager.Instance.OnPlayerStartMoving += PlayerStartedMoving;
        GameManager.Instance.OnPlayerStopMoving += PlayerStoppedMoving;
    }

    public float currentHealth
    {
        get;

        private set;
    }

    public void ApplyDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, enemySO.hp);
        
        if (possessed)
            GameManager.Instance.UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            //animator.SetTrigger("Damage");
        }

    }

    private void Die()
    {
        animator.SetTrigger("Death");
        //TODO implements Die method
        //...

        GameManager.Instance.RemoveEnemy(this);
        
        GameManager.Instance.OnPlayerStartMoving -= PlayerStartedMoving;
        GameManager.Instance.OnPlayerStopMoving -= PlayerStoppedMoving;
        
        Destroy(gameObject);
    }

    //ATTACK TO DO
    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void Corruption()
    {
        animator.SetBool("Corrupted", true);
    }

    void PlayerStartedMoving()
    {
        if (GameManager.Instance.isPlayerTransition) return;

        playerMoving = true;
    }
    
    void PlayerStoppedMoving()
    {
        playerMoving = false;
    }

    private float elapsed = 0;
    private float rate = 0.25f;
    private void Update()
    {
        if (this == GameManager.Instance.Player) return;
        
        if (playerMoving)
        {
            elapsed += Time.deltaTime;
            if (elapsed > rate)
            {
                agent?.SetDestination(GameManager.Instance.Player.transform.position);
                elapsed = 0;
            }
        }
    }
}
