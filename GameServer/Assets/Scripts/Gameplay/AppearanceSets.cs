#pragma warning disable 414
using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Gameplay
{
    public class AppearanceSets : ScriptableObject
    {
        public List<AppearanceSet> sets = new List<AppearanceSet>();

        [Serializable]
        public class AppearanceSet
        {
            public List<SetItem> items = new List<SetItem>();

            [SerializeField, HideInInspector] string name = "Set"; //only visual (Inspector)

            public EquipmentSlot slot;

            public AppearanceSet()
            {
            }

            public AppearanceSet(EquipmentSlot slot)
            {
                this.slot = slot;
                name = slot.ToString();
            }
        }

        [Serializable]
        public class SetItem
        {
            public int index;
            public int itemID;
            public string temporaryAppearanceName;

            public SetItem(int index, string temporaryName)
            {
                this.index = index;
                temporaryAppearanceName = temporaryName;
            }
        }
    }
}