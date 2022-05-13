using System;
using UnityEngine;

/// <summary>
/// Degrees and microns are listed with negative/positive values assuming (0,0) is at the center of the screen
/// Screen position is solely positive and spans 0:1 (with .5 being at the center of the screen)
///
/// When converting from screen position to other coordinates (or vise versa), the y-axis needs inverted.
/// In screen position, y=0 is the top of the screen and y=1 is the bottom
/// 
/// </summary>
public class UnitConverter
{
    public static int flatten2DElectrodePosition(int row, int col)
    {
        return row + RunShaders.Instance.numberElectrodesY * col;
    }
    
    public static int flatten2DPixel(int x, int y)
    {
        return x + RunShaders.Instance.xRes * y; 
    }
    
    public static int[] unflatten2DPixel(int flattened)
    {
        int x = flattened / RunShaders.Instance.xRes;
        int y = flattened % RunShaders.Instance.xRes;
        return new int[]{x,y}; 
    }

    public static float degreeToScreenPos(float degree)
    {
        return ((RunShaders.Instance.headset_fov / 2.0f) + degree) / RunShaders.Instance.headset_fov;
    }
    
    public static float degreeToScreenPos(float degree, bool invert)
    {
        return ((RunShaders.Instance.headset_fov / 2.0f) - degree) / RunShaders.Instance.headset_fov;
    }

    public static float pixelToDegree(int pixel)
    {
        return pixel * (RunShaders.Instance.headset_fov / RunShaders.Instance.xRes) - RunShaders.Instance.headset_fov/2.0f; 
    }
    
    public static float screenPosToDegree(float screenPos)
    {
        return (screenPos - 0.5f) * RunShaders.Instance.headset_fov; 
    }

    public static float screenPosToDegree(float screenPos, bool invert)
    {
        return invert? screenPosToDegree(1 - screenPos) : screenPosToDegree(screenPos);
    }
    
}