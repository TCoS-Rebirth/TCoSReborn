using System.Collections.Generic;
using UnityEngine;

namespace World.Paths
{
    public class PatrolPoint : AIPoint
    {
        public bool bWalking;
        public List<string> groups = new List<string>();
        public float MinimalPathHeight;
        public List<AIPath> PatrolPaths = new List<AIPath>();
        public bool PausePatrolScript;
        public bool showPoints;

        void OnDrawGizmosSelected()
        {
            if (showPoints)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                var size = new Vector3(0.1f, 1f, 0.1f);
                for (var i = 0; i < PatrolPaths.Count; i++)
                {
                    for (var p = 0; p < PatrolPaths[i].Path.Count; p++)
                    {
                        Gizmos.DrawWireCube(PatrolPaths[i].Path[p], size);
                    }
                }
            }
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.25f);
            if (PatrolPaths.Count == 0)
            {
                return;
            }
            for (var i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].ConnectedActor != null)
                {
                    Gizmos.DrawLine(transform.position, Connections[i].ConnectedActor.transform.position);
                }
            }
        }
    }
}