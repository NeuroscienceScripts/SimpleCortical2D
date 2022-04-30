using System;
using Unity.VisualScripting;

public struct Electrode
{
    public float xPosition;
    public float yPosition;
    public float screenPosX;
    public float screenPosY;
    public float current;
    

    public Electrode(float xPosition, float yPosition, float screenPosX, float screenPosY, float current)
    {
        this.xPosition = xPosition;
        this.yPosition = yPosition;
        this.screenPosX = screenPosX;
        this.screenPosY = screenPosY;
        this.current = current;
    }

    public static Electrode[] SetRectangularArray(int numberElectrodesX, int numberElectrodesY, float electrodeSpacing, float rotation, float xPos, float yPos, int xRes, int yRes)
    {
        Electrode[] electrodes = new Electrode[numberElectrodesX * numberElectrodesY]; 
        float x_pos;
        float y_pos;
        float x_temp;
        float y_temp;
        
        float screen_x_pos;
        float screen_y_pos;

        for (int row = 0; row < numberElectrodesY; row++)
        {
            for (int col = 0; col < numberElectrodesX; col++)
            {
                // Space out electrodes
                x_pos = (col - numberElectrodesX / 2.0f + 0.5f) * electrodeSpacing;
                y_pos = (row - numberElectrodesY / 2.0f + 0.5f) * electrodeSpacing;
                screen_x_pos = (col - numberElectrodesX / 2.0f + 0.5f) * (1.0f / numberElectrodesX); 
                screen_y_pos = (col - numberElectrodesY / 2.0f + 0.5f) * (1.0f / numberElectrodesY);

                // Rotate electrodes 
                float deg2rad = (float) Math.PI / 180.0f; 
                x_temp = (float) (x_pos * Math.Cos(deg2rad*rotation) - y_pos * Math.Sin(deg2rad*rotation));
                y_temp = (float) (y_pos * Math.Sin(deg2rad*rotation) + y_pos * Math.Cos(deg2rad*rotation));

                // Shift electrodes
                x_pos = x_temp + xPos; 
                y_pos = y_temp + yPos;


                float minDist = 9999.0f; 
                for(int y=0; y<yRes; y++)
                    for (int x = 0; x < xRes; x++)
                    {
                        float distX = x_pos - RunShaders.Instance.pixelMap[(y*xRes) + x].Item1; 
                        float disty = y_pos - RunShaders.Instance.pixelMap[(y*xRes) + x].Item2;
                        if (Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(disty, 2)) < minDist)
                        {
                            minDist = (float) (Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(disty, 2)));
                            screen_x_pos = x;
                            screen_y_pos = y; 

                        }
                    }
                   

                electrodes[GetLocation1D(row, col, numberElectrodesX)] = new Electrode(x_pos, y_pos, screen_x_pos, screen_y_pos, 0.0f);
            }
        }

        return electrodes; 
    }

    public static int GetLocation1D(int row, int col, int numColumns)
    {
        return (row * numColumns) + col; 
    }
    public static (float, float) GetLocation2D(int loc1D, int numColumns)
    {
        return (((float) loc1D / numColumns), (float) loc1D % numColumns); 
    }

    public override string ToString()
    {
        return xPosition + ", " + yPosition + "(" + screenPosX + "," + screenPosY + "):  " + current; 
    }
    
}
