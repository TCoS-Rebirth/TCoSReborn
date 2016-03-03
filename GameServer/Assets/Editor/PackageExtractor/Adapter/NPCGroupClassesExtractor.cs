using Common;
using Database.Static;
using Gameplay.Entities.NPCs;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class NPCGroupClassesExtractor : ExtractorAdapter
    {
        NPCGroupClassCollection gcc = new NPCGroupClassCollection();

        public override string Name
        {
            get { return "ENPC Group Class Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract ENPC group classes into a collection"; }
        }

        public override bool IsCompatible(PackageWrapper p)
        {
            return base.IsCompatible(p) && p.Name.Contains("NPCgroups");
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            gcc = ScriptableObject.CreateInstance<NPCGroupClassCollection>();
            AssetDatabase.CreateAsset(gcc, "Assets/GameData/NPCs/GroupClasses.asset");

            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                ExtractGroupClass(wpo);
            }
            EditorUtility.SetDirty(gcc);
        }

        NPCGroupClass_Type ExtractGroupClass(WrappedPackageObject wpo)
        {
            if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("Package"))
            {
                return null;
            }

            var groupClass = ScriptableObject.CreateInstance<NPCGroupClass_Type>();
            groupClass.name = groupClass.className = wpo.Name;


            if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("Boss"))
            {
                groupClass.isBoss = true;
                gcc.groupTypes.Add(groupClass);
                AssetDatabase.AddObjectToAsset(groupClass, gcc);
                return groupClass;
            }
            groupClass.isBoss = false;


            foreach (var units in wpo.sbObject.IterateProperties())
            {
                foreach (var unit in units.IterateInnerProperties())
                {
                    var newUnit = new NPCGroupClassUnit();

                    foreach (var var in unit.IterateInnerProperties())
                    {
                        switch (var.Name)
                        {
                            case "Minimum":
                                newUnit.min = var.GetValue<int>();
                                break;
                            case "Maximum":
                                newUnit.max = var.GetValue<int>();
                                break;

                            case "RequestedClassTypes":
                                foreach (var subVar in var.IterateInnerProperties())
                                {
                                    newUnit.ReqClassTypes.Add((ENPCClassType) subVar.GetValue<int>());
                                }
                                break;
                            case "ForbiddenClassTypes":
                                foreach (var subVar in var.IterateInnerProperties())
                                {
                                    newUnit.ForbidClassTypes.Add((ENPCClassType) subVar.GetValue<int>());
                                }
                                break;
                        }
                    }
                    groupClass.units.Add(newUnit);
                }
            }
            groupClass.name = wpo.sbObject.Name;
            gcc.groupTypes.Add(groupClass);
            AssetDatabase.AddObjectToAsset(groupClass, gcc);
            return groupClass;
        }
    }
}