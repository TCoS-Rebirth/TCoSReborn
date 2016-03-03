using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Gameplay.Entities.NPCs
{
    [Serializable]
    public class NPCGroupClass_Type : ScriptableObject
    {
        public string className;
        public bool isBoss;

        [SerializeField] public List<NPCGroupClassUnit> units = new List<NPCGroupClassUnit>();
    }

    [Serializable]
    public class NPCGroupClassUnit
    {
        public List<ENPCClassType> ForbidClassTypes = new List<ENPCClassType>();

        [Header("Number to spawn")] public int min, max;

        public List<ENPCClassType> ReqClassTypes = new List<ENPCClassType>();
    }
}