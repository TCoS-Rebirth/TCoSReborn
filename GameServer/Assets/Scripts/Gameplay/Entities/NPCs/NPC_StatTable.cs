using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities.NPCs
{
    public class NPC_StatTable : ScriptableObject
    {
        [Header("Stat")] public int BasePoints;

        public int LevelPerPoints;
        public int PointsMultiplier;
        public List<StatPreview> Preview = new List<StatPreview>();

        [Header("Preview")] public int PreviewLevel;

        public int resourceID;

        [ContextMenu("MakePreview")]
        protected void MakePreview()
        {
            for (var i = 0; i < PreviewLevel; i++)
            {
                var sp = new StatPreview();
                sp.B = GetBody(i);
                sp.M = GetMind(i);
                sp.F = GetFocus(i);
                Preview.Add(sp);
            }
        }

        public virtual int PointsAtLevel(int aLevel)
        {
            if (LevelPerPoints == 0)
            {
                return BasePoints + aLevel;
            }
            return BasePoints + aLevel/LevelPerPoints;
        }

        public virtual int GetFocus(int aLevel)
        {
            return 0;
        }

        public virtual int GetMind(int aLevel)
        {
            return 0;
        }

        public virtual int GetBody(int aLevel)
        {
            return 0;
        }

        public virtual int GetHitpointsPerLevel(int aLevel)
        {
            return 10;
        }

        public virtual int GetBaseHitpoints(int aLevel)
        {
            return 100;
        }

        [Serializable]
        public class StatPreview
        {
            public int B;
            public int F;
            public int M;
        }
    }
}