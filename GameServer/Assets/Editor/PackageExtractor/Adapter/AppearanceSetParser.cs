using Database.Static;
using Gameplay;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class AppearanceSetParser : ExtractorAdapter
    {
        AppearanceSets appearanceAsset;

        public override string Name
        {
            get { return "AppearanceSet Parser"; }
        }

        public override string Description
        {
            get { return "Tries to fill the appearance asset with the CharacterAppearance names, set needs further processing then"; }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Asset:");
            appearanceAsset = EditorGUILayout.ObjectField(appearanceAsset, typeof (AppearanceSets), false) as AppearanceSets;
            GUILayout.EndHorizontal();
        }

        public override bool IsCompatible(PackageWrapper p)
        {
            return base.IsCompatible(p) && p.Name.Contains("AppearanceSetGP");
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            Log("obsolete, disabled", Color.red);
            /*
            if (appearanceAsset == null) { Log("Asset not assigned!", Color.yellow); return; }
            appearanceAsset.sets.Clear();
            Dictionary<EquipmentSlot, string> setDefinitions = new Dictionary<EquipmentSlot, string>()
            {
                {EquipmentSlot.ES_CHEST,"ChestClothesSet"},
                {EquipmentSlot.ES_LEFTGLOVE,"LeftGloveSet"},
                {EquipmentSlot.ES_RIGHTGLOVE, "RightGloveSet"},
                {EquipmentSlot.ES_PANTS, "PantsSet"},
                {EquipmentSlot.ES_SHOES, "ShoesSet"},
                {EquipmentSlot.ES_HELMET, "HeadGearSet"},
                {EquipmentSlot.ES_LEFTSHOULDER, "LeftShoulderSet"},
                {EquipmentSlot.ES_RIGHTSHOULDER, "RightShoulderSet"},
                {EquipmentSlot.ES_LEFTGAUNTLET, "LeftGauntletSet"},
                {EquipmentSlot.ES_RIGHTGAUNTLET, "RightGauntletSet"},
                {EquipmentSlot.ES_CHESTARMOUR, "ChestSet"},
                {EquipmentSlot.ES_BELT, "BeltSet"},
                {EquipmentSlot.ES_LEFTTHIGH, "LeftThighSet"},
                {EquipmentSlot.ES_RIGHTTHIGH, "RightThighSet"},
                {EquipmentSlot.ES_LEFTSHIN, "LeftShinSet"},
                {EquipmentSlot.ES_RIGHTSHIN, "RightShinSet"},
                {EquipmentSlot.ES_MELEEWEAPON, "MainWeaponSet"},
                {EquipmentSlot.ES_NECKLACE, "MainSheathSet"},
                {EquipmentSlot.ES_SHIELD, "OffhandWeaponSet"},
                {EquipmentSlot.ES_RANGEDWEAPON, "OffhandSheathSet"},
                {EquipmentSlot.ES_HELMETDETAIL, "HairSet"}
            };

            foreach (KeyValuePair<EquipmentSlot, string> setDef in setDefinitions)
            {
                AppearanceSets.AppearanceSet newSet = new AppearanceSets.AppearanceSet(setDef.Key);
                SBProperty setProp;
                if (wrappedObject.sbObject.Properties.TryGetValue(setDef.Value, out setProp))
                {
                    int counter = 0;
                    foreach (SBProperty appProp in setProp.Array.Values)
                    {
                        AppearanceSets.SetItem newSetItem = new AppearanceSets.SetItem(counter, null, appProp.Value.Replace("\0", string.Empty));
                        newSet.items.Add(newSetItem);
                        counter++;
                    }
                    appearanceAsset.sets.Add(newSet);
                }
            }
            List<Item> items = new List<Item>(Resources.LoadAll<Item>("Items/Equipment"));
            foreach (AppearanceSets.AppearanceSet appSet in appearanceAsset.sets)
            {
                foreach (AppearanceSets.SetItem setItem in appSet.items)
                {
                    string[] searchSplit = setItem.temporaryAppearanceName.Split('.');
                    if (searchSplit.Length > 1)
                    {
                        foreach (Item it in items)
                        {
                            string itRace = it.name.Substring(0, it.name.IndexOf('_'));
                            string itName = it.name.Replace(itRace, string.Empty).TrimStart('_');
                            if (searchSplit[0].Contains(itRace) && searchSplit[searchSplit.Length - 1].Equals(itName, StringComparison.OrdinalIgnoreCase))
                            {
                                setItem.item = it;
                                break;
                            }
                        }
                    }
                }
            }
            Log("Finished adding sets", Color.green);
            */
        }
    }
}