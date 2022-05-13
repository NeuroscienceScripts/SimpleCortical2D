using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class RunShaders : MonoBehaviour
{
    public static RunShaders Instance { get; private set; }

    public Material shaderMaterial;
    public Material preProcessingMaterial; 
    
    public bool useRectangularArray = true; 
    public int numberElectrodesX = 10;
    public int numberElectrodesY = 10;
    public int downscaleFactor = 1;
    public float headset_fov = 55.0f; 
    
    public float electrodeSpacing = 5.0f;
    public float rotation = 0.0f;
    public float xPosition = 0.0f;
    public float yPosition = 0.0f;
    public float amplitude = .1f; 

    public Electrode[] electrodes;
    public float[] pixelsToElectrodesGauss; 
    public (float, float)[] pixelMap; 
    public CorticalModel cm;
    public int xRes;
    public int yRes; 
    
    public bool runCorticalModel; 
    
    private ComputeBuffer electrodesBuffer;
    private ComputeBuffer pixelsToElectrodesGaussBuffer;

    private int currentFrame = 0; 

    private void Start()
    {
        cm = CorticalModel.Instance;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
            runCorticalModel = !runCorticalModel; 
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(src.width / downscaleFactor, src.height / downscaleFactor, 0);
        Graphics.Blit(src, tmp); 
        if (currentFrame == 0)
        {
            xRes = (int) Math.Floor((double) tmp.width / (double) downscaleFactor);
            yRes = (int) Math.Floor((double) tmp.height / (double) downscaleFactor);

            Debug.Log("X-res" + xRes);
            Debug.Log("Y-res" + yRes);

            if (cm.MapPixelsToCortex(xRes, yRes))
            {
                if (useRectangularArray)
                     electrodes = Electrode.SetRectangularArray(numberElectrodesX, numberElectrodesY,
                        electrodeSpacing, rotation, xPosition, yPosition, xRes, yRes);
            }
            else Debug.Log("Failed to create cortical map");

            InitializeElectrodes(); 
        }

       
        shaderMaterial.SetBuffer("electrodesBuffer", electrodesBuffer);
        preProcessingMaterial.SetBuffer("electrodesBuffer", electrodesBuffer);
        electrodesBuffer.SetData(electrodes);
        
        
        pixelsToElectrodesGaussBuffer.SetData(pixelsToElectrodesGauss);
        shaderMaterial.SetBuffer("pixelsToElectrodesGaussBuffer", pixelsToElectrodesGaussBuffer);

        preProcessingMaterial.SetInt("numberElectrodes", electrodes.Length);
        preProcessingMaterial.SetInt("xResolution", tmp.width);
        preProcessingMaterial.SetInt("yResolution", tmp.height); 
        preProcessingMaterial.SetFloat("amplitude", amplitude);
        
        
        shaderMaterial.SetInt("xResolution", tmp.width);
        shaderMaterial.SetInt("yResolution", tmp.height);
        shaderMaterial.SetInt("numberElectrodes", electrodes.Length);

        RenderTexture tmp2 = RenderTexture.GetTemporary((int) tmp.width, 
            (int) tmp.height, 0); 
        Graphics.Blit(tmp, tmp2, preProcessingMaterial);
        RenderTexture tmp3 = RenderTexture.GetTemporary(tmp2.width, tmp2.height, 0); 
        
        if(runCorticalModel)
            Graphics.Blit(tmp2, tmp3, shaderMaterial);
        else
            Graphics.Blit(tmp2, tmp3);

        Graphics.Blit(tmp3, dest); 

        // if (currentFrame % 1000 == 0)
        // {
        //     electrodesBuffer.GetData(electrodes); 
        //     foreach (var electrode in electrodes)
        //     {
        //         Debug.Log(electrode);
        //     }
        // }
        RenderTexture.ReleaseTemporary(tmp); 
        RenderTexture.ReleaseTemporary(tmp2); 
        RenderTexture.ReleaseTemporary(tmp3); 
        currentFrame++; 
    }

    private void InitializeElectrodes()
    {
        cm.GetGaussToElectrodes();
        electrodesBuffer = new ComputeBuffer(electrodes.Length,
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(Electrode)), ComputeBufferType.Default);
        Graphics.SetRandomWriteTarget(2, electrodesBuffer, true);
        pixelsToElectrodesGaussBuffer = new ComputeBuffer(electrodes.Length * xRes * yRes,
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)), ComputeBufferType.Default);
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
