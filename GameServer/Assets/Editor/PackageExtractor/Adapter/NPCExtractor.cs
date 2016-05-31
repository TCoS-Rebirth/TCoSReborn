using System;
using System.Collections.Generic;
using Common;
using Database.Static;
using Gameplay;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;
using Gameplay.Skills;
using Gameplay.Skills.Effects;
using UnityEditor;
using UnityEngine;
using Gameplay.Loot;
using System.IO;

namespace PackageExtractor.Adapter
{
    public class NPCExtractor : ExtractorAdapter
    {
        List<SkillEffectCollection> avEffectCollections = new List<SkillEffectCollection>();

        //ConvCollection conversationsGP;

        List<NPC_Type> extractedNPCs = new List<NPC_Type>();

        List<LootTableCollection> ltCols;

        string path = "";

        List<PackageWrapper> refPackages = new List<PackageWrapper>();

        public override string Name
        {
            get { return "NPC Extractor"; }
        }

        public override string Description
        {
            get
            {
                return
                    "Tries to extract the NPC_Types. Steps: 1 - load references into stash, 2- click button below, 3 - load all NPC packages into stash, execute after last stashed package (load a random one for that)";
            }
        }

        public override void DrawGUI(Rect r)
        {
            if (GUILayout.Button("Load Stash as StatReferences"))
            {
                refPackages.AddRange(extractorWindowRef.StashedPackages);
                extractorWindowRef.StashedPackages.Clear();
            }
            for (var i = refPackages.Count; i-- > 0;)
            {
                if (GUILayout.Button(refPackages[i].Name))
                {
                    refPackages.RemoveAt(i);
                }
            }
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (extractorWindowRef.StashedPackages.Count == 0 || refPackages.Count == 0)
            {
                Log("All steps followed?", Color.yellow);
                return;
            }

            //Loot table cols
            ltCols = new List<LootTableCollection>();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/LootTables/");
            foreach (var f in files)
            {
                var ltc = AssetDatabase.LoadAssetAtPath<LootTableCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (ltc != null)
                {
                    ltCols.Add(ltc);
                }
            }

            path = "Assets/GameData/";
            //conversationsGP = AssetDatabase.LoadAssetAtPath<ConvCollection>("Assets/GameData/Conversations/ConversationsGP.asset");
            foreach (var pw in extractorWindowRef.StashedPackages)
            {
                WalkStashStack(pw, resources, localizedStrings);
            }
        }

        void WalkStashStack(PackageWrapper p, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            extractedNPCs.Clear();

            //Loading and resetting existing collection maintains asset links
            /*
            var collection = AssetDatabase.LoadAssetAtPath<NPCCollection>(path + "NPCs/" + p.Name + ".asset");
            if (!collection)
            {
                AssetDatabase.CreateAsset(collection, path + "NPCs/" + p.Name + ".asset");
            }
            */

            var collection = ScriptableObject.CreateInstance<NPCCollection>();
            AssetDatabase.CreateAsset(collection, path + "NPCs/" + p.Name + ".asset");

            foreach (var wpo in p.IterateObjects())
            {
                foreach (NPC_Type.ConsolidatedNPCType type in Enum.GetValues(typeof (NPC_Type.ConsolidatedNPCType)))
                {
                    if (!wpo.sbObject.ClassName.Replace("\0", string.Empty).Equals("SBGamePlay." + type) &&
                        !wpo.sbObject.ClassName.Replace("\0", string.Empty).Equals("SBGame." + type))
                    {
                        continue;
                    }
                    Log("Extracting NPC " + wpo.sbObject.Name, Color.yellow);

                    var thisNPC = ExtractNPCType(p, wpo, type, resources, localizedStrings);
                    collection.types.Add(thisNPC);
                    AssetDatabase.AddObjectToAsset(thisNPC, collection);
                    if (thisNPC.Stats != null)
                    {
                        thisNPC.Stats.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                        AssetDatabase.AddObjectToAsset(thisNPC.Stats, thisNPC);
                        EditorUtility.SetDirty(thisNPC);
                    }

                    Log("Extracting NPC " + thisNPC.name + ", saving", Color.green);
                }
            }

            /*
            foreach (NPC_Type npc in extractedNPCs)
            {
                AssetDatabase.AddObjectToAsset(npc, collection);
                collection.types.Add(npc);
                if (npc.Stats != null)
                {
                    npc.Stats.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(npc.Stats, npc);
                    EditorUtility.SetDirty(npc);
                }
            }
            */

            EditorUtility.SetDirty(collection);
        }

