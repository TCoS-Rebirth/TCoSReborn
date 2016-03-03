using UnityEngine;

namespace Utility
{
    public class DevWorldNote : MonoBehaviour
    {
        public int characterID;
        public string note = "";
        public int zoneID;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawCube(transform.position, Vector3.one);
        }
    }
}