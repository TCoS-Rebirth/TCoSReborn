using System.Collections.Generic;
using System.IO;
using Common;
using Database.Static;
using Gameplay;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;
using Gameplay.Items;
using Gameplay.Quests;
using Gameplay.Quests.QuestTargets;
using Gameplay.RequirementSpecifier;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class QuestExtractor : ExtractorAdapter
    {
        ConvCollection convCol = ScriptableObject.CreateInstance<ConvCollection>();
        List<Taxonomy> factionData = new List<Taxonomy>();

        string gameDataPath = "Assets/GameData/";
        List<ItemCollection> itemCols = new List<ItemCollection>();
        SBLocalizedStrings locStrings = ScriptableObject.CreateInstance<SBLocalizedStrings>();
        List<NPCCollection> npcCols = new List<NPCCollection>();
        QuestCollection questCol = ScriptableObject.CreateInstance<QuestCollection>();
        SBResources resourcesProp = ScriptableObject.CreateInstance<SBResources>();

        public override string Name
        {
            get { return "Quest Extractor"; }
        }

        public override string Description
        {
            get { return "Extracts quest information from a quest package file, and tries to link up references. Run BEFORE Conversation Extractor!"; }
        }


        public override void DrawGUI(Rect r)
        {
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            //Log("Disabled, unused", Color.red);
            //return;

            //Package name
            var pW = extractorWindowRef.ActiveWrapper;
            var saveName = pW.Name;

            //Load Taxonomies
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Taxonomies/");
            foreach (var f in files)
            {
                var t = AssetDatabase.LoadAssetAtPath<Taxonomy>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (t != null)
                {
                    factionData.Add(t);
                }
            }

            //Load NPC collections

            files = Directory.GetFiles(Application.dataPath + "/GameData/NPCs/");
            foreach (var f in files)
            {
                var nc = AssetDatabase.LoadAssetAtPath<NPCCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (nc != null)
                {
                    npcCols.Add(nc);
                }
            }

            //Load item collections

            files = Directory.GetFiles(Application.dataPath + "/GameData/Items/");
            foreach (var f in files)
            {
                var ic = AssetDatabase.LoadAssetAtPath<ItemCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (ic != null)
                {
                    itemCols.Add(ic);
                }
            }

            //Load localised strings
            locStrings = AssetDatabase.LoadAssetAtPath<SBLocalizedStrings>("Assets/GameData/SBResources/SBLocalizedStrings.asset");

            //Load resources
            resourcesProp = resources;

            questCol = ScriptableObject.CreateInstance<QuestCollection>();
            //Create new assets
            AssetDatabase.CreateAsset(questCol, gameDataPath + "Quests/" + saveName + ".asset");
            AssetDatabase.CreateAsset(convCol, gameDataPath + "Conversations/" + saveName + ".asset");

            //Populate quest collection with quest chain skeletons first (name, localized name ID, quest area)
            populateChains();

            //Iterate quests

            foreach (var wpo in pW.IterateObjects())
            {
                var objClass = wpo.sbObject.ClassName.Replace("\0", string.Empty);

                //TODO: possibly handle non-Quest_Standard quests?
                if (objClass.EndsWith("Quest_Standard"))
                {
                    extractQuest(wpo, questCol, extractorWindowRef.ActiveWrapper);
                }
            }

            //Save collections                
            EditorUtility.SetDirty(questCol);
            EditorUtility.SetDirty(convCol);
        }

        void populateChains()
        {
            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                var objClass = wpo.sbObject.ClassName.Replace("\0", string.Empty);

                if (objClass.EndsWith("Quest_Chain"))
                {
                    var curQC = new QuestChain();
                    curQC.internalName = wpo.sbObject.Name;

                    ReadLocalizedString(wpo, "Name", locStrings, out curQC.localizedName);

                    int qA;
                    ReadInt(wpo, "QuestArea", out qA);
                    curQC.questArea = (EQuestArea) qA;
                    questCol.questChains.Add(curQC);
                }
            }
        }

        void extractQuest(WrappedPackageObject qo, QuestCollection qC, PackageWrapper pW)
        {
            var curQ = new Quest_Type();

            //populate Quest class properties
            curQ.internalName = qo.Name;
            curQ.resourceID = resourcesProp.GetResourceID(extractorWindowRef.ActiveWrapper.Name + "." + qo.Name);

            //Quest area
            int qA;
            ReadInt(qo, "QuestArea", out qA);
            curQ.questArea = (EQuestArea) qA;

            //Localized name string
            ReadLocalizedString(qo, "Name", locStrings, out curQ.nameLocStr);

            //Summary string
            ReadLocalizedString(qo, "Summary", locStrings, out curQ.summaryLocStr);

            //Tag
            ReadString(qo, "Tag", out curQ.tag);

            //Note
            ReadString(qo, "Note", out curQ.note);

            //Level
            ReadInt(qo, "Level", out curQ.level);

            //Deliver by mail quest
            ReadBool(qo, "DeliverByMail", out curQ.deliverByMail);

            //Provider, Finisher, Conversation topics
            if (!curQ.deliverByMail)
            {
                ReadObject(qo, "Provider", resourcesProp, out curQ.provider);
                ReadObject(qo, "Finisher", resourcesProp, out curQ.finisher);

                SBProperty provideProp, midProp, finishProp;
                provideProp = qo.FindProperty("ProvideTopic");
                midProp = qo.FindProperty("MidTopic");
                finishProp = qo.FindProperty("FinishTopic");

                WrappedPackageObject provideWPO, midWPO, finishWPO;
                if (provideProp != null)
                {
                    provideWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(provideProp.GetValue<string>());
                    curQ.provideCT = getConvTopicRef(provideWPO, resourcesProp, pW.Name);

                    //Add topic to quest provider
                    addTopicToNPC(curQ.provideCT, curQ.provider);

                    //Add full topic to conversations database 
                    //ConversationTopic pTopic = getConvTopicFull(provideWPO, resourcesProp, locStrings, pW);
                    //convCol.topics.Add(pTopic);
                    //AssetDatabase.AddObjectToAsset(pTopic, convCol);

                    //Debug.Log("QuestExtractor : " + curQ.provideCT.internalName + " has " +
                    //    curQ.provideCT.allNodes.Count + " nodes and " +
                    //    curQ.provideCT.allResponses.Count + " responses");
                }

                if (midProp != null)
                {
                    midWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(midProp.GetValue<string>());
                    curQ.midCT = getConvTopicRef(midWPO, resourcesProp, pW.Name);

                    //Add topic to quest provider
                    addTopicToNPC(curQ.midCT, curQ.provider);

                    //Add full topic to conversations database 
                    //ConversationTopic mTopic = getConvTopicFull(midWPO, resourcesProp, locStrings, pW);
                    //convCol.topics.Add(mTopic);
                    //AssetDatabase.AddObjectToAsset(mTopic, convCol);

                    //Debug.Log("QuestExtractor : " + curQ.midCT.internalName + " has " +
                    //    curQ.midCT.allNodes.Count + " nodes and " +
                    //    curQ.midCT.allResponses.Count + " responses");
                }

                if (finishProp != null)
                {
                    finishWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(finishProp.GetValue<string>());
                    curQ.finishCT = getConvTopicRef(finishWPO, resourcesProp, pW.Name);

                    //Add topic to quest finisher
                    addTopicToNPC(curQ.finishCT, curQ.finisher);

                    //Add full topic to conversations database 
                    //ConversationTopic fTopic = getConvTopicFull(finishWPO, resourcesProp, locStrings, pW);
                    //convCol.topics.Add(fTopic);
                    //AssetDatabase.AddObjectToAsset(fTopic, convCol);


                    //Debug.Log("QuestExtractor : " + curQ.finishCT.internalName + " has " +
                    //   curQ.finishCT.allNodes.Count + " nodes and " +
                    //    curQ.finishCT.allResponses.Count + " responses");
                }
            }

            //Requirements
            //For each requirement in requirements property
            var requirements = qo.FindProperty("Requirements");
            if (requirements != null)
            {
                curQ.requirements = new List<Content_Requirement>();
                foreach (var req in requirements.IterateInnerProperties())
                {
                    //Get each requirement WPO
                    var reqWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(req.GetValue<string>());
                    //populate requirement object and add to requirements
                    var newCR = ExtractRequirement(reqWPO, resourcesProp);
                    curQ.requirements.Add(newCR);
                    newCR.name = qo.Name + "." + reqWPO.Name;
                    AssetDatabase.AddObjectToAsset(newCR, qC);
                }
            }

            //Prequests
            //For each prop in Prequests array
            var preQuests = qo.FindProperty("Prequests");
            if (preQuests != null)
            {
                curQ.preQuests = new List<SBResource>();
                foreach (var preQuest in preQuests.IterateInnerProperties())
                {
                    //Get quest resource
                    //Add it to prequests
                    curQ.preQuests.Add(resourcesProp.GetResource(extractorWindowRef.ActiveWrapper.Name, preQuest.Value));
                }
            }


            //Targets
            var targets = qo.FindProperty("Targets");
            if (targets != null)
            {
                curQ.targets = new List<QuestTarget>();
                foreach (var target in targets.IterateInnerProperties())
                {
                    //Get each requirement WPO
                    var tarWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(target.GetValue<string>());
                    //Debug.Log("Working on target WPO " + tarWPO.Name);

                    //populate requirement object and add to requirements
                    var newQT = extractQuestTarget(tarWPO, locStrings, pW.Name);
                    if (newQT != null)
                    {
                        //Assign asset a name
                        newQT.name = tarWPO.sbObject.Package + "." + tarWPO.sbObject.Name;

                        //Add to targets and asset DB
                        curQ.targets.Add(newQT);
                        AssetDatabase.AddObjectToAsset(newQT, qC);
                    }
                }
            }


            //Rewards
            var rewards = qo.FindProperty("Rewards");
            if (rewards != null)
            {
                foreach (var reward in rewards.IterateInnerProperties())
                {
                    //Get each reward WPO
                    var rewardWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(reward.GetValue<string>());

                    //Switch reward type

                    switch (rewardWPO.sbObject.ClassName.Replace("\0", string.Empty).Replace("SBGamePlay.", string.Empty))
                    {
                        //Money
                        case "QR_Money":
                            ReadInt(rewardWPO, "Money", out curQ.money);
                            break;

                        //Quest points
                        case "QR_QuestPoints":
                            curQ.questPoints = new QuestPoints();
                            ReadInt(rewardWPO, "QP", out curQ.questPoints.QP);
                            ReadInt(rewardWPO, "QPFrac", out curQ.questPoints.QPFrac);
                            break;

                        //Item(s)
                        case "QR_Item":
                            curQ.rewardItems = getRewardItems(rewardWPO);
                            break;
                    }
                }
            }

            //If quest is in a chain, add to the appropriate chain
            string qcInternalName;
            ReadString(qo, "QuestChain", out qcInternalName);
            var curQC = getChain(qcInternalName);

            if (curQC != null)
            {
                foreach (var colQCh in questCol.questChains)
                {
                    if (colQCh.internalName == curQC.internalName)
                    {
                        colQCh.quests.Add(curQ);
                        return;
                    }
                }
            }
            else
            {
                //Otherwise add to the looseQuests container
                questCol.looseQuests.Add(curQ);
            }
        }


        Content_Inventory getRewardItems(WrappedPackageObject rewardWPO)
        {
            var output = new Content_Inventory();
            var CIProp = rewardWPO.FindProperty("RewardItems");
            var contentInvWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(CIProp.GetValue<string>());

            //For each item in contentInventory
            foreach (var itemProp in contentInvWPO.FindProperty("Items").IterateInnerProperties())
            {
                var item = new Content_Inventory.ContentItem();

                //Link item reference
                var itemResProp = itemProp.GetInnerProperty("Item");

                //Match collection name
                foreach (var ic in itemCols)
                {
                    if (itemResProp.Value.StartsWith(ic.name))
                    {
                        //Retrieve Item_Type
                        foreach (var it in ic.items)
                        {
                            if (itemResProp.Value.Contains(it.internalName))
                            {
                                //populate Item_Type and ID
                                item.Item = it;
                                item.itemID = it.resourceID;
                                break;
                            }
                        }
                        break;
                    }
                }

                if (item.Item != null)
                {
                }

                //Populate stack size and colours
                item.StackSize = itemProp.GetInnerProperty("StackSize").GetValue<int>();
                item.Color1 = itemProp.GetInnerProperty("Color1").GetValue<byte>();
                item.Color2 = itemProp.GetInnerProperty("Color2").GetValue<byte>();

                //Add to output
                output.Items.Add(item);
            }
            return output;
        }

        void addTopicToNPC(SBResource topic, SBResource npcRes)
        {
            if (npcRes == null)
            {
                //Debug.Log("QuestExtractor.addTopicToNPC : invalid NPC_Type resource, aborting");
                return;
            }
            if (npcRes.ID == 0)
            {
                //Debug.Log("QuestExtractor.addTopicToNPC : npcRes.ID == 0, aborting");
                return;
            }

            //Debug.Log("Attempting to add topic " + topic.internalName + " to " + npcRes.Name);

            foreach (var nc in npcCols)
            {
                foreach (var nt in nc.types)
                {
                    if (nt.resourceID == npcRes.ID)
                    {
                        var alteredNT = nt;

                        if (alteredNT.QuestTopics == null)
                        {
                            alteredNT.QuestTopics = new List<SBResource>();
                        }

                        //Duplicate avoidance check
                        if (!alteredNT.QuestTopics.Contains(topic))
                            alteredNT.QuestTopics.Add(topic);

                        //AssetDatabase.AddObjectToAsset(topic, nt);
                        EditorUtility.CopySerialized(alteredNT, nt);
                        EditorUtility.SetDirty(nt);
                        return;
                    }
                }
            }
        }

        public QuestChain getChain(string name)
        {
            foreach (var qc in questCol.questChains)
            {
                if (qc.internalName == name)
                    return qc;
            }
            return null;
        }

        #region QuestTargets

        QuestTarget extractQuestTarget(WrappedPackageObject tarWPO, SBLocalizedStrings locStrings, string pwName)
        {
            QuestTarget qtObj;
            switch (tarWPO.sbObject.ClassName.Replace("\0", string.Empty).Replace("SBGamePlay.", string.Empty))
            {
                //TODO: Handle all QuestTarget & QuestCondition subclasses
                case "QT_Talk":
                    //Debug.Log("Handling QT_Talk quest target");
                    qtObj = getQTTalk(tarWPO, locStrings, questCol, pwName);
                    break;

                default:
                    return null;
            }

            //Add base QuestTarget class properties to qtObj

            //TODO: public List<SBResource> Pretargets;

            //public bool AlwaysVisible;
            ReadBool(tarWPO, "AlwaysVisible", out qtObj.AlwaysVisible);

            //TODO: public List<Content_Event> CompleteEvents;            

            //public SBLocalizedString Description;
            ReadLocalizedString(tarWPO, "Description", locStrings, out qtObj.Description);

            return qtObj;
        }

        QT_Talk getQTTalk(WrappedPackageObject tarWPO, SBLocalizedStrings locStrings, QuestCollection questCol, string pwName)
        {
            var qtTalk = ScriptableObject.CreateInstance<QT_Talk>();

            //Topic
            WrappedPackageObject topicWPO;
            var topic = tarWPO.FindProperty("Topic");
            if (topic != null)
            {
                topicWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(topic.GetValue<string>());
                if (topicWPO == null)
                {
                    //Debug.Log("topicWPO is null"); 
                }
                qtTalk.Topic = getConvTopicRef(topicWPO, resourcesProp, pwName);
                //ConversationTopic fullTopic = getConvTopicFull(topicWPO, resourcesProp, locStrings, extractorWindowRef.ActiveWrapper);
                //AssetDatabase.AddObjectToAsset(fullTopic, convCol);
                //Debug.Log("Got topic " + tarWPO.sbObject.Package + "." + topicWPO.Name);
            }

            //Person
            SBResource person;
            if (ReadObject(tarWPO, "Person", resourcesProp, out person))
            {
                foreach (var nc in npcCols)
                {
                    if (person.Name.StartsWith(nc.name))
                    {
                        foreach (var nt in nc.types)
                        {
                            if (person.ID == nt.resourceID)
                            {
                                qtTalk.Person = nt;
                            }
                        }
                    }
                }
                //AssetDatabase.AddObjectToAsset(qtTalk.Person, questCol);
            }
            return qtTalk;
        }

        //TODO: Implement QuestTarget subclass get methods              

        #endregion
    }
}