        SkillDeck FindSkillDeck(string deckname)
        {
            var parts = deckname.Split('.');
            return AssetDatabase.LoadAssetAtPath<SkillDeck>("Assets/GameData/Decks/" + parts[0] + "/" + parts[parts.Length - 1] + ".asset");
        }

        NPC_StatTable FindNPCStats(PackageWrapper active, string npcname)
        {
            WrappedPackageObject requestedObject = null;
            for (var i = 0; i < refPackages.Count; i++)
            {
                requestedObject = refPackages[i].FindObjectWrapper(npcname);
                if (requestedObject != null)
                {
                    break;
                }
            }
            if (requestedObject == null)
            {
                requestedObject = active.FindObjectWrapper(npcname);
            }
            if (requestedObject == null)
            {
                Log("Stats not found! -" + npcname + "(stash the needed package)", Color.yellow);
                return null;
            }
            switch (requestedObject.sbObject.ClassName.Replace("\0", string.Empty))
            {
                case "SBGamePlay.NS_Rotator":
                    var rot = ScriptableObject.CreateInstance<NS_Rotator>();
                    ReadInt(requestedObject, "BasePoints", out rot.BasePoints);
                    ReadInt(requestedObject, "LevelPerPoints", out rot.LevelPerPoints);
                    ReadInt(requestedObject, "PointsMultiplier", out rot.PointsMultiplier);
                    ReadByteArray(requestedObject, "Rotation", out rot.Rotation);
                    ReadInt(requestedObject, "DefaultBody", out rot.DefaultBody);
                    ReadInt(requestedObject, "DefaultMind", out rot.DefaultMind);
                    ReadInt(requestedObject, "DefaultFocus", out rot.DefaultFocus);
                    ReadInt(requestedObject, "Hitpoints", out rot.Hitpoints);
                    ReadInt(requestedObject, "HpPerLevel", out rot.HpPerLevel);
                    return rot;
                case "SBGamePlay.NS_Fixed":
                    var fix = ScriptableObject.CreateInstance<NS_Fixed>();
                    ReadInt(requestedObject, "BasePoints", out fix.BasePoints);
                    ReadInt(requestedObject, "LevelPerPoints", out fix.LevelPerPoints);
                    ReadInt(requestedObject, "PointsMultiplier", out fix.PointsMultiplier);
                    ReadInt(requestedObject, "Body", out fix.Body);
                    ReadInt(requestedObject, "Mind", out fix.Mind);
                    ReadInt(requestedObject, "Focus", out fix.Focus);
                    ReadInt(requestedObject, "Hitpoints", out fix.Hitpoints);
                    return fix;
                case "SBGamePlay.NPC_StatsTable": //needed?
                    var table = ScriptableObject.CreateInstance<NPC_StatTable>();
                    ReadInt(requestedObject, "BasePoints", out table.BasePoints);
                    ReadInt(requestedObject, "LevelPerPoints", out table.LevelPerPoints);
                    ReadInt(requestedObject, "PointsMultiplier", out table.PointsMultiplier);
                    return table;
            }
            return null;
        }

        AudioVisualSkillEffect FindEffect(string name)
        {
            if (avEffectCollections.Count == 0)
            {
                var collections = Resources.LoadAll<SkillEffectCollection>("Skills/Effects");
                foreach (var c in collections)
                {
                    if (c.name.Contains("AVGP"))
                    {
                        avEffectCollections.Add(c);
                    }
                }
            }
            foreach (var sec in avEffectCollections)
            {
                var se = sec.GetEffect(name) as AudioVisualSkillEffect;
                if (se != null)
                {
                    return se;
                }
            }
            return null;
        }

