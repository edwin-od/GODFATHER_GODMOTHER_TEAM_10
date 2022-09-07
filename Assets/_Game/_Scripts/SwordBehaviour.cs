using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordBehaviour : MonoBehaviour
{
    [SerializeField] private LayerMask obstaclesLayers;

    private void OnTriggerEnter(Collider other)
    {
        if (!GameManager.Instance.isPlayerTransition) return;
        if (other.CompareTag("Enemy"))
        {
            GameManager.Instance.ChangePlayer(other.GetComponent<EnemyController>());
        }
        if (obstaclesLayers == (obstaclesLayers | (1 << other.gameObject.layer)))
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, Vector3.forward);
            Physics.Raycast(ray, out hit);
            Vector3 reflectVec = Vector3.Reflect(Vector3.forward, hit.normal);

        }
    }
}
