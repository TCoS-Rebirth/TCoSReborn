using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Paths
{
    public class PathPoint : MonoBehaviour
    {
        public List<PathConnection> Connections = new List<PathConnection>();

        [Serializable]
        public class PathConnection
        {
            public PathPoint ConnectedActor;
            public float MoveSpeed;
            public bool Walking;
        }
    }

    [Serializable]
    public class AIPath
    {
        public List<AnnotationPoint> Annotations = new List<AnnotationPoint>();
        public bool Complete;
        public AnnotationPoint ControlPoint;

        public List<Vector3> Path = new List<Vector3>();
        public string referenceName;
    }
}