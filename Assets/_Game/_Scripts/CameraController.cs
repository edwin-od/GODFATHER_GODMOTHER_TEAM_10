using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float camFollowSpeed = 10f;

    void Update()
    {
        if (!GameManager.Instance) return;

        Transform follow = GameManager.Instance.isPlayerTransition ? GameManager.Instance.sword.transform : GameManager.Instance.Player.transform;
        
        Vector3 aprox = Vector3.Lerp(transform.position, follow.position, Time.deltaTime * camFollowSpeed);
        transform.position = Vector3.Distance(follow.position, aprox) > 0.01f ? aprox : follow.position;
    }
}
