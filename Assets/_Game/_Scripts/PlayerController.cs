using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour , IDamageable
{
    public KeyCode up = KeyCode.UpArrow;
    public KeyCode down = KeyCode.DownArrow;
    public KeyCode left = KeyCode.LeftArrow;
    public KeyCode right = KeyCode.RightArrow;

    public float translationSpeed = 5;
    //public float rotationSpeed = 100;


    private static PlayerController _instance;
    public static PlayerController Instance => _instance;
    void Awake()
    {
        _instance = this;
            
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        currentHealth = 5;
    }

    private void Update()
    {
        Vector3 dir = (Vector3.forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal")).normalized * Time.deltaTime * translationSpeed;
        transform.position += dir;
        transform.rotation = Quaternion.LookRotation(dir.magnitude == 0 ? transform.forward : dir.normalized);

        //transform.rotation *= Quaternion.Euler(0, Time.deltaTime * rotationSpeed * Input.GetAxis("Horizontal"), 0);
    }
    
    //IDamageable
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
            
        }
        Debug.Log("Current health : "+currentHealth);

    }

    private void Die()
    {
        //TODO implements Die method
        //...
    }
}
