using System;
using System.Collections.Generic;
using Gameplay.Items;

namespace Gameplay
{
    [Serializable]
    public class Content_Inventory
    {
        public List<ContentItem> Items = new List<ContentItem>();

        [Serializable]
        public class ContentItem
        {
            public byte Color1;
            public byte Color2;
            public Item_Type Item;
            public int itemID;
            public int StackSize;
            public string temporaryItemName;
        }
    }
}