        Taxonomy FindTaxonomy(string name)
        {
            return GameData.Get.factionDB.GetFaction(name);
        }

        NPC_Type ExtractNPCType(PackageWrapper pW, WrappedPackageObject wpo, NPC_Type.ConsolidatedNPCType type, SBResources resources, SBLocalizedStrings strings)
        {
            var npc = ScriptableObject.CreateInstance<NPC_Type>();
            npc.name = wpo.Name.Replace("\0", string.Empty);
            var searchString = string.Format("{0}.{1}.{2}", pW.Name, wpo.sbObject.Package, wpo.Name).Replace("\0", string.Empty).Replace("null.", string.Empty);
            var r = resources.GetResource(searchString);
            if (r != null)
            {
                npc.resourceID = r.ID;
            }
            else
            {
                Log("ResourceID not found for: " + wpo.Name, Color.cyan);
            }
            npc.internalType = type;
            List<byte> cTypes;
            if (ReadByteArray(wpo, "ClassTypes", out cTypes))
            {
                foreach (var b in cTypes)
                {
                    npc.ClassTypes.Add((ENPCClassType) b);
                }
            }
            ReadInt(wpo, "ClassRank", out npc.ClassRank);
            ReadLocalizedString(wpo, "LongName", strings, out npc.LongName);
            ReadLocalizedString(wpo, "ShortName", strings, out npc.ShortName);
            ReadString(wpo, "Note", out npc.Note);
            ReadEnum(wpo, "Category", out npc.Category);
            ReadFloat(wpo, "CorpseTimeout", out npc.CorpseTimeout);
            ReadBool(wpo, "Invulnerable", out npc.Invulnerable);
            ReadEnum(wpo, "NPCClassClassification", out npc.NPCClassClassification);
            string skillDeckName;
            if (ReadString(wpo, "SkillDeck", out skillDeckName, false))
            {
                npc.SkillDeck = FindSkillDeck(skillDeckName);
            }
            else
            {
                Log("Error parsing Skilldeck from: " + wpo.Name, Color.yellow);
            }
            ReadString(wpo, "AttackSheet", out npc.temporaryAttackSheetName);
            ReadInt(wpo, "FameLevel", out npc.FameLevel);
            ReadInt(wpo, "PePRank", out npc.PePRank);
            ReadFloat(wpo, "CreditMultiplier", out npc.CreditMultiplier);
            string statsName;
            if (ReadString(wpo, "Stats", out statsName, true))
            {
                npc.Stats = FindNPCStats(pW, statsName);
                if (npc.Stats != null)
                {
                    npc.Stats.name = statsName;
                }
            }
            ReadByte(wpo, "Movement", out npc.Movement);
            ReadFloat(wpo, "AccelRate", out npc.AccelRate);
            ReadFloat(wpo, "JumpSpeed", out npc.JumpSpeed);
            ReadFloat(wpo, "GroundSpeed", out npc.GroundSpeed);
            ReadFloat(wpo, "CombatSpeed", out npc.CombatSpeed);
            ReadFloat(wpo, "StrollSpeed", out npc.StrollSpeed);
            ReadFloat(wpo, "WaterSpeed", out npc.WaterSpeed);
            ReadFloat(wpo, "AirSpeed", out npc.AirSpeed);
            ReadFloat(wpo, "AirControl", out npc.AirControl);
            ReadFloat(wpo, "AirMinSpeed", out npc.AirMinSpeed);
            ReadFloat(wpo, "ClimbSpeed", out npc.ClimbSpeed);
            ReadFloat(wpo, "TurnSpeed", out npc.TurnSpeed);
            ReadFloat(wpo, "TerminalVelocity", out npc.TerminalVelocity);
            ReadBool(wpo, "CanStrafe", out npc.CanStrafe);
            ReadBool(wpo, "CanRest", out npc.CanRest);
            ReadBool(wpo, "CanWalkBackwards", out npc.CanWalkBackwards);
            ReadInt(wpo, "BossPriority", out npc.BossPriority);
            var effectProperty = wpo.FindProperty("Effects");
            if (effectProperty != null)
            {
                foreach (var effect in effectProperty.IterateInnerProperties())
                {
                    var found = FindEffect(effect.Value.Replace("\0", string.Empty));
                    if (found != null)
                    {
                        npc.Effects.Add(found);
                    }
                }
            }
            ReadString(wpo, "Equipment", out npc.temporaryEquipmentName);

            #region Loot
            var lootProp = wpo.FindProperty("Loot");
            if (lootProp != null)
            {
                npc.Loot = new List<LootTable>();
                foreach (var lp in lootProp.Array.Values)
                {
                    var lt = getLootTable(lp.Value);
                    if (lt != null)
                    {
                        npc.Loot.Add(lt);
                    }
                }
            }
            #endregion

            ReadBool(wpo, "IndividualKillCredit", out npc.IndividualKillCredit);

            #region ConversationTopics 
            //Polymo: TODO: some are still not correctly extracted (more than needed etc)

            //get list of regular topics            
            var topics = wpo.FindProperty("Topics");
            if (topics != null)
            {
                if (npc.Topics == null)
                {
                    npc.Topics = new List<SBResource>();
                }

                foreach (var topic in topics.IterateInnerProperties())
                {
                    //Get each topicWPO


                    //TODO : Handle conversations contained in ConversationsGP
                    //Log info for now
                    if (topic.GetValue<string>().Contains("ConversationsGP"))
                    {
                        Log("ConversationsGP topic referenced by " + npc.name + " in package " + pW.Name, Color.yellow);
                        continue;
                    }

                        /*
                        if (topic.GetValue<string>().Contains("ConversationsGP"))
                        {
                            //Find in ConversationsGP                                                
                            foreach (var ct in conversationsGP.topics)
                            {
                                //TODO: Look into getresource not returning valid topic resource in this instance
                                if (ct.resource.ID == resources.GetResource(topic.GetValue<string>()).ID)
                                {
                                    //Add ref to this NPC
                                    npc.Topics.Add(ct.resource);
                                }
                            }
                        }
                        //end ConversationsGP
                        */


                    var topicWPO = pW.FindObjectWrapper(topic.GetValue<string>());
                    //ConversationTopic newTopic;
                    SBResource newTopicRef;

                    if (topicWPO != null)
                    {
                        //Debug.Log("Extracting topic " + topicWPO.Name + " for NPC " + npc.name);
                        //newTopic = getConvTopicFull(topicWPO, resources, strings, pW);
                        newTopicRef = getConvTopicRef(topicWPO, resources, pW.Name);
                        //if (newTopic != null)
                        //{
                        npc.Topics.Add(newTopicRef);

                        //}
                    }
                }
            }

            //The Topics array doesn't contain all topics (e.g. CT_Greeting types), so check the NPC wrapper children for these...
            //foreach(WrappedPackageObject child in wpo.ChildObjects) {
            foreach (var CTSearchWPO in pW.IterateObjects())
            {
                var ctObj = CTSearchWPO.sbObject;

                var ctFullName = pW.Name + "." + ctObj.Package + "." + ctObj.Name;
                var wpoFullName = pW.Name + "." + wpo.sbObject.Package + "." + wpo.sbObject.Name;

                if (ctObj.ClassName.Contains("CT_")
                    //&& CTObj.Package.Contains(wpo.Name))
                    //&& (resources.GetResourceID(fullName))
                    && (ctFullName.Contains(wpoFullName)))
                {
                    Debug.Log("Found ConversationTopic WPO outside Topics array...");

                    //Flag denotes whether topic is a quest topic or not
                    var isQuestTopic = false;

                    if (ctObj.ClassName.Contains("Quest"))
                    {
                        isQuestTopic = true;
                    }
                    
                    var newTopicRef = resources.GetResource(ctFullName);


                    if (newTopicRef == null)
                    {
                        Log("...couldn't find CT resource for " + ctObj.Package + "." + ctObj.Name + ", skipping", Color.yellow);
                        break;
                    }


                    //SBResource newTopicRef = getConvTopicRef(CTSearchWPO, resources, pW.Name);
                    Log("...newTopicRef.Name = " + newTopicRef.Name + "...", Color.yellow);

                    //Check that the topic isn't added already
                    var dupTopic = false;
                    if (npc.Topics != null)
                    {
                        foreach (var topic in npc.Topics)
                        {
                            if (newTopicRef.ID == topic.ID)
                            {
                                dupTopic = true;
                                Debug.Log("... CT was a duplicate of a normal topic.");
                                break;
                            }
                        }
                    }
                    else
                    {
                        npc.Topics = new List<SBResource>();
                    }

                    if (npc.QuestTopics != null)
                    {
                        foreach (var qTopic in npc.QuestTopics)
                        {
                            if (newTopicRef.ID == qTopic.ID)
                            {
                                dupTopic = true;
                                Debug.Log("... CT was a duplicate of a quest topic.");
                                break;
                            }
                        }
                    }
                    else
                    {
                        npc.QuestTopics = new List<SBResource>();
                    }

                    if (!dupTopic)
                    {
                        if (isQuestTopic)
                            npc.QuestTopics.Add(newTopicRef);
                        else
                            npc.Topics.Add(newTopicRef);

                        Debug.Log("... adding to topics!");
                    }
                }
            }

            #endregion

            ReadBool(wpo, "TriggersKilledHook", out npc.TriggersKilledHook);
            string taxonomyName;
            if (ReadString(wpo, "TaxonomyFaction", out taxonomyName,
                !(type == NPC_Type.ConsolidatedNPCType.NPC_MonsterClass || type == NPC_Type.ConsolidatedNPCType.NPC_Monster)))
            {
                npc.TaxonomyFaction = FindTaxonomy(taxonomyName);
                if (npc.TaxonomyFaction == null)
                {
                    npc.TaxonomyFaction = GameData.Get.factionDB.defaultFaction;
                }
            } else
            {
                //Look for factions referencing this package wrapper as class package
                foreach(var faction in GameData.Get.factionDB.Factions)
                {
                    //Try to match faction name with current sbObj package name
                    if (faction.name == wpo.sbObject.Package)
                    {
                        npc.TaxonomyFaction = faction;
                        break;
                    }
                }

                /*
                //Try to match by class package again, but  with any faction with parent Taxonomy
                if (npc.TaxonomyFaction == null)
                {
                    foreach (var faction in GameData.Get.factionDB.Factions)
                    {
                        if (faction.classPackage && faction.classPackage.name == pW.Name)
                        {
                            if (faction.parent.ID == 77462)
                            {
                                npc.TaxonomyFaction = faction;
                                break;
                            }
                        }
                    }
                }
                */

            }

            ReadBool(wpo, "Travel", out npc.Travel);
            ReadBool(wpo, "Arena", out npc.Arena);
            ReadString(wpo, "Shop", out npc.temporaryShopBaseName);
            return npc;
        }

        LootTable getLootTable(string ltRef)
        {
            char[] splitter = new char[] { '.' };
            string[] parts = ltRef.Split(splitter, 2);
            if (parts.Length < 2) return null;

            //Find col with name matching parts[0]
            foreach (var ltCol in ltCols)
            {
                if (ltCol.name == parts[0])
                {
                    //Find LT with ref matching parts[1]
                    foreach (var lt in ltCol.tables)
                    {
                        if (lt.Reference == parts[1])
                        {
                            return lt;
                        }
                    }
                    return null;
                }
            }
            return null;
        }
    }
}