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
using Gameplay.Events;

namespace PackageExtractor.Adapter
{
    public class QuestExtractor : ExtractorAdapter
    {
        ConvCollection convCol = ScriptableObject.CreateInstance<ConvCollection>();
        List<Taxonomy> factionData = new List<Taxonomy>();

        string gameDataPath = "Assets/GameData/";
        List<ItemCollection> itemCols = new List<ItemCollection>();
        List<NPCCollection> npcCols = new List<NPCCollection>();
        QuestCollection questCol = ScriptableObject.CreateInstance<QuestCollection>();

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

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings locStrings)
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

            questCol = ScriptableObject.CreateInstance<QuestCollection>();
            //Create new assets
            AssetDatabase.CreateAsset(questCol, gameDataPath + "Quests/" + saveName + ".asset");

            //TODO : Obliterates existing conversation data, look at improving this if possible
            AssetDatabase.CreateAsset(convCol, gameDataPath + "Conversations/" + saveName + ".asset");

            //Populate quest collection with quest chain skeletons first (name, localized name ID, quest area)
            populateChains(locStrings);

            //Iterate quests

            foreach (var wpo in pW.IterateObjects())
            {
                var objClass = wpo.sbObject.ClassName.Replace("\0", string.Empty);

                //TODO: possibly handle non-Quest_Standard quests?
                if (objClass.EndsWith("Quest_Standard"))
                {
                    extractQuest(wpo, extractorWindowRef.ActiveWrapper, resources, locStrings);
                }
            }

