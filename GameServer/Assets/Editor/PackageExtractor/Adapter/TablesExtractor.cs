
using System;
using Database.Static;
using UnityEditor;
using UnityEngine;
using Gameplay.Loot;
using System.IO;
using Gameplay.Items;
using System.Collections.Generic;
using Common;

namespace PackageExtractor.Adapter
{
    class TablesExtractor : ExtractorAdapter
    {
        List<ItemCollection> itemCols;

        public override string Description
        {
            get
            {
                return "Extracts and saves loot and shop table data as an asset";
            }
        }

        public override string Name
        {
            get
            {
                return "Tables Extractor";
            }
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {

            var pW = extractorWindowRef.ActiveWrapper;

            var tableCol = ScriptableObject.CreateInstance<LootTableCollection>();
            var tableColPath = "Assets/GameData/LootTables/" + pW.Name + ".asset";

            //Load item collections
            itemCols = new List<ItemCollection>();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Items/");
            foreach (var f in files)
            {
                var ic = AssetDatabase.LoadAssetAtPath<ItemCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (ic != null)
                {
                    itemCols.Add(ic);
                }
            }

            AssetDatabase.CreateAsset(tableCol, tableColPath);

            //Cycle WPOs
            //If WPO is of a conversation topic class
            foreach (var wpo in pW.IterateObjects())
            {
                if (wpo.sbObject.ClassName.EndsWith("LootTable"))
                {
                    //Extract loot table
                    var table = getLootTable(wpo, resources, pW);
                    if (table != null)
                    {
                        AssetDatabase.AddObjectToAsset(table, tableCol);
                        tableCol.tables.Add(table);                        
                    }
                    else Log("Failed to extract table " + wpo.Name, Color.red);
                }
            }



            AssetDatabase.SaveAssets();

        }

        LootTable getLootTable(WrappedPackageObject wpoIn, SBResources resources, PackageWrapper pW)
        {
            var output = ScriptableObject.CreateInstance<LootTable>();

            //Name
            ReadString(wpoIn, "Name", out output.Name);
            output.name = output.Reference = wpoIn.Name;

            //Entries
            output.Entries = new List<LootTableEntry>();
            var entriesProp = wpoIn.FindProperty("Entries");

            foreach(var entry in entriesProp.IterateInnerProperties())
            {
                var newEntry = getLootTableEntry(entry, resources, pW);
                if (newEntry != null) output.Entries.Add(newEntry);
            }

            //Vars
            int tableType;
            ReadInt(wpoIn, "TableType", out tableType);
            output.TableType = (ETableType)tableType;

            ReadInt(wpoIn, "MinDropQuantity", out output.MinDropQuantity);
            ReadInt(wpoIn, "MaxDropQuantity", out output.MaxDropQuantity);
            ReadInt(wpoIn, "MoneyBase", out output.MoneyBase);
            ReadInt(wpoIn, "MoneyPerLevel", out output.MoneyPerLevel);

            return output;
        }

        LootTableEntry getLootTableEntry(SBProperty entryProp,  SBResources resources, PackageWrapper pW)
        {

            var output = new LootTableEntry();

            var itemProp = entryProp.GetInnerProperty("Item");
            var itemRes = resources.GetResource(pW.Name + "." + itemProp.Value);

            if (itemRes == null) itemRes = resources.GetResource(itemProp.Value);

            if (itemRes == null) return null;
            else { output.ItemType = getItemType(itemRes.ID); }
            if (output.ItemType == null) return null;

            output.MinQuantity = entryProp.GetInnerProperty("MinQuantity").GetValue<int>();
            output.MaxQuantity = entryProp.GetInnerProperty("MaxQuantity").GetValue<int>();
            output.Chance = entryProp.GetInnerProperty("Chance").GetValue<int>();
            output.MinLevel = entryProp.GetInnerProperty("MinLevel").GetValue<int>();
            output.MaxLevel = entryProp.GetInnerProperty("MaxLevel").GetValue<int>();

            return output;
        }

        public Item_Type getItemType(int resourceID)
        {
            for (var i = 0; i < itemCols.Count; i++)
            {
                var it = itemCols[i].GetItem(resourceID);
                if (it != null)
                {
                    return it;
                }
            }
            return null;
        }
    }
}
