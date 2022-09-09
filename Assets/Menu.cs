using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    
    public CanvasAnim _AnimInstance;
    void Start()
    {
        _AnimInstance.StartGame();
    }

    void Update()
    {
        if (Input.anyKey)
        {
            SceneManager.LoadScene("SampleScene 1");
        }
    }
}
