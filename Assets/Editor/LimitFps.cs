using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFps : MonoBehaviour
{
    public int frameRate;
    
    private void Update()
    {
        Application.targetFrameRate = frameRate;
    }
}
