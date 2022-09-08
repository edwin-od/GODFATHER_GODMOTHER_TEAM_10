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

    [SerializeField] private Material possesedTexture;
    [SerializeField] private Material enemyTexture;

    [SerializeField] private SkinnedMeshRenderer mesh;

    public Rigidbody rb;

    public CapsuleCollider col;

    public Transform prediction;

    bool attacking = false;
    bool hit = false;
    
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

    public void SwitchMaterial(bool possessed)
    {
        if (possessed)
        {
            mesh.material = possesedTexture;
        }
        else
        {
            mesh.material = enemyTexture;
        }
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

        if (this != GameManager.Instance.Player) 
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
    

    private float elapsedAgent = 0;
    private float rateAgent = 0.25f;
    public float cooldownAttack = 1f;
    private float elapsedAttack = 0;
    private void Update()
    {
        animator.SetFloat("Speed", !agent.enabled && GameManager.Instance.Player != this ? 0 : rb.velocity.magnitude);

        if (this == GameManager.Instance.Player) return;

        elapsedAgent += Time.deltaTime;
        if (elapsedAgent > rateAgent)
        {
            if (agent.enabled)
            {
                agent.SetDestination(GameManager.Instance.Player.transform.position);
            }
            elapsedAgent = 0;
        }

        if (elapsedAttack > 0)
            elapsedAttack -= Time.deltaTime;

        if (elapsedAttack <= 0)
        {
            if (Vector3.Distance(GameManager.Instance.Player.transform.position, transform.position) <= 2)
            {
                if (!attacking)
                {
                    Attack();
                }
                else if (!hit)
                {
                    GameManager.Instance.Player.ApplyDamage(enemySO.dmg);
                    hit = true;
                }
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

    public void AttackStart()
    {
        attacking = true;
        hit = false;
    }

    public void AttackEnd()
    {
        attacking = false;
        hit = false;
        elapsedAttack = cooldownAttack;
    }
}
