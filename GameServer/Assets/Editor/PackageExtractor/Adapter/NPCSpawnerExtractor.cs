using Common.UnrealTypes;
using Database.Static;
using Gameplay.Entities.NPCs;
using UnityEditor;
using UnityEngine;
using Utility;
using World;
using World.Paths;

namespace PackageExtractor.Adapter
{
    public class NPCSpawnerExtractor : ExtractorAdapter
    {
        public Zone targetZone;

        public override string Name
        {
            get { return "NPC Spawn Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract and place the NPC and Wildlife Spawns in the specified zone"; }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Target zone");
            targetZone = EditorGUILayout.ObjectField(targetZone, typeof (Zone), true) as Zone;
        }

        NPC_Type FindNPCType(string fullName)
        {
            var parts = fullName.Split('.');
            if (parts.Length > 1)
            {
                var col = AssetDatabase.LoadAssetAtPath<NPCCollection>("Assets/GameData/NPCs/" + parts[0] + ".asset");
                if (col != null)
                {
                    for (var i = 0; i < col.types.Count; i++)
                    {
                        if (col.types[i].name.Equals(parts[parts.Length - 1]))
                        {
                            return col.types[i];
                        }
                    }
                }
            }
            return null;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (targetZone == null)
            {
                Log("Select a zone first", Color.yellow);
                return;
            }
            var npcHolder = targetZone.transform.FindChild("Spawners");
            for (var i = npcHolder.childCount; i-- > 0;)
            {
                var child = npcHolder.GetChild(i);
                if (child.GetComponent<NpcSpawner>() != null)
                {
                    if (child.GetComponent<NpcSpawner>().GetType() != typeof (WildlifeSpawner))
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }
            }
            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("SBGamePlay.Spawn_NPC"))
                {
                    var go = new GameObject(wpo.Name.Replace("\0", string.Empty));
                    var npc = go.AddComponent<NpcSpawner>();
                    string npcTypeName;
                    if (ReadString(wpo, "NPCType", out npcTypeName))
                    {
                        npc.npc = FindNPCType(npcTypeName);
                    }
                    if (npc.npc == null)
                    {
                        Log(wpo.Name + ": NPC_Type not found", Color.red);
                        Object.DestroyImmediate(go);
                        continue;
                    }

                    //GameObject naming
                    //If NPC has 'null' ShortName, name game object by NPC_Type asset's name
                    if (npc.npc.ShortName == "[No Text]")
                    {
                        go.name = string.Format("{0} ({1})", go.name, npc.npc.name);
                    }
                    //Otherwise name by its ShortName
                    else
                    {
                        go.name = string.Format("{0} ({1})", go.name, npc.npc.ShortName);
                    }

                    ReadFloat(wpo, "RespawnTimeout", out npc.respawnTimeout);
                    ReadBool(wpo, "TriggeredSpawn", out npc.triggeredSpawn);
                    ReadBool(wpo, "TriggeredRespawn", out npc.triggeredRespawn);
                    ReadBool(wpo, "TriggeredDespawn", out npc.triggeredDespawn);
                    ReadString(wpo, "EventOnWiped", out npc.referenceEventOnWipedName);
                    ReadString(wpo, "EventOnSpawn", out npc.referenceEventOnSpawnName);
                    ReadString(wpo, "StateMachine", out npc.referenceAiStatemachineName);
                    var scriptsProp = wpo.FindProperty("Scripts");
                    if (scriptsProp != null)
                    {
                        foreach (var script in scriptsProp.IterateInnerProperties())
                        {
                            npc.referenceScriptNames.Add(script.GetValue<string>());
                            if (npc.referenceScriptNames[npc.referenceScriptNames.Count - 1].Contains("Patrol"))
                            {
                                var patrolObject = extractorWindowRef.ActiveWrapper.FindObjectWrapper(npc.referenceScriptNames[npc.referenceScriptNames.Count - 1]);
                                if (patrolObject != null)
                                {
                                    var ppName = "";
                                    ReadString(patrolObject, "StartPoint", out ppName);

                                    //Link patrol point to spawner
                                    var WPsHolder = targetZone.transform.FindChild("Waypoints");
                                    foreach (Transform wpObj in WPsHolder)
                                    {
                                        if (wpObj.name == ppName)
                                        {
                                            npc.linkedPatrolPoint = wpObj.GetComponent<PatrolPoint>();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    ReadFloat(wpo, "AggressionFactor", out npc.aggressionFactor);
                    ReadFloat(wpo, "FearFactor", out npc.fearFactor);
                    ReadFloat(wpo, "SocialFactor", out npc.socialFactor);
                    ReadFloat(wpo, "BoredomFactor", out npc.boredomFactor);
                    var groupRefProp = wpo.FindProperty("ActorGroups");
                    if (groupRefProp != null)
                    {
                        foreach (var groupProp in groupRefProp.IterateInnerProperties())
                        {
                            var groupObject = extractorWindowRef.ActiveWrapper.FindObjectWrapper(groupProp.GetValue<string>());
                            if (groupObject != null)
                            {
                                string groupDescription;
                                if (ReadString(groupObject, "Description", out groupDescription))
                                {
                                    npc.groups.Add(groupDescription.Replace("\0", string.Empty));
                                }
                            }
                        }
                    }
                    go.transform.parent = npcHolder;
                    Vector3 loc;
                    if (ReadVector3(wpo, "Location", out loc))
                    {
                        loc = UnitConversion.ToUnity(loc);
                        go.transform.parent = npcHolder;
                        go.transform.position = loc;
                    }
                    Rotator r;
                    if (ReadRotator(wpo, "Rotation", out r))
                    {
                        go.transform.rotation = UnitConversion.ToUnity(r);
                    }
                }
                else if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("SBGamePlay.Spawn_Wildlife"))
                {
                    var go = new GameObject(wpo.Name.Replace("\0", string.Empty));
                    var npc = go.AddComponent<WildlifeSpawner>();
                    string npcTypeName;
                    if (ReadString(wpo, "NPCType", out npcTypeName))
                    {
                        npc.npc = FindNPCType(npcTypeName);
                    }
                    if (npc.npc == null)
                    {
                        Log(wpo.Name + ": NPC_Type not found", Color.red);
                        Object.DestroyImmediate(go);
                        continue;
                    }

                    //GameObject naming
                    //If NPC has 'null' ShortName, name game object by NPC_Type asset's name
                    if (npc.npc.ShortName == "[No Text]")
                    {
                        go.name = string.Format("{0} ({1})", go.name, npc.npc.name);
                    }
                    //Otherwise name by its ShortName
                    else
                    {
                        go.name = string.Format("{0} ({1})", go.name, npc.npc.ShortName);
                    }

                    ReadFloat(wpo, "RespawnTimeout", out npc.respawnTimeout);
                    ReadBool(wpo, "TriggeredSpawn", out npc.triggeredSpawn);
                    ReadBool(wpo, "TriggeredRespawn", out npc.triggeredRespawn);
                    ReadBool(wpo, "TriggeredDespawn", out npc.triggeredDespawn);
                    ReadString(wpo, "EventOnWiped", out npc.referenceEventOnWipedName);
                    ReadString(wpo, "EventOnSpawn", out npc.referenceEventOnSpawnName);
                    ReadString(wpo, "StateMachine", out npc.referenceAiStatemachineName);
                    var scriptsProp = wpo.FindProperty("Scripts");
                    if (scriptsProp != null)
                    {
                        foreach (var script in scriptsProp.IterateInnerProperties())
                        {
                            npc.referenceScriptNames.Add(script.GetValue<string>());
                        }
                    }
                    ReadFloat(wpo, "AggressionFactor", out npc.aggressionFactor);
                    ReadFloat(wpo, "FearFactor", out npc.fearFactor);
                    ReadFloat(wpo, "SocialFactor", out npc.socialFactor);
                    ReadFloat(wpo, "BoredomFactor", out npc.boredomFactor);
                    var groupRefProp = wpo.FindProperty("ActorGroups");
                    if (groupRefProp != null)
                    {
                        foreach (var groupProp in groupRefProp.IterateInnerProperties())
                        {
                            var groupObject = extractorWindowRef.ActiveWrapper.FindObjectWrapper(groupProp.GetValue<string>());
                            if (groupObject != null)
                            {
                                string groupDescription;
                                if (ReadString(groupObject, "Description", out groupDescription))
                                {
                                    npc.groups.Add(groupDescription.Replace("\0", string.Empty));
                                }
                            }
                        }
                    }
                    ReadInt(wpo, "LevelMin", out npc.LevelMin);
                    ReadInt(wpo, "LevelMax", out npc.LevelMax);
                    ReadInt(wpo, "SpawnMin", out npc.SpawnMin);
                    ReadInt(wpo, "SpawnMax", out npc.SpawnMax);
                    ReadBool(wpo, "LoSSpawning", out npc.LoSSpawning);
                    ReadFloat(wpo, "MaxSpawnDistance", out npc.MaxSpawnDistance);
                    npc.MaxSpawnDistance = UnitConversion.UnrUnitsToMeters*npc.MaxSpawnDistance;
                    ReadBool(wpo, "UseAbsoluteAmounts", out npc.UseAbsoluteAmounts);
                    ReadFloat(wpo, "RespawnTime", out npc.RespawnTime);
                    ReadFloat(wpo, "RespawnVariation", out npc.RespawnVariation);
                    ReadFloat(wpo, "VisualRange", out npc.VisualRange);
                    npc.VisualRange = UnitConversion.UnrUnitsToMeters*npc.VisualRange;
                    ReadFloat(wpo, "ThreatRange", out npc.ThreatRange);
                    npc.ThreatRange = npc.ThreatRange*UnitConversion.UnrUnitsToMeters;

                    go.transform.parent = npcHolder;
                    Vector3 loc;
                    if (ReadVector3(wpo, "Location", out loc))
                    {
                        loc = UnitConversion.ToUnity(loc);
                        go.transform.parent = npcHolder;
                        go.transform.position = loc;
                    }
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