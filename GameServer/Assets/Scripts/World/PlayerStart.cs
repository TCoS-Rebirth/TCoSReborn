using Common;
using UnityEngine;

namespace World
{
    public class PlayerStart : MonoBehaviour
    {
        public bool IsRespawn;

        [SerializeField, HideInInspector] public MapIDs MapID;

        public string NavigationTag;

        public Vector3 Position
        {
            get { return transform.position; }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation; }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Destination.psd");
        }
    }
}