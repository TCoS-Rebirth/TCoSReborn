using Common;
using Common.UnrealTypes;
using Database.Static;
using Gameplay.Entities;
using Gameplay.Entities.Interactives;
using Gameplay.Events;
using Gameplay.Quests;
using Gameplay.Quests.QuestTargets;
using Gameplay.RequirementSpecifier;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utility;
using World;

namespace PackageExtractor.Adapter
{
    public class ILEExtractor : ExtractorAdapter
    {
        Zone targetZone;

        ILEIDCollection ileCol;
        List<QuestCollection> questCols = new List<QuestCollection>();

        public override string Name
        {
            get { return "ILE Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract InteractiveLevelElements from a map package, placing them in the provided zone"; }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Target zone");
            targetZone = EditorGUILayout.ObjectField(targetZone, typeof(Zone), true) as Zone;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            PackageWrapper pW = extractorWindowRef.ActiveWrapper;

            //Load quest data
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Quests/");
            foreach (var f in files)
            {
                var qc = AssetDatabase.LoadAssetAtPath<QuestCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (qc != null)
                {
                    questCols.Add(qc);
                }
            }

            if (targetZone == null)
            {
                Log("Select a zone first", Color.yellow);
                return;
            }

            var ieHolder = targetZone.transform.Find("InteractiveElements");
            if (ieHolder == null)
            {
                Log("InteractiveElementsHolder not found", Color.yellow);
                return;
            }

            for (var i = ieHolder.childCount; i-- > 0;)
            {
                Object.DestroyImmediate(ieHolder.GetChild(i).gameObject);
            }

            foreach (var ie in ieHolder.GetComponentsInChildren<InteractiveLevelElement>())
            {
                Object.DestroyImmediate(ie.gameObject);
            }

            //Load levelObjectID data
            ileCol = AssetDatabase.LoadAssetAtPath<ILEIDCollection>("Assets/GameData/Interactives/" + targetZone.name + ".asset");
            if (ileCol == null)
                {
                ileCol = ScriptableObject.CreateInstance<ILEIDCollection>();
                ileCol.ileIDs = new List<ILEID>();
                }
            

            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (    wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("InteractiveLevelElement")
                    || wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("InteractiveChair")
                    || wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("InteractiveMailbox")
                    || wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("InteractiveQuestElement")
                    || wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("InteractiveShop"))
                {                    

                    var go = new GameObject(wpo.sbObject.Name);
                    go.transform.parent = ieHolder;
                    var output = go.AddComponent<InteractiveLevelElement>();
                    output.ActiveZone = targetZone;

                    string tagName;
                    ReadString(wpo, "Tag", out tagName);
                    output.Name = tagName;

                    //Specialised
                    if (wpo.Name.StartsWith("InteractiveLevelElement")) output.ileType = EILECategory.ILE_Base;

                    if (wpo.Name.StartsWith("InteractiveChair")) {
                        output.ileType = EILECategory.ILE_Chair;

                        var sitAction = ScriptableObject.CreateInstance<ILEAction>();
                        if (output.Actions == null) output.Actions = new List<ILEAction>();
                        output.Actions.Add(sitAction);
                        sitAction.menuOption = ERadialMenuOptions.RMO_SIT;
                        var sitComp = ScriptableObject.CreateInstance<InteractionSit>();
                        sitComp.owner = output;
                        ReadVector3(wpo, "ActionPositionOffset", out sitComp.Offset);
                        sitComp.Offset = UnitConversion.ToUnity(sitComp.Offset);
                        sitAction.StackedActions.Add(sitComp);

                    }
                    if (wpo.Name.StartsWith("InteractiveMailbox")) output.ileType = EILECategory.ILE_Mailbox;
                    if (wpo.Name.StartsWith("InteractiveQuestElement")) output.ileType = EILECategory.ILE_Quest;
                    if (wpo.Name.StartsWith("InteractiveShop")) output.ileType = EILECategory.ILE_Shop;


                    //Valshaaran - Assign from levelObjectID data. Assigns -1 if not found
                    output.LevelObjectID = ileCol.GetLOID(wpo.Name);

                    //int.TryParse(wpo.sbObject.Name.Replace("InteractiveLevelElement", string.Empty), out goid);
                    //int.TryParse(wpo.sbObject.Name.Replace("\0", string.Empty), out goid);

                    Vector3 loc;
                    Vector3 rot;                                        

                    //Positioning
                    if (ReadVector3(wpo, "Location", out loc))
                    {
                        go.transform.position = UnitConversion.ToUnity(loc);
                    }
                    if (ReadVector3(wpo, "Rotation", out rot))
                    {
                        go.transform.rotation = Quaternion.Euler(rot);
                    }

                    #region Actions
                    foreach (var prop in wpo.sbObject.IterateProperties())
                    {
                        if (prop.Name == "Actions")
                        {
                            output.Actions = new List<ILEAction>();

                            foreach (var actionProp in prop.IterateInnerProperties())
                            {
                                var actionObj = ScriptableObject.CreateInstance<ILEAction>() ;

                                //MenuOption
                                actionObj.menuOption = (ERadialMenuOptions) actionProp.GetInnerProperty("MenuOption").GetValue<int>();

                                //StackedActions
                                var stackedActionsProp = actionProp.GetInnerProperty("StackedActions");
                                actionObj.StackedActions = new List<InteractionComponent>();
                                foreach(var stackedActionProp in stackedActionsProp.IterateInnerProperties())
                                {
                                    var intCompWPO = FindReferencedObject(stackedActionProp);
                                    var intComp = extractIntComp(intCompWPO, resources, localizedStrings, pW, actionObj.menuOption, tagName);
                                    intComp.owner = output;
                                    actionObj.StackedActions.Add(intComp);
                                }

                                //Requirements
                                var reqsProp = actionProp.GetInnerProperty("Requirements");
                                actionObj.Requirements = new List<Content_Requirement>();
                                foreach (var reqProp in reqsProp.IterateInnerProperties())
                                {
                                    var referencedReqWPO = FindReferencedObject(reqProp);
                                    actionObj.Requirements.Add(getReq(referencedReqWPO, resources, pW, null));
                                }

                                /*
                                if (    actionObj.menuOption == ERadialMenuOptions.RMO_LOOT
                                    ||  actionObj.menuOption == ERadialMenuOptions.RMO_USE)
                                {
                                    #region Link quest interactive elements
                                    var qtActionObj = ScriptableObject.CreateInstance<ILEQTAction>();
                                    qtActionObj.importBase(actionObj);                                    

                                    //actionObj = qtActionObj;
                                    output.Actions.Add(qtActionObj);
                                    #endregion
                                }
                                */
                                output.Actions.Add(actionObj);
                                
                            }
                        }
                    }
                    #endregion

                        output.InitEnabled = true;
                        output.InitColl = ECollisionType.COL_Blocking;

                }
            }
        } 
          