            //Save collections                
            EditorUtility.SetDirty(questCol);
            EditorUtility.SetDirty(convCol);
        }

        void populateChains(SBLocalizedStrings locStrings)
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
                    curQC.questArea = (EQuestArea)qA;
                    questCol.questChains.Add(curQC);
                }
            }
        }

        void extractQuest(WrappedPackageObject qo, PackageWrapper pW, SBResources resources, SBLocalizedStrings locStrings)
        {
            var curQ = new Quest_Type();

            //populate Quest class properties
            curQ.internalName = qo.Name;
            curQ.resourceID = resources.GetResourceID(extractorWindowRef.ActiveWrapper.Name + "." + qo.Name);

            //Quest area
            int qA;
            ReadInt(qo, "QuestArea", out qA);
            curQ.questArea = (EQuestArea)qA;

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
                ReadObject(qo, "Provider", resources, out curQ.provider);
                ReadObject(qo, "Finisher", resources, out curQ.finisher);

                SBProperty provideProp, midProp, finishProp;
                provideProp = qo.FindProperty("ProvideTopic");
                midProp = qo.FindProperty("MidTopic");
                finishProp = qo.FindProperty("FinishTopic");

                WrappedPackageObject provideWPO, midWPO, finishWPO;
                if (provideProp != null)
                {
                    provideWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(provideProp.GetValue<string>());
                    curQ.provideCT = getConvTopicRef(provideWPO, resources, pW.Name);

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
                    curQ.midCT = getConvTopicRef(midWPO, resources, pW.Name);

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
                    curQ.finishCT = getConvTopicRef(finishWPO, resources, pW.Name);

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
                    var newCR = getReq(reqWPO, resources, pW, questCol);
                    curQ.requirements.Add(newCR);
                    newCR.name = qo.Name + "." + reqWPO.Name;
                    AssetDatabase.AddObjectToAsset(newCR, questCol);
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
                    curQ.preQuests.Add(resources.GetResource(extractorWindowRef.ActiveWrapper.Name, preQuest.Value));
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
                    var newQT = extractQuestTarget(tarWPO, locStrings, resources, pW.Name, questCol, pW);
                    if (newQT != null)
                    {
                        //Assign asset a name
                        newQT.name = tarWPO.sbObject.Package + "." + tarWPO.sbObject.Name;

                        //Add to targets and asset DB
                        curQ.targets.Add(newQT);
                        AssetDatabase.AddObjectToAsset(newQT, questCol);
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
                foreach (var colQCh in this.questCol.questChains)
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
                this.questCol.looseQuests.Add(curQ);
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

        QuestTarget extractQuestTarget(WrappedPackageObject tarWPO, SBLocalizedStrings locStrings, SBResources resources, string pwName, Object assetObj, PackageWrapper pW)
        {
            QuestTarget output;
            switch (tarWPO.sbObject.ClassName.Replace("\0", string.Empty).Replace("SBGamePlay.", string.Empty))
            {
                //TODO: Handle all QuestTarget & QuestCondition subclasses

                case "QT_Take":
                    output = getQTTake(tarWPO, locStrings);
                    break;

                case "QT_Talk":
                    //Debug.Log("Handling QT_Talk quest target");
                    output = getQTTalk(tarWPO, questCol, resources, locStrings, pwName, assetObj);
                    break;



                case "QT_Hunt":
                    output = getQTHunt(tarWPO, resources);
                    break;

                
                case "QT_Deliver":
                    output = getQTDeliver(tarWPO, resources);
                    break;                
                
                case "QT_Fedex":
                    output = getQTFedex(tarWPO, resources);
                    break;
                /*
                case "QT_Kill":
                    output = getQTKill(tarWPO);
                    break;                                               

                case "QT_Exterminate":
                    output = getQTExterminate(tarWPO);
                    break;

                case "QT_Defeat":
                    output = getQTDefeat(tarWPO);
                    break;

                case "QT_Gather":
                    output = getQTGather(tarWPO);
                    break;

                case "QT_BeDefeated":
                    output = getQTBeDefeated(tarWPO);
                    break;

                case "QT_Challenge":
                    output = getQTChallenge(tarWPO);
                    break;

                case "QT_Escort":
                    output = getQTEscort(tarWPO);
                    break;                

                case "QT_Interactor":
                    output = getQTBeDefeated(tarWPO);
                    break;                

                case "QT_Place":
                    output = getQTPlace(tarWPO);
                    break;

                case "QT_Reach":
                    output = getQTReach(tarWPO);
                    break;                

                case "QT_Use":
                    output = getQTUse(tarWPO);
                    break;

                case "QT_UseAt":
                    output = getQTUseAt(tarWPO);
                    break;

                case "QT_UseOn":
                    output = getQTUseOn(tarWPO);
                    break;

                case "QT_Wait":
                    output = getQTWait(tarWPO);
                    break;

                case "QT_Subquest":
                    output = getQTSubquest(tarWPO);
                    break;
                
                //TODO:Remove one of these cases
                case "QT_SubQuest":
                    output = getQTSubquest(tarWPO);
                    break;
            */
                default:
                    return null;
            }

            //Add base QuestTarget class properties to qtObj

                    
            //Pretargets array
            output.Pretargets = new List<SBResource>();

            var pretargetsProp = tarWPO.FindProperty("Pretargets");

            //for each "Target" in "Pretargets"
            if (pretargetsProp != null)
            {
                foreach (var preTarget in pretargetsProp.IterateInnerProperties())
                {
                    //Find SBResource from value
                    //add output to Pretargets
                    output.Pretargets.Add(resources.GetResource(pwName + "." + preTarget.Value));

                }
            }

            //public bool AlwaysVisible;
            ReadBool(tarWPO, "AlwaysVisible", out output.AlwaysVisible);

            output.CompleteEvents = new List<Content_Event>();

            var completeEventsProp = tarWPO.FindProperty("CompleteEvents");

            //CompleteEvents array;  
            if (completeEventsProp != null)
            {
                foreach (var completeEventProp in completeEventsProp.IterateInnerProperties())
                {
                    //Find event WPO in wrapper
                    //ExtractEvent and add it to completeEvents
                    var eventWPO = pW.FindObjectWrapper(pW.Name + "." + completeEventProp.Value);
                    Content_Event thisEvent = ExtractEvent(eventWPO, resources, pW, assetObj);
                    thisEvent.name = eventWPO.sbObject.Package + "." + eventWPO.sbObject.Name;
                    output.CompleteEvents.Add(thisEvent);
                    AssetDatabase.AddObjectToAsset(thisEvent, assetObj);
                }
            }   

            //public SBLocalizedString Description;
            ReadLocalizedString(tarWPO, "Description", locStrings, out output.Description);

            return output;
        }

        //TODO: Implement all QuestTarget subclass get methods 

        QT_Talk getQTTalk(WrappedPackageObject tarWPO, QuestCollection questCol, SBResources resources, SBLocalizedStrings locStrings, string pwName, Object assetObj)
        {
            var qtTalk = ScriptableObject.CreateInstance<QT_Talk>();

            //Person
            SBResource person;
            if (ReadObject(tarWPO, "Person", resources, out person))
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
            }

            //Topic
            WrappedPackageObject topicWPO = findWPOFromObjProp(tarWPO, "Topic");
            if (topicWPO != null) {
            qtTalk.Topic = getConvTopicRef(topicWPO, resources, pwName);
            ConversationTopic fullTopic = getConvTopicFull(topicWPO, resources, locStrings, extractorWindowRef.ActiveWrapper);

            //Add topic ref to npc
            if ((qtTalk.Topic != null) && (qtTalk.Person != null))
                addTopicToNPC(qtTalk.Topic, person);

            //Debug.Log("Got topic " + tarWPO.sbObject.Package + "." + topicWPO.Name); 
            }           

            
            return qtTalk;
        }

        QT_Take getQTTake(WrappedPackageObject tarWPO, SBLocalizedStrings locStrings) {

            var qtTake = ScriptableObject.CreateInstance<QT_Take>();

            //get Content_Inventory class WPO where package contains WPO name
            WrappedPackageObject cargoWPO = findWPOFromObjProp(tarWPO, "Cargo");
            qtTake.Cargo = getContentInventory(cargoWPO);

            ReadString(tarWPO, "SourceTag", out qtTake.SourceTag);
            ReadLocalizedString(tarWPO, "SourceDescription", locStrings, out qtTake.SourceDescription);

            //TODO: check if this actually occurs in packages
            int takeOptionTemp;
            ReadInt(tarWPO, "Option", out takeOptionTemp);
            qtTake.Option = (ERadialMenuOptions)takeOptionTemp;

            return qtTake;
        }

        QT_Hunt getQTHunt(WrappedPackageObject tarWPO, SBResources resources)
        {
            var qtHunt = ScriptableObject.CreateInstance<QT_Hunt>();

            ReadObject(tarWPO, "Target", resources, out qtHunt.Target);
            ReadInt(tarWPO, "Amount", out qtHunt.Amount);

            return qtHunt;
        }

        QT_Subquest getQTSubquest(WrappedPackageObject tarWPO)
        {
            var qtSubquest = ScriptableObject.CreateInstance<QT_Subquest>();

            //TODO
            //Valshaaran : struggling to find any instances whatsoever of this target type in quest packages
            //qtSubquest.SubQuestID;

            return qtSubquest;
        }

        QT_Deliver getQTDeliver(WrappedPackageObject tarWPO, SBResources resources)
        {
            var qtDeliver = ScriptableObject.CreateInstance<QT_Deliver>();

            ReadObject(tarWPO, "Address", resources, out qtDeliver.NpcRecipientID);
            ReadInt(tarWPO, "Amount", out qtDeliver.Amount);
            qtDeliver.Cargo = getItemType(tarWPO, "Cargo");
            ReadObject(tarWPO, "DeliveryConversation", resources, out qtDeliver.DeliveryConvID);

            return qtDeliver;
        }

        QT_Fedex getQTFedex(WrappedPackageObject tarWPO, SBResources resources)
        {
            var qtFedex = ScriptableObject.CreateInstance<QT_Fedex>();

            ReadObject(tarWPO, "Address", resources, out qtFedex.NpcRecipientID);
            qtFedex.Cargo = getContentInventory(findWPOFromObjProp(tarWPO, "Cargo"));
            ReadInt(tarWPO, "Price", out qtFedex.Price);

            ReadObject(tarWPO, "ThankYou", resources, out qtFedex.ThanksConvID);

            //Add Thanks topic to recipient NPC
            //TODO: Some QTFedex targets don't specify recipient or thanks topic
            //Could be assumed that in these cases recipient is quest finisher, thanks topic is quest finish topic?
            //Could either extract these here, or handle in runtime quest code
            //Revisit this when implementing quest target/finish logic

            if ((qtFedex.ThanksConvID != null) && (qtFedex.NpcRecipientID != null))
                addTopicToNPC(qtFedex.ThanksConvID, qtFedex.NpcRecipientID);

            return qtFedex;
        }

        #endregion

        #region Helpers

        #region Items

        Content_Inventory getRewardItems(WrappedPackageObject rewardWPO)
        {
            var CIProp = rewardWPO.FindProperty("RewardItems");
            var contentInvWPO = extractorWindowRef.ActiveWrapper.FindObjectWrapper(CIProp.GetValue<string>());
            return getContentInventory(contentInvWPO);
        }

        Content_Inventory getContentInventory(WrappedPackageObject contentInvWPO)
        {
            Content_Inventory output = new Content_Inventory();
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

        Item_Type getItemType(WrappedPackageObject wpo, string itemTPropName)
        {
            var output = new Item_Type();

            //Link item reference

            var itemProp = wpo.FindProperty(itemTPropName);

            //Match collection name
            foreach (var ic in itemCols)
            {
                if (itemProp.Value.StartsWith(ic.name))
                {
                    //Retrieve Item_Type
                    foreach (var it in ic.items)
                    {
                        if (itemProp.Value.Contains(it.internalName))
                        {
                            //populate Item_Type and ID
                            output = it;
                            break;
                        }
                    }
                    break;
                }
            }

            //output
            return output;

        }

        #endregion

        #region NPCs
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
                        bool isDuplicate = false;
                        foreach (var questTopic in alteredNT.QuestTopics)
                        {
                            if (questTopic.ID == topic.ID)
                            {
                                isDuplicate = true;
                                break;
                            }
                        }
                        if (!isDuplicate)
                        {
                            //Not a duplicate, so add
                            alteredNT.QuestTopics.Add(topic);
                            //AssetDatabase.AddObjectToAsset(topic, nt);
                            EditorUtility.CopySerialized(alteredNT, nt);
                            EditorUtility.SetDirty(nt);
                        }


                        return;
                    }
                }
            }
        }
        #endregion

        #region Conversations

        #endregion

        #endregion
    }
}