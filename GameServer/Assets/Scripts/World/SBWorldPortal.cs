using Gameplay.Entities;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof (BoxCollider))]
    public class SBWorldPortal : Trigger
    {
        public float collisionHeight;

        public float collisionRadius;

        //public SBWorldPortal EntryPortal;

        public PlayerStart Destination;

        public string PortalTag; //'Tag'

        public Zone TargetZone;

        void Start()
        {
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;
        }

        protected override void OnEnteredTrigger(Character ch)
        {
            if (Destination == null)
            {
                Debug.LogWarning("No PlayerStart set");
                return;
            }
            var p = ch as PlayerCharacter;
            if (p != null)
            {
                if (TargetZone.IsEnabled)
                {
                    GameWorld.Instance.TravelPlayer(p, Destination);
                }
                else
                {
                    Debug.LogWarning("TODO Handle zone disabled");
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Portal.psd");
        }

        void OnDrawGizmosSelected()
        {
            if (Destination != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, Destination.Position);
            }
        }
    }
}