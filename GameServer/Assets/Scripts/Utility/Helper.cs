using System;
using System.Text;
using UnityEngine;

namespace Utility
{
    public static class Helper
    {
        public static bool EnumTryParse<T>(string strType, out T result)
        {
            var strTypeFixed = strType.Replace(' ', '_');
            if (Enum.IsDefined(typeof (T), strTypeFixed))
            {
                result = (T) Enum.Parse(typeof (T), strTypeFixed, true);
                return true;
            }
            foreach (var value in Enum.GetNames(typeof (T)))
            {
                if (!value.Equals(strTypeFixed, StringComparison.OrdinalIgnoreCase)) continue;
                result = (T) Enum.Parse(typeof (T), value);
                return true;
            }
            result = default(T);
            return false;
        }

        public static string ByteArrayToHex(byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length*2);
            const string hexAlphabet = "0123456789ABCDEF";
            foreach (var b in bytes)
            {
                result.Append(hexAlphabet[b >> 4]);
                result.Append(hexAlphabet[b & 0xF]);
            }
            return result.ToString();
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        public static Vector3 QuadraticSplinePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            //t = Clamp(t,0,1);
            var u = 1.0f - t;
            var tt = t * t;
            var uu = u * u;
            var uuu = uu * u;
            var ttt = tt * t;
            var p = uuu * p0; //first term
            p += 3 * uu * t * p1; //second term 
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term
            return p;
        }

        public static Vector3 SquareSplinePoint(Vector3 Start, Vector3 Control, Vector3 End, float t)
        {
            return (1 - t) * (1 - t) * Start + 2 * t * (1 - t) * Control + t * t * End;
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
        }
    }
}