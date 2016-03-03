using System;

namespace Common.UnrealTypes
{
    [Serializable]
    public class Rotator
    {
        public int Pitch;
        public int Roll;
        public int Yaw;

        public Rotator()
        {
        }

        public Rotator(int rPitch, int rYaw, int rRoll)
        {
            Pitch = rPitch;
            Yaw = rYaw%65536;
            Roll = rRoll;
        }

        public static Rotator Zero
        {
            get { return new Rotator(); }
        }
    }
}