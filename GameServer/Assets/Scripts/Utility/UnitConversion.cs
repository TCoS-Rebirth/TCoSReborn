using Common.UnrealTypes;
using UnityEngine;

namespace Utility
{
    public static class UnitConversion
    {
        public const double RadToDeg = 57.295779513082321600; // 180 / Pi
        public const double DegToRad = 0.017453292519943296; // Pi / 180
        public const double UnrRotToRad = 0.00009587379924285; // Pi / 32768
        public const double RadToUnrRot = 10430.3783504704527; // 32768 / Pi
        public const float DegToUnrRot = 182.0444f;
        public const double UnrRotToDeg = 0.00549316540360483;
        public const float UnrUnitsToMeters = 0.019047619047619f;
        public const float MetersToUnrUnits = 52.5f;

        public static Vector3 ToUnity(Vector3 pos)
        {
            return new Vector3(pos.y, pos.z, pos.x)*UnrUnitsToMeters;
        }

        public static Vector3 ToUnreal(Vector3 pos)
        {
            return new Vector3(pos.z, pos.x, pos.y)*MetersToUnrUnits;
        }

        public static Quaternion ToUnity(Rotator rot)
        {
            var deg = (float) (rot.Yaw*UnrRotToDeg);
            var q = Quaternion.AngleAxis(deg, Vector3.up);
            return q;
        }

        public static Rotator ToUnreal(Quaternion quat)
        {
            var yaw = Mathf.RoundToInt(quat.eulerAngles.y*DegToUnrRot);
            return new Rotator(0, yaw, 0);
        }
    }
}