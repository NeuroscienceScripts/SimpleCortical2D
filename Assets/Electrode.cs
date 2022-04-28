namespace BionicVisionVR.Structs
{
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
            this.current = 0;
        }

        public override string ToString()
        {
            return xPosition + ", " + yPosition + ", " + ":  " + current; 
        }
    }
}