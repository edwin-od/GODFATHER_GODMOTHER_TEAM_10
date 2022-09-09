using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class CanvasAnim : MonoBehaviour
{

    public Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void End()
    {
        anim.SetTrigger("End");
    }

    public void StartGame()
    {
        anim.SetTrigger("Start");
    }

    public void Restart()
    {
        
    }
}

