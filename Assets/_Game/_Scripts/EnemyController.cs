using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private void Awake()
    {
        currentHealth = 5;
    }

    public float currentHealth
    {
        get;

        private set;
    }

    public void ApplyDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Damage");
        }
        Debug.Log("Current health : "+currentHealth);

    }

    private void Die()
    {
        animator.SetTrigger("Death");
        //TODO implements Die method
        //...
    }

    //ATTACK TO DO
    public void Attack()
    {
        animator.SetTrigger("Attack");
    }
}
