using System;
using Common;
using UnityEngine;

namespace Gameplay.Items
{
    [Serializable]
    public class Game_Item : ScriptableObject
    {
        public byte Attuned = 0;
        public int CharacterID = -1;
        public byte Color1 = 0;
        public byte Color2 = 0;
        public int DBID = 0;
        public int LocationID = 0;
        public int LocationSlot = 0;
        public EItemLocationType LocationType = EItemLocationType.ILT_Unknown;
        public int StackSize = 1;
        public Item_Type Type;
        public float UseTime;
    }
}