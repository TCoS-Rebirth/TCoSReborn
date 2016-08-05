using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common;
using Gameplay;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;
using Gameplay.Items;
using Gameplay.Quests;
using Gameplay.Skills;
using Gameplay.Entities.Interactives;
using World;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Database.Static
{
    public sealed class GameData : MonoBehaviour
    {
        static GameData _instance;
        public ConvDB convDB = new ConvDB();
        public FactionTemplateDB factionDB = new FactionTemplateDB();
        public ItemTemplateDB itemDB = new ItemTemplateDB();
        public NpcDB npcDB = new NpcDB();
        public SkillTemplateDB skillDB = new SkillTemplateDB();
        public QuestDB questDB = new QuestDB();
        public LevelProgression levelProg;


        public static GameData Get
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameData>();
                }
                return _instance;
            }
        }

        void Awake()
        {
            _instance = this;
        }

        public void Initialize(Action<bool> onFinishedCallback)
        {
            StartCoroutine(InitializeAsync(onFinishedCallback));
        }

        public IEnumerator InitializeAsync(Action<bool> callback)
        {
            if (!skillDB.Initialize())
            {
                Debug.Log("error while loading skillDB");
                callback(false);
                yield break;
            }
            yield return null;
            if (!npcDB.Initialize())
            {
                Debug.Log("error while loading npcDB");
                callback(false);
                yield break;
            }
            yield return null;
            if (!factionDB.Initialize(npcDB.Collections))
            {
                Debug.Log("error while loading factionDB");
                callback(false);
                yield break;
            }
            yield return null;
            if (!itemDB.Initialize())
            {
                Debug.Log("error while loading itemDB");
                callback(false);
                yield break;
            }
            yield return null;
            if (!convDB.Initialize())
            {
                Debug.Log("error while loading conversationDB");
                callback(false);
                yield break;
            }
            yield return null;
            if (!questDB.Initialize())
            {
                Debug.Log("error while loading questDB");
                callback(false);
                yield break;
            }
            yield return null;
            if (levelProg == null)
            {
                Debug.Log("Error while loading level progression data");
                callback(false);
                yield break;
            }
            callback(true);
        }

        [Serializable]
        public class SkillTemplateDB
        {
            [SerializeField] List<SkillCollection> skillCollections = new List<SkillCollection>();

            public List<SkillCollection> Collections
            {
                get { return skillCollections; }
            }

            public bool Initialize()
            {
                return skillCollections.Count > 0;
            }

            public FSkill_Type GetSkill(int sID)
            {
                for (var i = 0; i < skillCollections.Count; i++)
                {
                    var skill = skillCollections[i].GetSkill(sID);
                    if (skill != null)
                    {
                        return skill;
                    }
                }
                return null;
            }

            public FSkill_Type GetSkill(string skillName)
            {
                for (var i = 0; i < skillCollections.Count; i++)
                {
                    var skill = skillCollections[i].FindSkill(skillName);
                    if (skill != null)
                    {
                        return skill;
                    }
                }
                return null;
            }
        }

        [Serializable]
        public class ItemTemplateDB
        {
            [SerializeField] AppearanceSets appearanceSets;
            [SerializeField] List<ItemCollection> itemCollections = new List<ItemCollection>();

            public List<ItemCollection> Collections
            {
                get { return itemCollections; }
            }

            public bool Initialize()
            {
                return itemCollections.Count > 0;
            }

            public int GetSetIndex(Game_Item it)
            {
                foreach (var set in appearanceSets.sets)
                {
                    foreach (var item in set.items)
                    {
                        if (item.itemID == it.Type.resourceID)
                        {
                            return item.index;
                        }
                    }
                }
                return 0;
            }

            public Game_Item GetSetItem(int setIndex, EquipmentSlot slot)
            {
                if (setIndex == 0)
                {
                    return null;
                }
                foreach (var set in appearanceSets.sets)
                {
                    if (set.slot != slot)
                    {
                        continue;
                    }
                    foreach (var item in set.items)
                    {
                        if (item.index == setIndex)
                        {
                            var it = GetItemType(item.itemID);
                            if (it != null)
                            {
                                var git = ScriptableObject.CreateInstance<Game_Item>();
                                git.Type = it;
                                return git;
                            }
                        }
                    }
                }
                return null;
            }

            public Item_Type GetItemType(int resourceID)
            {
                for (var i = 0; i < itemCollections.Count; i++)
                {
                    var it = itemCollections[i].GetItem(resourceID);
                    if (it != null)
                    {
                        return it;
                    }
                }
                return null;
            }
        }

        [Serializable]
        public class FactionTemplateDB
        {
            [SerializeField] public Taxonomy defaultFaction;

            [SerializeField] List<Taxonomy> factions;

            [SerializeField, ReadOnly] public LookupTables lookupTables;

            public List<Taxonomy> Factions
            {
                get { return factions; }
            }

            public bool Initialize(List<NPCCollection> npcs)
            {
                if (factions.Count == 0)
                {
                    Debug.Log("GameData : Taxonomies not assigned yet");
                }
                if (lookupTables == null)
                {
                    lookupTables = ScriptableObject.CreateInstance<LookupTables>();
                }
                return (factions.Count > 0) && lookupTables.Init(npcs, factions);
            }

            public void AddFaction(Taxonomy factionIn)
            {
                factions.Add(factionIn);
            }

            //public int getFacID(Taxonomy factionIn)
            //{
            //    foreach (var f in factions)
            //    {
            //        if (f == factionIn)
            //        {
            //            return f.ID;
            //        }
            //    }
            //    throw new Exception("GameData : couldn't get faction ID for faction " + factionIn.name);
            //}

            public Taxonomy GetFaction(int resID)
            {
                for (var i = 0; i < factions.Count; i++)
                {
                    if (factions[i].ID == resID)
                    {
                        return factions[i];
                    }
                }
                return defaultFaction;
            }

            public Taxonomy GetFaction(string factionName)
            {
                var parts = factionName.Split('.');
                factionName = parts[parts.Length - 1];
                for (var i = 0; i < factions.Count; i++)
                {
                    if (factions[i].name.Equals(factionName, StringComparison.OrdinalIgnoreCase))
                    {
                        return factions[i];
                    }
                }
                return null;
            }


            internal void Clear()
            {
                factions = new List<Taxonomy>();
                //descendantsTable = ScriptableObject.CreateInstance<IDListTable>();
                //factionMemsTable = ScriptableObject.CreateInstance<IDListTable>();
            }
        }

        [Serializable]
        public class NpcDB
        {
            [SerializeField] List<NPCCollection> npcCollections = new List<NPCCollection>();

            public List<NPCCollection> Collections
            {
                get { return npcCollections; }
            }

            public bool Initialize()
            {
                return npcCollections.Count > 0;
            }

            public NPC_Type GetNPCType(int resourceID)
            {
                for (var i = 0; i < npcCollections.Count; i++)
                {
                    var nt = npcCollections[i].GetItem(resourceID);
                    if (nt != null)
                    {
                        return nt;
                    }
                }
                throw new Exception("Failed to get NPC Type with resource ID " + resourceID);
            }


            public List<int> GetClassTypeMemberIDs(ENPCClassType classType)
            {
                var classTypeMems = new List<int>();
                for (var i = 0; i < npcCollections.Count; i++)
                {
                    foreach (var nt in npcCollections[0].types)
                    {
                        foreach (var ct in nt.ClassTypes)
                        {
                            if (ct == classType)
                            {
                                classTypeMems.Add(nt.resourceID);
                            }
                        }
                    }
                }
                Debug.Log("GameData : got " + classTypeMems.Count
                          + " members from NPC class type " + classType);
                return classTypeMems;
            }
        }

        [Serializable]
        public class ConvDB
        {
            [SerializeField] List<ConvCollection> convCollections = new List<ConvCollection>();

            public List<ConvCollection> ConvCollections
            {
                get { return convCollections; }
            }

            public bool Initialize()
            {
                return convCollections.Count > 0;
            }

            public ConversationTopic GetTopic(SBResource topicRes)
            {
                //Cycle collections
                for (var i = 0; i < convCollections.Count; i++)
                {
                    //If package accountName matches, search collection, otherwise skip
                    if (topicRes.Name.StartsWith(convCollections[i].name))
                    {
                        var ct = convCollections[i].GetTopic(topicRes.ID);
                        if (ct != null)
                        {
                            return ct;
                        }
                    }
                }
                Debug.Log("Failed to get ConversationTopic with resource ID " + topicRes.ID);
                return null;
            }

            public List<ConversationTopic> GetTopics(List<SBResource> topicResList)
            {
                var output = new List<ConversationTopic>();
                //Store topic collection names to search in
                var colNames = new List<string>();
                foreach (var ct in topicResList)
                {
                    //Split off the package accountName (into parts[0])
                    var parts = ct.Name.Split(new[] {'.'}, 2);
                    //Debug.Log("Database.GetTopics : Splitting off package accountName " + parts[0]);

                    if (!colNames.Contains(parts[0]))
                    {
                        colNames.Add(parts[0]);
                    }
                }

                //Cycle collections
                for (var i = 0; i < convCollections.Count; i++)
                {
                    //If package accountName flagged to be searched
                    if (colNames.Contains(convCollections[i].name))
                    {
                        //Cycle input topic resources
                        foreach (var topicRes in topicResList)
                        {
                            //If topic resource contains this collection's accountName
                            if (topicRes.Name.StartsWith(convCollections[i].name))
                            {
                                //Find the topic and add to output
                                var ct = convCollections[i].GetTopic(topicRes.ID);
                                if (ct != null)
                                {
                                    output.Add(ct);
                                }
                            }
                        }
                    }
                }
                return output;
            }

            internal void Clear()
            {
                convCollections = new List<ConvCollection>();
            }

            public void addCol(ConvCollection cc)
            {
                convCollections.Add(cc);
            }
        }

        [Serializable]
        public class QuestDB
        {
            [SerializeField]
            List<QuestCollection> questCollections = new List<QuestCollection>();

            public List<QuestCollection> QuestCollections
            {
                get { return questCollections; }
            }

            public bool Initialize()
            {
                return questCollections.Count > 0;
            }

            public void addCol(QuestCollection qC)
            {
                QuestCollections.Add(qC);
            }

            internal void Clear()
            {
                questCollections = new List<QuestCollection>();
            }

            public Quest_Type GetQuest(SBResource questRes)
            {
                //Determine quest collection
                foreach (var questCol in questCollections)
                {
                    if (questRes.Name.StartsWith(questCol.name))
                    {

                        //Search collection chains for resource ID match, return matching quest
                        foreach (var questChain in questCol.questChains)
                        {
                            foreach (var quest in questChain.quests)
                            {
                                if (quest.resourceID == questRes.ID)
                                    return quest;
                            }
                        }

                        //Search loose quests for resID match, return match
                        foreach (var quest in questCol.looseQuests)
                        {
                            if (quest.resourceID == questRes.ID)
                                return quest;
                        }
                    }
                }

                //Could not find
                Debug.Log("GameData.QuestDB.GetQuest : Couldn't find quest by resource");
                return null;

            }

            public Quest_Type GetQuest(int questID)
            {
                //Determine quest collection
                foreach (var questCol in questCollections)
                {

                    //Search collection chains for resource ID match, return matching quest
                    foreach (var questChain in questCol.questChains)
                    {
                        foreach (var quest in questChain.quests)
                        {
                            if (quest.resourceID == questID)
                                return quest;
                        }
                    }

                    //Search loose quests for resID match, return match
                    foreach (var quest in questCol.looseQuests)
                    {
                        if (quest.resourceID == questID)
                            return quest;
                    }
                }
                return null;
            }

            public Quest_Type GetQuestFromContained(SBResource res)
            {
                //Determine quest collection
                foreach (var questCol in questCollections)
                {
                    if (res.Name.StartsWith(questCol.name))
                    {
                        string[] resNameParts = res.Name.Split('.');

                        //resNameParts[1] will refer to the quest object
                        string questName = resNameParts[1];

                        //Search collection chains for resource ID match, return matching quest
                        foreach (var questChain in questCol.questChains)
                        {
                            foreach (var quest in questChain.quests)
                            {
                                if (quest.internalName == questName)
                                    return quest;
                            }
                        }

                        //Search loose quests for resID match, return match
                        foreach (var quest in questCol.looseQuests)
                        {
                            if (quest.internalName == questName)
                                return quest;
                        }

                    }
                }

                //Could not find
                Debug.Log("GameData.QuestDB.GetQuestFromContained : Couldn't find quest by contained resource");
                return null;
            }

        }

