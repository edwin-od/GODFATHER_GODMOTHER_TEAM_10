using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public EnemySO enemySO;
    public EnemySO EnemySO => enemySO;

    [HideInInspector] public bool possessed = false;

    public NavMeshAgent agent;
    public Transform swordContainer;

    public Rigidbody rb;

    public CharacterController cont;
    
    private void Start()
    {
        cont = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        
        Init(enemySO);
    }

    public void Init(EnemySO enemySO)
    {
        this.enemySO = enemySO;
        
        currentHealth = enemySO.hp;

        GameManager.Instance.AddEnemy(this);
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
        
        Destroy(gameObject);
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void Transition()
    {
        animator.SetBool("Corrupted", possessed);
        animator.SetTrigger("Transition");
    }
    

    private float elapsed = 0;
    private float rate = 0.25f;
    private void Update()
    {
        if (this == GameManager.Instance.Player) return;
        animator.SetFloat("Speed", agent.velocity.magnitude);
        if (true)//GameManager.Instance.moving)
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
