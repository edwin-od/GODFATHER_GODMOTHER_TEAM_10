using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public EnemySO enemySO;
    public EnemySO EnemySO => enemySO;

    [HideInInspector] public bool possessed = false;

    private void Start()
    {
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
        Debug.Log("Current health : "+currentHealth);

    }

    private void Die()
    {
        //animator.SetTrigger("Death");
        //TODO implements Die method
        //...

        GameManager.Instance.RemoveEnemy(this);
    }

    //ATTACK TO DO
    public void Attack()
    {
        //animator.SetTrigger("Attack");
    }
}
