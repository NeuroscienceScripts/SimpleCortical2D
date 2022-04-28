using System;
using System.Collections;
using System.Collections.Generic;
using BionicVisionVR.Structs;
using UnityEngine;

public class RunShaders : MonoBehaviour
{
    public static RunShaders Instance { get; private set; }
    
    public Electrode[] electrodes;
    public (float, float)[] pixelMap; 
    public CorticalModel cm; 

    private void Start()
    {
        cm = CorticalModel.Instance;
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        throw new NotImplementedException();
    }
}
