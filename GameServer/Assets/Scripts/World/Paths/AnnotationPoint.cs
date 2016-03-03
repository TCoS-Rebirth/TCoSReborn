using System.Collections.Generic;
using Common;
using UnityEngine;

namespace World.Paths
{
    public class AnnotationPoint : PathPoint
    {
        public List<EAnnotationType> AnnotationMask = new List<EAnnotationType>();
        public EAnnotationType AnnotationType;
        public bool CreateNode;
        public Vector3 Extent;
        public bool FitNode;
    }
}