#if UNITY_EDITOR
        [ContextMenu("Load Items")]
        void LoadItemCollections()
        {
            itemDB.Collections.Clear();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Items/");
            foreach (var f in files)
            {
                var t =
                    AssetDatabase.LoadAssetAtPath<ItemCollection>("Assets" +
                                                                  f.Replace(Application.dataPath, string.Empty));
                if (t != null)
                {
                    itemDB.Collections.Add(t);
                }
            }
        }

        [ContextMenu("Load Skills")]
        void LoadSkillCollections()
        {
            skillDB.Collections.Clear();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Skills/");
            foreach (var f in files)
            {
                var t =
                    AssetDatabase.LoadAssetAtPath<SkillCollection>("Assets" +
                                                                   f.Replace(Application.dataPath, string.Empty));
                if (t != null)
                {
                    skillDB.Collections.Add(t);
                }
            }
        }

        [ContextMenu("Load NPCs")]
        void LoadNPCs()
        {
            npcDB.Collections.Clear();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/NPCs/");
            foreach (var f in files)
            {
                var n =
                    AssetDatabase.LoadAssetAtPath<NPCCollection>("Assets" +
                                                                 f.Replace(Application.dataPath, string.Empty));
                if (n != null)
                {
                    npcDB.Collections.Add(n);
                }
            }
        }

        [ContextMenu("Load Factions")]
        void LoadFactions()
        {
            factionDB.Clear();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Taxonomies/");
            foreach (var f in files)
            {
                var t = AssetDatabase.LoadAssetAtPath<Taxonomy>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (t != null)
                {
                    factionDB.AddFaction(t);
                }
            }
        }

        [ContextMenu("Load Conversations")]
        void LoadConvs()
        {
            convDB.Clear();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Conversations/");
            foreach (var f in files)
            {
                var cc =
                    AssetDatabase.LoadAssetAtPath<ConvCollection>("Assets" +
                                                                  f.Replace(Application.dataPath, string.Empty));
                if (cc != null)
                {
                    convDB.addCol(cc);
                }
            }
        }

        [ContextMenu("Load Quests")]
        void LoadQuests()
        {
            questDB.Clear();
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Quests/");
            foreach (var f in files)
            {
                var qc =
                    AssetDatabase.LoadAssetAtPath<QuestCollection>("Assets" +
                                                                  f.Replace(Application.dataPath, string.Empty));
                if (qc != null)
                {
                    questDB.addCol(qc);
                }
            }
        }

        [ContextMenu("Initialize LookupTables")]
        void InitLookupTables()
        {
            if (factionDB.lookupTables == null)
            {
                Debug.Log("No lookuptable assigned to initialize");
                return;
            }
            factionDB.lookupTables.Init(npcDB.Collections, factionDB.Factions);
        }

#endif
    }
}