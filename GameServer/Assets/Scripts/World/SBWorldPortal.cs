using Gameplay.Entities;
using UnityEngine;

namespace World
{
    public class SBWorldPortal : SBBasePortal
    {
        public float collisionHeight;

        public float collisionRadius;

        public PlayerStart Destination;

        public Zone TargetZone;

        void Start()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        [ContextMenu("FixColliders")]
        public void FixColliders()
        {
            var portals = FindObjectsOfType<SBWorldPortal>();
            foreach (var portal in portals)
            {
                var cols = portal.GetComponents<Collider>();
                CapsuleCollider requiredCollider = null;
                foreach (var col in cols)
                {
                    var sc = col as CapsuleCollider;
                    if (sc != null)
                    {
                        if (requiredCollider != null)
                        {
                            DestroyImmediate(sc, false);
                            continue;
                        }
                        requiredCollider = sc;
                    }
                    if (col is BoxCollider)
                    {
                        DestroyImmediate(col, false);
                    }
                }
                if (requiredCollider == null) requiredCollider = portal.gameObject.AddComponent<CapsuleCollider>();
                requiredCollider.radius = portal.collisionRadius;
                requiredCollider.height = portal.collisionHeight;
            }
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