        private InteractionComponent extractIntComp(WrappedPackageObject wpoIn, SBResources resources, SBLocalizedStrings locStrings, PackageWrapper pW, ERadialMenuOptions activeOption, string tagName)
        {
            var output = ScriptableObject.CreateInstance<InteractionComponent>();

            string className = wpoIn.sbObject.ClassName.Replace("SBGamePlay.", string.Empty);
            switch (className)
            {
                case "Interaction_Action":
                    output = getIAct(wpoIn, resources, locStrings, pW);
                    break;

                case "Interaction_Animation":
                    var iani = ScriptableObject.CreateInstance<InteractionAnimation>();
                    ReadString(wpoIn, "animName", out iani.AnimName);
                    ReadFloat(wpoIn, "LoopDuration", out iani.LoopDuration);
                    output = iani;
                    break;

                case "Interaction_Enable":
                    var ien = ScriptableObject.CreateInstance<InteractionEnable>();
                    ReadBool(wpoIn, "Enabled", out ien.Enabled);
                    output = ien;
                    break;

                case "Interaction_EnableCollision":
                    var ienc = ScriptableObject.CreateInstance<InteractionEnableCollision>();
                    ReadBool(wpoIn, "EnableCollision", out ienc.EnableCollision);
                    output = ienc;
                    break;

                case "Interaction_Event":
                    var iev = ScriptableObject.CreateInstance<InteractionEvent>();
                    ReadString(wpoIn, "EventTag", out iev.EventTag);
                    output = iev;
                    break;

                case "Interaction_Move":
                    var imove = ScriptableObject.CreateInstance<InteractionMove>();

                    ReadVector3(wpoIn, "Movement", out imove.Movement);
                    imove.Movement = UnitConversion.ToUnity(imove.Movement);

                    var unrRot = new Rotator();
                    ReadRotator(wpoIn, "Rotation", out unrRot);
                    imove.Rotation = UnitConversion.ToUnity(unrRot);
                    ReadFloat(wpoIn, "TimeSec", out imove.TimeSec);

                    output = imove;
                    break;

                case "Interaction_Progress":
                    var iprog = ScriptableObject.CreateInstance<InteractionProgress>();
                    iprog.ProgressSeconds = 0.0f;
                    ReadFloat(wpoIn, "ProgressSeconds", out iprog.ProgressSeconds);
                    output = iprog;
                    break;

                case "Interaction_Quest":
                    output = getIQuest(wpoIn, resources, locStrings, pW, tagName);
                    break;

                case "Interaction_RandomTimer":
                    var irt = ScriptableObject.CreateInstance<InteractionRandomTimer>();
                    var rangeProp = wpoIn.FindProperty("RangeSeconds");
                    foreach (var prop in rangeProp.IterateInnerProperties())
                    {
                        if (prop.Name == "Min") float.TryParse(prop.Value, out irt.MinTime);
                        if (prop.Name == "Max") float.TryParse(prop.Value, out irt.MaxTime);
                    }
                    output = irt;
                    break;

                case "Interaction_Show":
                    var ishow = ScriptableObject.CreateInstance<InteractionShow>();
                    ReadBool(wpoIn, "Show", out ishow.Show);
                    output = ishow;
                    break;

                    //TODO : Implement the rest of InteractionComponent subclasses
            }

            output.activeOption = activeOption;
            ReadBool(wpoIn, "Reverse", out output.Reverse);

            return output;
        }

