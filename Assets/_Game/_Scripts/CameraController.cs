using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float camFollowSpeed = 10f;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        Vector3 aprox = Vector3.Lerp(transform.position, GameManager.Instance.Player.transform.position, Time.deltaTime * camFollowSpeed);
        transform.position = Vector3.Distance(PlayerController.Instance.transform.position, aprox) > 0.01f ? aprox : GameManager.Instance.Player.transform.position;
    }
}
