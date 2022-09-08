using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordBehaviour : MonoBehaviour
{
    [SerializeField] private LayerMask obstaclesLayers;
    [HideInInspector] public bool swinging;

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
        }/*
        else if (obstaclesLayers == (obstaclesLayers | (1 << other.gameObject.layer)))
        {
            Vector3 reflectVec = Vector3.Reflect(transform.forward, transform.position.normalized);
            reflectVec.y = 0;
            GameManager.Instance.launchDir = reflectVec;
        }*/
    }
}
