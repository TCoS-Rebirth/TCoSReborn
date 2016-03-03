using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Items
{
    public class ItemCollection : ScriptableObject
    {
        public List<Item_Type> items = new List<Item_Type>();

        public Item_Type GetItem(int id)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].resourceID == id)
                {
                    return items[i];
                }
            }
            return null;
        }
    }
}