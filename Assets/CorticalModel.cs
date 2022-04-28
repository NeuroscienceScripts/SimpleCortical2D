using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using BionicVisionVR.Structs;
using UnityEngine;

public class CorticalModel : MonoBehaviour
{
    public static CorticalModel Instance { get; private set; }
    
    public float headsetFOV;
    public RunShaders rs;

    public float _k = 15.0f;
    public float _a = 0.5f;
    public float _b = 90.0f;
    public float _alpha1 = 1.0f;
    public float _alpha2 = 0.333f;
    public float _alpha3 = 0.25f;

    public bool MapPixelsToCortex(int xRes, int yRes)
    {
        for (int y = 0; y < yRes; y++)
        {
            for (int x = 0; x < xRes; x++)
            {
                float screenPosX = (float)x/xRes;
                float screenPosY = (float)y/yRes;
                rs.pixelMap[(x * yRes) + y] = GetCortexLocation(screenPosX, screenPosY);
            }
        }
        return true; 
    }

    public (float, float) GetCortexLocation(float screenPosX, float screenPosY)
    {
        float radius = (float) Math.Sqrt((screenPosX * screenPosX) + (screenPosY * screenPosY));
        float angle = (float) Math.Atan(screenPosY / screenPosX);

        (float, float) visualFieldLocation = ((radius * headsetFOV), angle);

        float thetaV1 = _alpha1 * angle;
        Complex zV1 = radius * Complex.Exp(new Complex() * (Complex)thetaV1);
        Complex wV1 = (_k * Complex.Log((zV1 + _a) / (zV1 + _b)) - _k * (float) Math.Log(_a / _b));
        (float, float) xV1, yV1 = np.real(wV1), np.imag(wV1)
        idx_nan = ((theta < -np.pi/2) | (theta > np.pi/2) | (radius < 0) |
                   (radius > 90) | (xV1 < 0) | (xV1 > 180))
        xV1[idx_nan], yV1[idx_nan] = np.nan, np.nan
        return xV1, yV1

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
}