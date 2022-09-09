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

    [SerializeField] private ParticleSystem corruptionParticle;
    [SerializeField] private ParticleSystem bloodParticle;

    private AudioManager audioManager;

    public Rigidbody rb;

    public CapsuleCollider col;

    public Transform prediction;

    public float hitInvincibilityCountdown = 0.2f;

    bool attacking = false;
    bool hit = false;
    bool dead = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<CapsuleCollider>();
        col.isTrigger = true;

        prediction.gameObject.SetActive(false);

        Init(enemySO);
        audioManager = GameManager.Instance.audioManager;
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

    public void FirstStep()
    {
        if (!possessed)
        {
            audioManager.PlayClip("FirstStep");
        }
        else
        {
            audioManager.PlayClip("FirstStepHero");
        }
    }
    
    public void SecondStep()
    {
        if (!possessed)
        {
            audioManager.PlayClip("SecondStep");
        }
        else
        {
            audioManager.PlayClip("SecondStephero");
        }
    }

    public void ApplyDamage(float damage)
    {
        if (this == GameManager.Instance.Player)
        {
            if (isInv) return;
            isInv = true;
        }
        
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, enemySO.hp);
        bloodParticle.Play();
        if (possessed)
            GameManager.Instance.UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        dead = true;

        animator.SetTrigger("Death");

        GameManager.Instance.RemoveEnemy(this);
        agent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;

        if (this != GameManager.Instance.Player)
        {
            rb.useGravity = false;
            StartCoroutine(Death());
        }
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
    
    public void ParticleStart()
    {
        corruptionParticle.Play();
    }

    public void ParticleEnd()
    {
        corruptionParticle.Stop();
    }

    [HideInInspector] public bool pause = false;
    private bool isInv = false;
    private float elapsedInv = 0;
    public float attackDistance = 3;
    private float elapsedAgent = 0;
    private float rateAgent = 0.25f;
    public float cooldownAttack = 1f;
    private float elapsedAttack = 0;
    private void Update()
    {
        animator.SetFloat("Speed", !agent.enabled && GameManager.Instance.Player != this ? 0 : rb.velocity.magnitude);

        if (!pause)
        {
            if (this == GameManager.Instance.Player)
            {
                if (isInv)
                {
                    elapsedInv += Time.deltaTime;
                    if (elapsedInv > hitInvincibilityCountdown)
                    {
                        isInv = false;
                        elapsedInv = 0;
                    }
                }
                return;
            }
            
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

            if (!dead && elapsedAttack <= 0)
            {
                if (Vector3.Distance(GameManager.Instance.Player.transform.position, transform.position) <= attackDistance)
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
    }

    public void CorruptionStart()
    {
        GameManager.Instance.isPlayerCorrupted = true;
        GameManager.Instance.PauseChase();
        SpawnManager.Instance.pause = true;
    }

    public void CorruptionEnd()
    {
        GameManager.Instance.isPlayerCorrupted = false;
        GameManager.Instance.ResumeChase();
        SpawnManager.Instance.pause = false;
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
