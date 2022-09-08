using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(CapsuleCollider))]
public class EnemyController : MonoBehaviour
{
    public Animator animator;

    public EnemySO enemySO;
    public EnemySO EnemySO => enemySO;

    [HideInInspector] public bool possessed = false;

    public NavMeshAgent agent;
    public Transform swordContainer;

    public Rigidbody rb;

    public CapsuleCollider col;

    public Transform prediction;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<CapsuleCollider>();
        col.isTrigger = true;

        prediction.gameObject.SetActive(false);

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
    }

    private void Die()
    {
        animator.SetTrigger("Death");

        GameManager.Instance.RemoveEnemy(this);
        agent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }


    public void Attack()
    {
        animator.SetTrigger("Attack");
    }


    public void StopAttack()
    {
        GameManager.Instance.StopSwing();
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
        animator.SetFloat("Speed", rb.velocity.magnitude);

        if (this == GameManager.Instance.Player) return;

        if (true)//GameManager.Instance.moving)
        {
            elapsed += Time.deltaTime;
            if (elapsed > rate)
            {
                if (agent.enabled)
                {
                    agent.SetDestination(GameManager.Instance.Player.transform.position);
                }
                elapsed = 0;
            }
            
            if (Vector3.Distance(GameManager.Instance.Player.transform.position, transform.position) <= 2)
            {
                Attack();
            }
        }
    }

    public void CorruptionStart()
    {
        GameManager.Instance.isPlayerCorrupted = true;
    }

    public void CorruptionEnd()
    {
        GameManager.Instance.isPlayerCorrupted = false;
    }
}