        private InteractionAction getIAct(WrappedPackageObject wpoIn, SBResources res, SBLocalizedStrings locStrings, PackageWrapper pW)
        {
            var output = ScriptableObject.CreateInstance<InteractionAction>();
            output.Actions = new List<Content_Event>();
            foreach (var prop in wpoIn.sbObject.IterateProperties())
            {
                if (prop.Name == "Actions")
                {
                    foreach (var actionProp in prop.IterateInnerProperties())
                    {
                        var evWPO = FindReferencedObject(actionProp);
                        if (evWPO != null)
                        {
                            output.Actions.Add(ExtractEvent(evWPO, res, locStrings, pW, null));
                        }
                    }
                }
            }
            return output;
        }
     
        private InteractionQuest getIQuest(WrappedPackageObject wpoin, SBResources res, SBLocalizedStrings locStrings, PackageWrapper pW, string tagName)
        {
            var output = ScriptableObject.CreateInstance<InteractionQuest>();

            //Find tagName
            bool found = false;
            foreach (var questCol in questCols)
            {
                foreach (var quest in questCol.looseQuests)
                {
                    var qTT = questTaggedTarget(quest, tagName);
                    if (qTT >= 0)
                    {
                        output.Quest = quest;
                        output.TarIndex = qTT;
                        found = true;
                        break;
                    }
                }
                if (found) break;

                foreach (var chain in questCol.questChains)
                {
                    foreach (var quest in chain.quests)
                    {
                        var qTT = questTaggedTarget(quest, tagName);
                        if (qTT >= 0)
                        {
                            output.Quest = quest;
                            output.TarIndex = qTT;
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
                if (found) break;
            }

            return output;
        }

        /// <summary>
        /// Returns the target index with the tag if the tag is found
        /// Otherwise returns -1
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        private int questTaggedTarget(Quest_Type q, string searchTag)
        {
            for (int n = 0; n < q.targets.Count; n++)
            {
                var target = q.targets[n];

                //Interactor, place, take
                if (target is QT_Take)
                {
                    var qtTake = target as QT_Take;
                    if(qtTake.SourceTag == searchTag)
                    {
                        return n;
                    }
                }
                else if (target is QT_Place)
                {
                    var qtPlace = target as QT_Place;
                    if (qtPlace.TargetTag == searchTag)
                    {
                        return n;
                    }
                }
                else if (target is QT_Interactor)
                {
                    var qtInteractor = target as QT_Interactor;
                    if (qtInteractor.TargetTag == searchTag)
                    {
                        return n;
                    }
                }
            }
            return -1;
        } 

    }
}