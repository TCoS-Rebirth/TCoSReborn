
using Database.Static;
using UnityEngine;
using World;
using Gameplay.Quests;
using Gameplay.Quests.QuestTargets;
using System.Collections.Generic;
using Gameplay.Entities.NPCs;
using Gameplay.RequirementSpecifier;
using Utility;
using System.IO;
using UnityEditor;
using Gameplay.Events;

namespace PackageExtractor.Adapter
{
    class MapScriptExtractor : ExtractorAdapter
    {
        Zone targetZone;
        List<QuestCollection> questCols = new List<QuestCollection>();
        List<NPCCollection> npcCols = new List<NPCCollection>();

        public override string Description
        {
            get
            {
                return "Tries to extract and link up various zone script requirements. Currently only implements Conditional_Enemy. Run after all other map data extractors";
            }
        }

        public override string Name
        {
            get
            {
                return "Map package Scripts Extractor";
            }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Target zone");
            targetZone = EditorGUILayout.ObjectField(targetZone, typeof(Zone), true) as Zone;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            var pW = extractorWindowRef.ActiveWrapper;

            if (targetZone == null)
            {
                Log("Select a zone first", Color.yellow);
                return;
            }

            loadCols();

            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {

                if (wpo.sbObject.ClassName.EndsWith("Conditional_Enemy"))
                {
                    if (!linkConditionalEnemy(wpo, resources, pW))
                    {
                        Log("Failed to fully link up" + wpo.Name, Color.red);
                    }
                }

                #region Triggers
                else if (wpo.sbObject.ClassName.EndsWith("Content_Trigger"))
                {
                    
                    if(!getContentTrigger(wpo, resources, pW))
                    {
                        Log("Failed to extract ContentTrigger" + wpo.Name, Color.red);
                    }

                }
                #endregion
            }
        }

        protected bool linkConditionalEnemy(WrappedPackageObject wpo, SBResources resources, PackageWrapper pW)
        {

            var reqs = new List<Content_Requirement>();
            //Get requirements
            foreach(var prop in wpo.sbObject.IterateProperties())
            {
                if (prop.Name == "Requirements")
                {
                    foreach(var reqProp in prop.IterateInnerProperties())
                    {
                        var reqWPO = FindReferencedObject(reqProp);
                        reqs.Add(getReq(reqWPO, resources, pW, null));
                    }
                }
            }

            var relatedTargets = new List<QuestTarget>();

            //Get lists of targets referenced in requirements            
            foreach (var req in reqs)
            {
               var refTars = getTarsFromReq(req);
               relatedTargets.AddRange(refTars);
            }

            var refNpcs = new List<NPC_Type>();

            //Resolve NPCs referenced by quest targets
            foreach (var relTar in relatedTargets)
            {
                refNpcs.AddRange(getNPCsFromTarget(relTar));
            }

            //Find spawner in zone's spawnerholder referencing each NPC
            
            var spawnersObj = targetZone.transform.Find("Spawners");
            var zoneSpawners = spawnersObj.GetComponentsInChildren<NpcSpawner>();
            var spawnerRefs = new List<NpcSpawner>();
            foreach (var refNpc in refNpcs)
            {
                //handles multiple matches, if that arises?
                foreach (var spawner in zoneSpawners)
                {
                    if (refNpc.resourceID == spawner.npc.resourceID)
                    {
                        spawnerRefs.Add(spawner);
                        break;
                    }
                }               
            }

            if (spawnerRefs.Count == 0) return false;

            foreach (var spawnerRef in spawnerRefs)
            {

                //Attach ConditionalEnemy component to spawner transform
                var ceRef = spawnerRef.gameObject.GetComponent<ConditionalEnemy>();
                if (!ceRef)
                    ceRef = spawnerRef.gameObject.AddComponent<ConditionalEnemy>();
                ceRef.owningZone = targetZone;
                ceRef.Requirements = reqs;
                ceRef.AttachedSpawner = spawnerRef;

                Vector3 location;
                Vector3 rotation;
                //If SBO has location, rotation, and transform doesn't already have them set, set them
                if (ReadVector3(wpo, "Location", out location))
                {
                    if (spawnerRef.transform.position == new Vector3(0.0f,0.0f,0.0f))
                        spawnerRef.transform.position = UnitConversion.ToUnity(location);
                }
                if (ReadVector3(wpo, "Rotation", out rotation))
                {
                    if (spawnerRef.transform.rotation == new Quaternion())
                        spawnerRef.transform.rotation = Quaternion.Euler(rotation);
                }

                spawnerRef.triggeredSpawn = true;
            }

            return true;                     
        }


