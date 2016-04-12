using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Gameplay.Loot
{
    public class LootTableCollection : ScriptableObject
    {
        public List<LootTable> tables = new List<LootTable>();

    }
}
