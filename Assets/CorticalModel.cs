using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CorticalModel : MonoBehaviour
{
    public static CorticalModel Instance { get; private set; }
    
    public float headsetFOV;
    public RunShaders rs;

    public float currentSpread_sigma = 0.276f; // S/m
    public float _k = 15.0f;
    public float _a = 0.5f;
    public float _b = 90.0f;
    public float _alpha1 = 1.0f;
    public float _alpha2 = 0.333f;
    public float _alpha3 = 0.25f;

    public bool MapPixelsToCortex(int xRes, int yRes)
    {
        rs.pixelMap = new (float, float)[xRes * yRes]; 
        Debug.Log("Creating cortical map from screen resolution: " + xRes + "," + yRes );
        for (int y = 0; y < yRes; y++)
        {
            for (int x = 0; x < xRes; x++)
            {
                float screenPosX = (float)x/xRes;
                float screenPosY = (float)y/yRes;
                rs.pixelMap[(y * xRes) + x] = GetCortexLocationV1(screenPosX, 1-screenPosY);
            }
        }
        Debug.Log("Finished creating cortical map, size=" + rs.pixelMap.Length);
        return rs.pixelMap != null; 
    }

    private (float, float) GetCortexLocationV1(float screenPosX, float screenPosY)
    {
        float radius = (float) Math.Sqrt((screenPosX * screenPosX) + (screenPosY * screenPosY));
        float angle = (float) Math.Atan(screenPosY / screenPosX);
        
        float thetaV1 = _alpha1 * angle;
        Complex zV1 = radius * Complex.Exp(new Complex(0,1) * (Complex)thetaV1);
        Complex wV1 = (_k * Complex.Log((zV1 + _a) / (zV1 + _b)) - _k * (float) Math.Log(_a / _b));
        float xV1 = (float) wV1.Real;
        float yV1 =(float) wV1.Imaginary;
        
        if ((angle < -Math.PI / 2) | (angle > Math.PI / 2) || (radius < 0) || (radius > 90) || (xV1 < 0) || (xV1 > 180))
            return (9999, 9999);

        return (float.IsNaN(xV1) ? 9999 : xV1, float.IsNaN(yV1) ? 9999 : yV1); 
    }

    public bool GetGaussToElectrodes()
    {
        int pToElecSize = rs.pixelMap.Length * rs.electrodes.Length;
        Debug.Log("Calculating gauss distances for " + pToElecSize + "(" + rs.pixelMap.Length + "," +
                  rs.electrodes.Length + ")"); 
        rs.pixelsToElectrodesGauss = new float[pToElecSize];
        for(int p=0; p < rs.pixelMap.Length; p++)
            for(int e=0; e < rs.electrodes.Length; e++)
            {
                float distX = (float) Math.Pow((rs.pixelMap[p].Item1 - rs.electrodes[e].xPosition), 2);
                float distY = (float) Math.Pow((rs.pixelMap[p].Item2 - rs.electrodes[e].yPosition), 2);
                float dist = (float) Math.Sqrt( distX + distY);

                float gauss = 1.0f / (4.0f * (float) Math.PI * currentSpread_sigma * dist);
                gauss = gauss < 0 ? 0 : gauss;
                gauss = gauss > 1 ? 1 : gauss; 
                rs.pixelsToElectrodesGauss[(p * rs.electrodes.Length) + e] = gauss;
            }

        return true; 
    }
    
    private void Start()
    {
        rs = RunShaders.Instance;
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