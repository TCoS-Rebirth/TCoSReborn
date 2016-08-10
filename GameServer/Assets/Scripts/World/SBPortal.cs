using System;
using UnityEngine;

namespace World
{
    public class SBPortal : ScriptableObject
    {
        public SBWorld TargetWorld;
        public SBPortal EntryPortal;
        public string Tag;
        [NonSerialized, HideInInspector]
        public GameObject LevelActor;
    }
}