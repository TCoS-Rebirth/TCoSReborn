using Common;
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
            var questCols = new List<QuestCollection>();
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

            var ieHolder = targetZone.transform.FindChild("InteractiveElements");
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
                                actionObj.Actions = new List<Content_Event>();
                                foreach(var stackedActionProp in stackedActionsProp.IterateInnerProperties())
                                {
                                    var referencedActionWPO = FindReferencedObject(stackedActionProp);
                                    actionObj.Actions.AddRange(extractActions(referencedActionWPO, resources, pW));
                                }

                                //Requirements
                                var reqsProp = actionProp.GetInnerProperty("Requirements");
                                actionObj.Requirements = new List<Content_Requirement>();
                                foreach (var reqProp in reqsProp.IterateInnerProperties())
                                {
                                    var referencedReqWPO = FindReferencedObject(reqProp);
                                    actionObj.Requirements.Add(getReq(referencedReqWPO, resources, pW, null));
                                }

                                
                                if (    actionObj.menuOption == ERadialMenuOptions.RMO_LOOT
                                    ||  actionObj.menuOption == ERadialMenuOptions.RMO_USE)
                                {
                                    #region Link quest interactive elements
                                    var qtActionObj = ScriptableObject.CreateInstance<ILEQTAction>();
                                    qtActionObj.importBase(actionObj);
                                    //Find tagName
                                    bool found = false;
                                    foreach(var questCol in questCols)
                                    {
                                        foreach(var quest in questCol.looseQuests)
                                        {
                                            var qTT = questTaggedTarget(quest, tagName);
                                            if (qTT >= 0)
                                            {
                                                qtActionObj.Quest = quest;
                                                qtActionObj.TargetIndex = qTT;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found) break;

                                        foreach(var chain in questCol.questChains)
                                        {
                                            foreach (var quest in chain.quests)
                                            {
                                                var qTT = questTaggedTarget(quest, tagName);
                                                if (qTT >= 0)
                                                {
                                                    qtActionObj.Quest = quest;
                                                    qtActionObj.TargetIndex = qTT;
                                                    found = true;
                                                    break;
                                                }
                                            }
                                            if (found) break;
                                        }
                                        if (found) break;
                                    }
                                    //actionObj = qtActionObj;
                                    output.Actions.Add(qtActionObj);
                                    #endregion
                                }

                                else output.Actions.Add(actionObj);
                            }
                        }
                    }
                    #endregion

                    

                }
            }
        } 
          
        private List<Content_Event> extractActions(WrappedPackageObject wpoIn, SBResources resources, PackageWrapper pW)
        {
            var output = new List<Content_Event>();

            foreach(var prop in wpoIn.sbObject.IterateProperties())
            {
                if (prop.Name == "Actions")
                {
                    foreach(var actionProp in prop.IterateInnerProperties())
                    {
                        var evWPO = FindReferencedObject(actionProp);
                        if (evWPO != null)
                        {
                            output.Add(ExtractEvent(evWPO, resources, pW, null));
                        }
                    }
                    break;
                }
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