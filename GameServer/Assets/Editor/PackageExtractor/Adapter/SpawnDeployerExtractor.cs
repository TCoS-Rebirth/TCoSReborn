using System.Collections.Generic;
using System.IO;
using Common;
using Common.UnrealTypes;
using Database.Static;
using Gameplay;
using Gameplay.Entities.NPCs;
using UnityEditor;
using UnityEngine;
using Utility;
using World;
using World.Paths;

namespace PackageExtractor.Adapter
{
    public class SpawnDeployerExtractor : ExtractorAdapter
    {
        public Zone targetZone;

        public override string Name
        {
            get { return "Spawn Deployer Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract spawn deployers from a map file and place them in the provided zone"; }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Target zone");
            targetZone = EditorGUILayout.ObjectField(targetZone, typeof (Zone), true) as Zone;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (targetZone == null)
            {
                Log("Select a zone first", Color.yellow);
                return;
            }
            var spawnersHolder = targetZone.transform.Find("Spawners");
            if (spawnersHolder == null)
            {
                Log("SpawnersHolder not found", Color.yellow);
                return;
            }
            foreach (var sd in spawnersHolder.GetComponentsInChildren<SpawnDeployer>())
            {
                Object.DestroyImmediate(sd.gameObject);
            }

            var groupClassDataPath = "Assets/GameData/NPCs/GroupClasses.asset";

            var groupClassData = ScriptableObject.CreateInstance<NPCGroupClassCollection>();
            groupClassData = AssetDatabase.LoadAssetAtPath<NPCGroupClassCollection>(groupClassDataPath);

            var factionData = new List<Taxonomy>();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Taxonomies/");
            foreach (var f in files)
            {
                var t = AssetDatabase.LoadAssetAtPath<Taxonomy>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (t != null)
                {
                    factionData.Add(t);
                }
            }

            var npcTypeData = new List<NPC_Type>();
            files = Directory.GetFiles(Application.dataPath + "/GameData/NPCs/");
            foreach (var f in files)
            {
                var n = AssetDatabase.LoadAssetAtPath<NPC_Type>("Assets" + f.Replace(Application.dataPath, string.Empty));
                var c = AssetDatabase.LoadAssetAtPath<NPCCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (n)
                {
                    npcTypeData.Add(n);
                }
                if (c)
                {
                    foreach (var t in c.types)
                    {
                        npcTypeData.Add(t);
                    }
                }
            }


            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("Spawn_Deployer"))
                {
                    var go = new GameObject(wpo.sbObject.Name);
                    var deployer = go.AddComponent<SpawnDeployer>();
                    go.transform.parent = spawnersHolder;

                    //Grab properties
                    //Find bosses
                    List<SBResource> sbrBosses;
                    if (ReadObjectArray(wpo, "Bosses", resources, out sbrBosses))
                    {
                        foreach (var b in sbrBosses)
                        {
                            foreach (var nt in npcTypeData)
                            {
                                if (nt.resourceID == b.ID)
                                {
                                    deployer.bosses.Add(nt);
                                    break;
                                }
                            }
                        }
                    }


                    //Find group class
                    string classGroup;
                    ReadString(wpo, "ClassGroup", out classGroup);


                    var foundGcType = false;
                    foreach (var gcType in groupClassData.groupTypes)
                    {
                        if (classGroup.EndsWith(gcType.className))
                        {
                            deployer.npcGroupClass = gcType;
                            foundGcType = true;
                            break;
                        }
                    }
                    if (!foundGcType)
                    {
                        Log("SpawnDeployerExtractor : failed to match a NPCGroupClass_Type", Color.red);
                        return;
                    }

                    //Faction
                    SBResource sbrTx;
                    ReadObject(wpo, "Faction", resources, out sbrTx);
                    var foundTxType = false;
                    foreach (var tx in factionData)
                    {
                        if (tx.ID == sbrTx.ID)
                        {
                            deployer.Faction = tx;
                            foundTxType = true;
                            break;
                        }
                    }
                    if (!foundTxType)
                    {
                        Log("SpawnDeployerExtractor : failed to match a Taxonomy", Color.red);
                        return;
                    }

                    //Min, max level (int)
                    ReadInt(wpo, "MinLevel", out deployer.minLevel);
                    ReadInt(wpo, "MaxLevel", out deployer.maxLevel);

                    //Spawn details
                    //Respawn timeout, variation, distance (float)
                    ReadFloat(wpo, "RespawnTimeout", out deployer.respawnTimeout);
                    ReadFloat(wpo, "RespawnVariation", out deployer.respawnVariation);
                    ReadFloat(wpo, "MaxSpawnDistance", out deployer.maxSpawnDistance);
                    deployer.maxSpawnDistance = deployer.maxSpawnDistance*UnitConversion.UnrUnitsToMeters;

                    //Chase, visual, threat, LoS range (float)
                    ReadFloat(wpo, "ChaseRange", out deployer.chaseRange);
                    ReadFloat(wpo, "VisualRange", out deployer.visualRange);
                    ReadFloat(wpo, "ThreatRange", out deployer.threatRange);
                    ReadFloat(wpo, "LineOfSightRange", out deployer.losRange);

                    //LoS spawning, spawnImmediately, triggeredSpawn (bool)
                    ReadBool(wpo, "LoSSpawning", out deployer.LoSSpawning);
                    ReadBool(wpo, "SpawnImmediatly", out deployer.spawnImmediately);
                    ReadBool(wpo, "TriggeredSpawn", out deployer.triggeredSpawn);

                    //Other
                    //Script references
                    var scriptsProp = wpo.FindProperty("Scripts");
                    if (scriptsProp != null)
                    {
                        foreach (var script in scriptsProp.IterateInnerProperties())
                        {
                            deployer.referenceLinkedScripts.Add(script.GetValue<string>());
                            if (deployer.referenceLinkedScripts[deployer.referenceLinkedScripts.Count - 1].Contains("Patrol"))
                            {
                                var patrolObject =
                                    extractorWindowRef.ActiveWrapper.FindObjectWrapper(deployer.referenceLinkedScripts[deployer.referenceLinkedScripts.Count - 1]);
                                if (patrolObject != null)
                                {
                                    string ppName;
                                    ReadString(patrolObject, "StartPoint", out ppName);

                                    //Link patrol point to spawner
                                    var WPsHolder = targetZone.transform.Find("Waypoints");
                                    foreach (Transform wpObj in WPsHolder)
                                    {
                                        if (wpObj.name == ppName)
                                        {
                                            deployer.linkedPatrolPoint = wpObj.GetComponent<PatrolPoint>();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Zone
                    deployer.zone = targetZone;

                    //Location
                    Vector3 loc;
                    if (ReadVector3(wpo, "Location", out loc))
                    {
                        go.transform.position = UnitConversion.ToUnity(loc);
                    }
                    else
                    {
                        //TODO: Implement adding of no-location deployers
                        Object.DestroyImmediate(go);
                    }


                    //Rotation
                    Rotator r;
                    if (ReadRotator(wpo, "Rotation", out r))
                    {
                        go.transform.rotation = UnitConversion.ToUnity(r);
                    }
                }
            }
        }
    }
}