        protected bool getContentTrigger(WrappedPackageObject wpo, SBResources resources, PackageWrapper pW)
        {
            var triggersHolder = targetZone.transform.FindChild("Triggers");
            if (!triggersHolder) return false;
            for (var i = triggersHolder.childCount; i-- > 0;)
            {
                var child = triggersHolder.GetChild(i);
                if (child.GetComponent<Trigger>() != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            var go = new GameObject(wpo.Name.Replace("\0", string.Empty));
            var trigger = go.AddComponent<Trigger>();
            go.transform.parent = triggersHolder;

            //Get requirements
            trigger.Requirements = new List<Content_Requirement>();
            foreach (var prop in wpo.sbObject.IterateProperties())
            {
                if (prop.Name == "Requirements")
                {
                    foreach (var reqProp in prop.IterateInnerProperties())
                    {
                        var reqWPO = FindReferencedObject(reqProp);
                        trigger.Requirements.Add(getReq(reqWPO, resources, pW, null));
                    }
                }
            }

            //Actions
            trigger.Actions = new List<Content_Event>();
            foreach (var prop in wpo.sbObject.IterateProperties())
            {
                if (prop.Name == "Actions")
                {
                    foreach (var eventProp in prop.IterateInnerProperties())
                    {
                        var evWPO = FindReferencedObject(eventProp);
                        trigger.Actions.Add(ExtractEvent(evWPO, resources, pW, null));
                    }
                }
            }

            //Coll geometry (pos, radius, height)
            Vector3 pos;
            ReadVector3(wpo, "ColLocation", out pos);
            trigger.transform.position = pos;
            ReadFloat(wpo, "CollisionRadius", out trigger.Radius);
            ReadFloat(wpo, "CollisionHeight", out trigger.Height);

            //Tag
            ReadString(wpo, "Tag", out trigger.Tag);

            return true;
        }

        #region Helpers
        protected bool loadCols()
        {
            //Load NPC collections
            var files = Directory.GetFiles(Application.dataPath + "/GameData/NPCs/");
            foreach (var f in files)
            {
                var nc = AssetDatabase.LoadAssetAtPath<NPCCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (nc != null)
                {
                    npcCols.Add(nc);
                }
            }
            if (npcCols.Count == 0) return false;
            //Quests
            files = Directory.GetFiles(Application.dataPath + "/GameData/Quests/");
            foreach (var f in files)
            {
                var qc = AssetDatabase.LoadAssetAtPath<QuestCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (qc != null)
                {
                    questCols.Add(qc);
                }
            }
            if (questCols.Count == 0) return false;

            return true;
        }

        protected List<QuestTarget> getTarsFromReq(Content_Requirement req)
        {
            var output = new List<QuestTarget>();

            //Recurse for requirements nested in conditionals
            if (req as Req_And != null)
            {
                var ra = req as Req_And;
                foreach(var condReq in ra.Requirements)
                {
                    output.AddRange(getTarsFromReq(condReq));
                }
            }

            else if (req as Req_Or != null)
            {
                var ro = req as Req_Or;
                foreach (var condReq in ro.Requirements)
                {
                    output.AddRange(getTarsFromReq(condReq));
                }
            }


            else if (req as Req_QuestActive != null)
            {
                var rqa = req as Req_QuestActive;
                var quest = getQuest(rqa.RequiredQuest);
                output.AddRange(quest.targets);
            }

            else if (req as Req_TargetActive != null)
            {
                var rta = req as Req_TargetActive;
                var target = getTarget(rta.quest, rta.objective);
                output.Add(target);
            }

            return output;
        }

        protected List<NPC_Type> getNPCsFromTarget(QuestTarget tar)
        {
            var output = new List<NPC_Type>();

            //Handle any quest target types which reference enemy NPCTypes
            if (tar as QT_BeDefeated != null)
            {
                var qtbd = tar as QT_BeDefeated;
                foreach (var npcRes in qtbd.NpcsNamedTargetIDs)
                {
                    output.Add(getNPC(npcRes));
                }
            }
            else if (tar as QT_Defeat != null)
            {
                var qtt = tar as QT_Defeat;
                output.Add(getNPC(qtt.NpcTargetID));
            }
            else if (tar as QT_Destroy != null)
            {
                var qtd = tar as QT_Destroy;
                output.Add(qtd.Target);
            }
            else if (tar as QT_Hunt != null)
            {
                var qth = tar as QT_Hunt;
                output.Add(getNPC(qth.NpcTargetID));
            }
            else if (tar as QT_Kill != null)
            {
                var qtk = tar as QT_Kill;
                foreach (var npcRes in qtk.NpcTargetIDs)
                {
                    output.Add(getNPC(npcRes));
                }
            }
            else if (tar as QT_Gather != null)
            {
                var qtg = tar as QT_Gather;
                foreach (var npcRes in qtg.NpcsNamedDropperIDs)
                {
                    output.Add(getNPC(npcRes));
                }
            }

            //TODO: Possibly revisit other types to handle
            //QC_Protect, but that involves friendlies
            //QC_Stealth - again, unlikely

            return output;
        }

        protected NPC_Type getNPC(SBResource res)
        {
            foreach(var npcCol in npcCols)
            {
                foreach(var nt in npcCol.types)
                {
                    if (nt.resourceID == res.ID) return nt;
                }
            }
            return null;
        }

        protected Quest_Type getQuest(SBResource res)
        {
            foreach (var questCol in questCols)
            {
                foreach (var quest in questCol.looseQuests)
                {
                    if (quest.resourceID == res.ID) return quest;
                }

                foreach (var questChain in questCol.questChains)
                {
                    foreach (var quest in questChain.quests)
                    {
                        if (quest.resourceID == res.ID) return quest;
                    }
                }
            }
            return null;
        }

        protected QuestTarget getTarget(SBResource questRes, int tarIndex)
        {
            var quest = getQuest(questRes);
            var targets = quest.targets;
            if (tarIndex >= targets.Count) return null;
            return targets[tarIndex];
        }
        #endregion
    }
}
