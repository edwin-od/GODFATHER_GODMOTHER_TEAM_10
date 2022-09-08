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
            if (GameManager.Instance.isPlayerTransition)
            {
                GameManager.Instance.ChangePlayer(other.GetComponent<EnemyController>());
            }
            if (swinging)
            {
                perso.ApplyDamage(perso.enemySO.dmg);
            }
        }
        else if (obstaclesLayers == (obstaclesLayers | (1 << other.gameObject.layer)))
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, Vector3.forward);
            Physics.Raycast(ray, out hit);
            Vector3 reflectVec = Vector3.Reflect(Vector3.forward, hit.normal);
        }
    }
}
