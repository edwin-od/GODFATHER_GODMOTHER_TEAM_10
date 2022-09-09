using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class SwordBehaviour : MonoBehaviour
{
    [SerializeField] private LayerMask obstaclesLayers;
    [HideInInspector] public bool swinging;
    [SerializeField] CapsuleCollider col;
    [SerializeField] private float attackColSize = 1;
    [SerializeField] private float throwColSize = 1;

    public void SetColliderRadius(bool attack)
    {
        col.radius = attack ? attackColSize : throwColSize;
    }

    private void Start()
    {
        SetColliderRadius(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyController perso = other.GetComponent<EnemyController>();
        if (perso && GameManager.Instance.Player != perso)
        {
            if (swinging && !GameManager.Instance.isPlayerTransition)
            {
                perso.ApplyDamage(GameManager.Instance.Player.enemySO.dmg);
            }
            else if (GameManager.Instance.isPlayerTransition)
            {
                GameManager.Instance.ChangePlayer(perso);
            }
        }
        else if (obstaclesLayers == (obstaclesLayers | (1 << other.gameObject.layer)))
        {
            GameManager.Instance.LaunchLimit();
        }
    }
}
