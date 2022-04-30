using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunShaders : MonoBehaviour
{
    public static RunShaders Instance { get; private set; }

    public Material shaderMaterial;
    public Material preProcessingMaterial; 
    
    public bool useRectangularArray = true; 
    public int numberElectrodesX = 10;
    public float screenInputSpacingX;
    public int numberElectrodesY = 10;
    public float screenInputSpacingY;
    public (float, float) screenInputCenter = (0, 0); 
    public float electrodeSpacing = 5.0f;
    public float rotation = 0.0f;
    public float xPosition = 0.0f;
    public float yPosition = 0.0f;
    public float amplitude = .1f; 

    public Electrode[] electrodes;
    public float[] pixelsToElectrodesGauss; 
    public (float, float)[] pixelMap; 
    public CorticalModel cm;

    public bool runCorticalModel; 
    
    private ComputeBuffer electrodesBuffer;
    private ComputeBuffer pixelsToElectrodesGaussBuffer;

    private int currentFrame = 0; 

    private void Start()
    {
        cm = CorticalModel.Instance;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (currentFrame == 0)
        {
            if(cm.MapPixelsToCortex(src.width, src.height))
                if (useRectangularArray)
                    electrodes = Electrode.SetRectangularArray(numberElectrodesX, numberElectrodesY, electrodeSpacing, rotation, xPosition, yPosition, src.width, src.height); 
            
            cm.GetGaussToElectrodes(); 
        }
        
        electrodesBuffer = new ComputeBuffer(electrodes.Length,
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(Electrode)), ComputeBufferType.Default);
        Graphics.SetRandomWriteTarget(2, electrodesBuffer, true);
        shaderMaterial.SetBuffer("electrodesBuffer", electrodesBuffer);
        preProcessingMaterial.SetBuffer("electrodesBuffer", electrodesBuffer);
        electrodesBuffer.SetData(electrodes);
        
        pixelsToElectrodesGaussBuffer = new ComputeBuffer(electrodes.Length * src.width * src.height,
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)), ComputeBufferType.Default);
        pixelsToElectrodesGaussBuffer.SetData(pixelsToElectrodesGauss);
        shaderMaterial.SetBuffer("pixelsToElectrodesGaussBuffer", pixelsToElectrodesGaussBuffer);

        preProcessingMaterial.SetInt("numberElectrodes", electrodes.Length);
        preProcessingMaterial.SetInt("xResolution", src.width);
        preProcessingMaterial.SetInt("yResolution", src.height); 
        preProcessingMaterial.SetFloat("amplitude", amplitude);
        
        
        shaderMaterial.SetInt("xResolution", src.width);
        shaderMaterial.SetInt("yResolution", src.height);
        shaderMaterial.SetInt("numberElectrodes", electrodes.Length);

        RenderTexture tmp = RenderTexture.GetTemporary((int) src.width, 
            (int) src.height, 0); 
        Graphics.Blit(src, tmp, preProcessingMaterial);
        
        if(runCorticalModel)
            Graphics.Blit(src, dest, shaderMaterial);
        else
            Graphics.Blit(tmp, dest);

        // if (currentFrame % 1000 == 0)
        // {
        //     electrodesBuffer.GetData(electrodes); 
        //     foreach (var electrode in electrodes)
        //     {
        //         Debug.Log(electrode);
        //     }
        // }
        
        electrodesBuffer.Dispose();
        pixelsToElectrodesGaussBuffer.Dispose();

        currentFrame++; 
    }
    
    private void Awake()
    {
        if ( Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
