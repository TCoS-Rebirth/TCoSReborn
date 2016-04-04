using System;
using System.Collections.Generic;
using Common;
using Common.UnrealTypes;
using Database.Static;
using Gameplay;
using Gameplay.Conversations;
using Gameplay.Events;
using Gameplay.RequirementSpecifier;
using UnityEngine;
using UnityEditor;

namespace PackageExtractor.Adapter
{
    public abstract class ExtractorAdapter
    {
        public PackageExtractorWindow extractorWindowRef;
        public Action<string, Color> OnLogMessage;
        public abstract string Name { get; }
        public abstract string Description { get; }

        protected void Log(string msg, Color c)
        {
            if (OnLogMessage != null)
            {
                OnLogMessage(msg, c);
            }
        }

        public virtual void DrawGUI(Rect r)
        {
        }

        public abstract void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings);

        public virtual bool IsCompatible(PackageWrapper p)
        {
            return p != null;
        }

        protected void LogParsingError(string propName)
        {
            Log(string.Format("Error parsing {0}", propName), Color.yellow);
        }

        protected string FormatSearchableName(WrappedPackageObject wpo)
        {
            return string.Format("{0}.{1}.{2}", extractorWindowRef.ActiveWrapper.Name, wpo.sbObject.Package, wpo.Name)
                .Replace("\0", string.Empty)
                .Replace("null.", string.Empty);
        }

        protected string FormatInternalName(WrappedPackageObject wpo)
        {
            return string.Format("{0}.{1}", wpo.sbObject.Package, wpo.Name).Replace("\0", string.Empty).Replace("null.", string.Empty);
        }

        protected WrappedPackageObject FindReferencedObject(WrappedPackageObject source, string fieldName)
        {
            SBProperty sbp;
            if (source.sbObject.Properties.TryGetValue(fieldName, out sbp))
            {
                return FindReferencedObject(sbp);
            }
            return null;
        }

        protected bool IsOfClass(WrappedPackageObject wpo, string className)
        {
            return wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith(className, StringComparison.Ordinal);
        }

        protected WrappedPackageObject FindReferencedObject(SBProperty source)
        {
            var nameParts = source.Value.Split('.');
            if (nameParts.Length == 2)
            {
                var searchName = nameParts[nameParts.Length - 1];
                var sbo = extractorWindowRef.ActiveWrapper.FindReferencedObject(searchName, nameParts[0]);
                if (sbo != null)
                {
                    return new WrappedPackageObject(sbo);
                }
            }
            else if (nameParts.Length == 1)
            {
                var sbo = extractorWindowRef.ActiveWrapper.FindReferencedObject(nameParts[0], "");
                if (sbo != null)
                {
                    return new WrappedPackageObject(sbo);
                }
            }
            else
            {
                var lastIndex = source.Value.LastIndexOf('.');
                var packagePart = source.Value.Substring(0, lastIndex);
                var objName = nameParts[nameParts.Length - 1];
                var sbo = extractorWindowRef.ActiveWrapper.FindReferencedObject(objName, packagePart);
                sbo = extractorWindowRef.ActiveWrapper.FindReferencedObject(objName, nameParts[nameParts.Length - 2]);
                if (sbo != null)
                {
                    return new WrappedPackageObject(sbo);
                }
            }
            return null;
        }

        protected WrappedPackageObject findWPOFromObjProp(WrappedPackageObject wpoIn, string propName)
        {
            var prop = wpoIn.FindProperty(propName);
            if (prop != null)
            {
                WrappedPackageObject output = extractorWindowRef.ActiveWrapper.FindObjectWrapper(prop.GetValue<string>());
                if (output == null)
                {
                    Log("findWPOFromObjProp : Failed to find WPO " + prop.Value + " in this package wrapper", Color.red);
                    return null;
                }
                else
                {
                    return output;
                }
                //ConversationTopic fullTopic = getConvTopicFull(topicWPO, resourcesProp, locStrings, extractorWindowRef.ActiveWrapper);
                //AssetDatabase.AddObjectToAsset(fullTopic, convCol);
                //Debug.Log("Got topic " + tarWPO.sbObject.Package + "." + topicWPO.Name);
            }
            else
            {
                Log("findWPOFromObjProp : Failed to find prop " + propName + " in WPO " + wpoIn.Name, Color.yellow);
                return null;
            }
        }

        #region Parsing

        protected bool ReadString(WrappedPackageObject obj, string propName, out string value, bool logErrors = false)
        {
            SBProperty groupProp;
            if (obj.sbObject.Properties.TryGetValue(propName, out groupProp))
            {
                value = groupProp.Value;
                return true;
            }
            if (logErrors)
            {
                LogParsingError(propName);
            }
            value = string.Empty;
            return false;
        }

        protected bool ReadByte(WrappedPackageObject obj, string propName, out byte value, bool logErrors = false)
        {
            SBProperty prop;
            if (obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                if (TryParse(prop.Value, out value))
                {
                    return true;
                }
            }
            if (logErrors)
            {
                LogParsingError(propName);
            }
            value = 0;
            return false;
        }

        protected bool ReadEnum<T>(WrappedPackageObject obj, string propName, out T value, bool logErrors = false) where T : struct, IConvertible
        {
            if (!typeof (T).IsEnum)
            {
                value = default(T);
                return false;
            }
            SBProperty prop;
            if (obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                byte v;
                if (TryParse(prop.Value, out v))
                {
                    value = (T) (object) (int) v;
                    return true;
                }
            }
            if (logErrors)
            {
                LogParsingError(propName);
            }
            value = default(T);
            return false;
        }

        protected bool ReadInt(WrappedPackageObject obj, string propName, out int value, bool logErrors = false)
        {
            SBProperty prop;
            if (obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                if (TryParse(prop.Value, out value))
                {
                    return true;
                }
            }
            if (logErrors)
            {
                LogParsingError(propName);
            }
            value = 0;
            return false;
        }

        protected bool ReadLocalizedString(WrappedPackageObject obj, string propName, SBLocalizedStrings locStrings, out string value, bool logErrors = false)
        {
            SBProperty nameProp;
            if (obj.sbObject.Properties.TryGetValue(propName, out nameProp))
            {
                int nameID;
                if (TryParse(nameProp.Value, out nameID))
                {
                    var ls = locStrings.GetString(nameID);
                    if (ls != null)
                    {
                        value = ls.languageStrings[0];
                        if (value == "[No Text]")
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            if (logErrors)
            {
                LogParsingError(propName);
            }
            value = "[No Text]";
            return false;
        }

        protected bool ReadLocalizedString(WrappedPackageObject obj, string propName, SBLocalizedStrings locStrings, out SBLocalizedString value, bool logErrors = false)
        {
            SBProperty nameProp;
            if (obj.sbObject.Properties.TryGetValue(propName, out nameProp))
            {
                int nameID;
                if (TryParse(nameProp.Value, out nameID))
                {
                    var ls = locStrings.GetString(nameID);
                    if (ls != null)
                    {
                        value = ls;
                        return true;
                    }
                }
            }
            if (logErrors)
            {
                LogParsingError(propName);
            }
            value = null;
            return false;
        }

        protected bool ReadFloat(WrappedPackageObject obj, string propName, out float value, bool logErrors = false)
        {
            SBProperty prop;
            if (obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                if (float.TryParse(prop.Value, out value))
                {
                    return true;
                }
            }
            if (logErrors)
            {
                LogParsingError(propName);
            }
            value = 0f;
            return false;
        }

        protected bool ReadBasicArray<T>(WrappedPackageObject obj, string arrayName, out T[] values, bool logErrors = false) where T : IConvertible
        {
            SBProperty prop;
            if (!obj.sbObject.Properties.TryGetValue(arrayName, out prop))
            {
                values = new T[0];
                if (logErrors)
                {
                    LogParsingError(arrayName);
                }
                return false;
            }
            var v = new List<T>();
            foreach (var sbp in prop.IterateInnerProperties())
            {
                T val;
                if (TryParse(sbp.Value, out val))
                {
                    v.Add(val);
                }
                else
                {
                    if (logErrors)
                    {
                        LogParsingError(arrayName + " innerProperty");
                    }
                    values = new T[0];
                    return false;
                }
            }
            values = v.ToArray();
            return true;
        }

        protected bool ReadByteArray(WrappedPackageObject obj, string propName, out List<byte> values, bool logErrors = false)
        {
            SBProperty prop;
            if (!obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                values = null;
                if (logErrors)
                {
                    LogParsingError(propName);
                }
                return false;
            }
            values = new List<byte>();
            foreach (var kv in prop.Array)
            {
                byte byteValue;
                if (TryParse(kv.Value.Value, out byteValue))
                {
                    values.Add(byteValue);
                }
                else
                {
                    if (logErrors)
                    {
                        LogParsingError(propName);
                    }
                    return false;
                }
            }
            return true;
        }

        protected bool ReadObject(WrappedPackageObject obj, string propName, SBResources res, out SBResource value, bool logErrors = false)
        {
            SBProperty prop;
            if (!obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                value = null;
                if (logErrors)
                {
                    LogParsingError(propName);
                }
                return false;
            }
            value = res.GetResource(prop.Value);
            if (value == null)
            {
                if (logErrors)
                {
                    LogParsingError(propName);
                }
                return false;
            }
            return true;
        }

        protected bool ReadObjectArray(WrappedPackageObject obj, string propName, SBResources res, out List<SBResource> values, bool logErrors = false)
        {
            SBProperty prop;
            if (!obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                values = null;
                if (logErrors)
                {
                    LogParsingError(propName);
                }
                return false;
            }
            values = new List<SBResource>();
            foreach (var kv in prop.Array)
            {
                var resValue = res.GetResource(kv.Value.Value);
                if (resValue != null)
                {
                    values.Add(new SBResource(resValue.ID, resValue.Name));
                }
                else
                {
                    if (logErrors)
                    {
                        LogParsingError(propName);
                    }
                    return false;
                }
            }
            return true;
        }

        protected bool ReadVector3(WrappedPackageObject obj, string propName, out Vector3 value, bool logErrors = false)
        {
            SBProperty prop;
            if (!obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                value = Vector3.zero;
                if (logErrors)
                {
                    LogParsingError(propName);
                }
                return false;
            }
            return TryConvertToVector3(prop.Value, out value);
            //return true;
        }

        protected bool TryConvertToVector3(string vectorstring, out Vector3 v)
        {
            var components = vectorstring.Split(',');
            if (components.Length != 3)
            {
                v = Vector3.zero;
                LogParsingError("Converting Vector3(" + vectorstring + ")");
                return false;
            }
            v = new Vector3(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]));
            return true;
        }

        protected bool ReadRotator(WrappedPackageObject obj, string propName, out Rotator value, bool logErrors = false)
        {
            SBProperty prop;
            if (!obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                value = Rotator.Zero;
                if (logErrors)
                {
                    LogParsingError(propName);
                }
                return false;
            }
            var components = prop.Value.Split(',');
            if (components.Length != 3)
            {
                value = Rotator.Zero;
                LogParsingError("Parsing value(" + propName + ")");
                return false;
            }
            value = new Rotator(int.Parse(components[0]), int.Parse(components[1]), int.Parse(components[2]));
            return true;
        }

        protected bool ReadBool(WrappedPackageObject obj, string propName, out bool value, bool logErrors = false)
        {
            SBProperty prop;
            if (!obj.sbObject.Properties.TryGetValue(propName, out prop))
            {
                value = false;
                if (logErrors)
                {
                    LogParsingError(propName);
                }
                return false;
            }
            if (TryParse(prop.Value, out value))
            {
                return true;
            }
            return false;
        }

        protected bool TryParse<T>(string value, out T converted)
        {
            try
            {
                converted = (T) Convert.ChangeType(value, typeof (T));
                return true;
            }
            catch
            {
                converted = default(T);
                return false;
            }
        }

        #endregion

        #region Specialized

        #region Requirements

        protected Content_Requirement getReq(WrappedPackageObject reqObj, SBResources resources, PackageWrapper pW, UnityEngine.Object assetObj)
        {
            Content_Requirement output; // = ScriptableObject.CreateInstance<ConversationTopic>();           


            switch (reqObj.sbObject.ClassName.Replace("SBGamePlay.", string.Empty))
            {
                //Switch Content_Requirement subclasses
                case "Req_And":
                    output = getReqAnd(reqObj, resources, pW, assetObj);  //special method
                    break;

                case "Req_Area":
                    Req_Area reqArea = ScriptableObject.CreateInstance<Req_Area>();
                    ReadString(reqObj, "AreaTag", out reqArea.AreaTag);
                    output = reqArea;
                    break;

                case "Req_Chance":
                    Req_Chance reqch = ScriptableObject.CreateInstance<Req_Chance>();
                    ReadFloat(reqObj, "Chance", out reqch.Chance);
                    output = reqch;
                    break;

                case "Req_Class":
                    Req_Class reqcl = ScriptableObject.CreateInstance<Req_Class>();
                    ReadEnum(reqObj, "RequiredClass", out reqcl.RequiredClass);
                    output = reqcl;
                    break;

                case "Req_Conditional":
                    Req_Conditional recc = ScriptableObject.CreateInstance<Req_Conditional>();
                    var condProp = reqObj.FindProperty("Condition");
                    if (condProp != null)
                    {
                        var condPropObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(condProp.GetValue<string>());
                        recc.Condition = getReq(condPropObj, resources, pW, assetObj);
                        recc.Condition.name = reqObj.Name + "." + condPropObj.Name;
                        if (assetObj != null)
                            AssetDatabase.AddObjectToAsset(recc.Condition, assetObj);
                    }
                    var recReqProp = reqObj.FindProperty("Requirement");
                    if (recReqProp != null)
                    {
                        var recReqPropObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(recReqProp.GetValue<string>());
                        recc.Requirement = getReq(recReqPropObj, resources, pW, assetObj);
                        recc.Requirement.name = reqObj.Name + "." + recReqPropObj.Name;
                        if (assetObj != null)
                            AssetDatabase.AddObjectToAsset(recc.Requirement, assetObj);
                    }
                    output = recc;
                    break;

                case "Req_Distance":
                    Req_Distance reqd = ScriptableObject.CreateInstance<Req_Distance>();
                    ReadString(reqObj, "ActorTag", out reqd.ActorTag);
                    ReadInt(reqObj, "Distance", out reqd.Distance);
                    ReadEnum(reqObj, "Operator", out reqd.Operator);
                    output = reqd;
                    break;

                case "Req_Equipment":
                    Req_Equipment reqe = ScriptableObject.CreateInstance<Req_Equipment>();
                    ReadString(reqObj, "Equipment", out reqe.temporaryEquipmentName);
                    reqe.equipmentID = resources.GetResourceID(reqe.temporaryEquipmentName);
                    output = reqe;
                    break;

                case "Req_Faction":
                    Req_Faction reqfac = ScriptableObject.CreateInstance<Req_Faction>();
                    ReadString(reqObj, "RequiredTaxonomy", out reqfac.temporaryTaxonomyName);
                    reqfac.taxonomyID = resources.GetResourceID(reqfac.temporaryTaxonomyName);
                    output = reqfac;
                    break;

                case "Req_False":
                    Req_False reqfalse = ScriptableObject.CreateInstance<Req_False>();
                    output = reqfalse;
                    break;

                case "Req_GameActorEnabled":
                    Req_GameActorEnabled reqga = ScriptableObject.CreateInstance<Req_GameActorEnabled>();
                    ReadString(reqObj, "Tag", out reqga.Tag);
                    ReadBool(reqObj, "AllMustSucceed", out reqga.AllMustSucceed);
                    ReadBool(reqObj, "CheckForEnabled", out reqga.CheckForEnabled);
                    output = reqga;
                    break;

                case "Req_Gender":
                    Req_Gender reqgen = ScriptableObject.CreateInstance<Req_Gender>();
                    if (!ReadEnum(reqObj, "Gender", out reqgen.Gender))
                    {
                        //TODO: guesswork, but when gender == 0, the property seems to dissapear
                        reqgen.Gender = NPCGender.ENG_Male;
                        //Debug.Log("reqgen.Gender = " + reqgen.Gender);
                    }
                    output = reqgen;
                    break;

                case "Req_Inventory":
                    Req_Inventory reqinv = ScriptableObject.CreateInstance<Req_Inventory>();
                    ReadString(reqObj, "Item", out reqinv.temporaryItemName);
                    reqinv.itemID = resources.GetResourceID(reqinv.temporaryItemName);
                    ReadInt(reqObj, "Amount", out reqinv.Amount);
                    ReadEnum(reqObj, "Operator", out reqinv.Operator);
                    output = reqinv;
                    break;

                case "Req_Level":
                    Req_Level reqlev = ScriptableObject.CreateInstance<Req_Level>();
                    ReadInt(reqObj, "RequiredLevel", out reqlev.RequiredLevel);
                    ReadEnum(reqObj, "Operator", out reqlev.Operator);
                    output = reqlev;
                    break;

                case "Req_Money":
                    Req_Money reqmon = ScriptableObject.CreateInstance<Req_Money>();
                    ReadInt(reqObj, "RequiredAmount", out reqmon.RequiredAmount);
                    ReadEnum(reqObj, "Operator", out reqmon.Operator);
                    output = reqmon;
                    break;

                case "Req_Not":
                    Req_Not reqnot = ScriptableObject.CreateInstance<Req_Not>();
                    var reqNotProp = reqObj.FindProperty("Requirement");
                    if (reqNotProp != null)
                    {
                        var reqnotObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(reqNotProp.GetValue<string>());
                        reqnot.Requirement = getReq(reqnotObj, resources, pW, assetObj);
                        reqnot.Requirement.name = reqObj.Name + "." + reqnotObj.Name;
                        if (assetObj != null)
                            AssetDatabase.AddObjectToAsset(reqnot.Requirement, assetObj);
                    }
                    output = reqnot;
                    break;

                case "Req_NPC":
                    Req_NPC reqnpc = ScriptableObject.CreateInstance<Req_NPC>();
                    output = reqnpc;
                    break;

                case "Req_NPC_Exists":
                    Req_NPC_Exists reqne = ScriptableObject.CreateInstance<Req_NPC_Exists>();
                    ReadString(reqObj, "NPCType", out reqne.temporaryNPCName);
                    reqne.npcID = resources.GetResourceID(reqne.temporaryNPCName);
                    ReadBool(reqObj, "MustBeAlive", out reqne.MustBeAlive);
                    output = reqne;
                    break;

                case "Req_NPCType":
                    Req_NPCType reqty = ScriptableObject.CreateInstance<Req_NPCType>();
                    ReadString(reqObj, "RequiredNPCType", out reqty.temporaryNPCTypeName);
                    reqty.npcID = resources.GetResourceID(reqty.temporaryNPCTypeName);
                    output = reqty;
                    break;

                case "Req_Or":
                    Req_Or reqor = ScriptableObject.CreateInstance<Req_Or>();
                    var reqorArrProp = reqObj.FindProperty("Requirements");
                    if (reqorArrProp != null)
                    {
                        reqor.Requirements = new List<Content_Requirement>();
                        foreach (var reqOrProp in reqorArrProp.IterateInnerProperties())
                        {
                            var reqorObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(reqOrProp.GetValue<string>());
                            var reqListItem = getReq(reqorObj, resources, pW, assetObj);
                            reqListItem.name = reqObj.Name + "." + reqorObj.Name;
                            reqor.Requirements.Add(reqListItem);
                            if (assetObj != null)
                                AssetDatabase.AddObjectToAsset(reqListItem, assetObj);
                        }
                    }
                    output = reqor;
                    break;

                case "Req_PePRank":
                    Req_PePRank reqpep = ScriptableObject.CreateInstance<Req_PePRank>();
                    ReadInt(reqObj, "RequiredPep", out reqpep.RequiredPep);
                    ReadEnum(reqObj, "Operator", out reqpep.Operator);
                    output = reqpep;
                    break;

                case "Req_PersistentValue":
                    Req_PersistentValue reqpv = ScriptableObject.CreateInstance<Req_PersistentValue>();

                    //Valshaaran - experimenting with player's active zone 
                    //as contextID for now
                    //reqpv.context = (int)getCurMapID();

                    ReadInt(reqObj, "VariableID", out reqpv.VariableID);
                    ReadInt(reqObj, "Value", out reqpv.Value);
                    ReadEnum(reqObj, "Operator", out reqpv.Operator);
                    output = reqpv;
                    break;

                case "Req_Player":
                    Req_Player reqplayer = ScriptableObject.CreateInstance<Req_Player>();
                    output = reqplayer;
                    break;

                case "Req_QuestActive":
                    Req_QuestActive reqqa = ScriptableObject.CreateInstance<Req_QuestActive>();
                    string tempQAName;
                    ReadString(reqObj, "RequiredQuest", out tempQAName);
                    reqqa.RequiredQuest = resources.GetResource(pW.Name, tempQAName);
                    if (reqqa.RequiredQuest == null)
                        reqqa.RequiredQuest = resources.GetResource(tempQAName);
                    output = reqqa;
                    break;

                case "Req_QuestFinished":
                    Req_QuestFinished reqqf = ScriptableObject.CreateInstance<Req_QuestFinished>();
                    string tempQFName;
                    ReadString(reqObj, "RequiredQuest", out tempQFName);
                    reqqf.RequiredQuest = resources.GetResource(pW.Name, tempQFName);
                    ReadInt(reqObj, "TimesFinished", out reqqf.TimesFinished);
                    output = reqqf;
                    break;

                case "Req_QuestRepeatable":
                    Req_QuestRepeatable reqqr = ScriptableObject.CreateInstance<Req_QuestRepeatable>();
                    string tempQRName;
                    ReadString(reqObj, "quest", out tempQRName);                    
                    reqqr.Quest = resources.GetResource(pW.Name, tempQRName);
                    ReadBool(reqObj, "Repeatable", out reqqr.Repeatable);
                    output = reqqr;
                    break;

                case "Req_QuestReq":
                    Req_QuestReq reqqreq = ScriptableObject.CreateInstance<Req_QuestReq>();
                    string tempQReqName;
                    ReadString(reqObj, "quest", out tempQReqName);
                    reqqreq.quest = resources.GetResource(pW.Name, tempQReqName);
                    output = reqqreq;
                    break;

                case "Req_Race":
                    Req_Race reqrace = ScriptableObject.CreateInstance<Req_Race>();
                    ReadEnum(reqObj, "RequiredRace", out reqrace.RequiredRace);
                    output = reqrace;
                    break;

                case "Req_TargetActive":
                    Req_TargetActive reqta = ScriptableObject.CreateInstance<Req_TargetActive>();
                    string tempTA;
                    ReadString(reqObj, "quest", out tempTA);
                    reqta.quest = resources.GetResource(pW.Name, tempTA);
                    if (reqta.quest == null)
                        reqta.quest = resources.GetResource(tempTA);
                    ReadInt(reqObj, "objective", out reqta.objective);
                    output = reqta;
                    break;

                case "Req_TargetProgress":
                    Req_TargetProgress reqpr = ScriptableObject.CreateInstance<Req_TargetProgress>();
                    string tempTP;
                    ReadString(reqObj, "quest", out tempTP);
                    reqpr.quest = resources.GetResource(tempTP);
                    ReadInt(reqObj, "objective", out reqpr.objective);
                    ReadInt(reqObj, "Progress", out reqpr.Progress);
                    ReadEnum(reqObj, "Operator", out reqpr.Operator);
                    output = reqpr;
                    break;

                case "Req_Team":
                    Req_Team reqteam = ScriptableObject.CreateInstance<Req_Team>();
                    ReadInt(reqObj, "RequiredSize", out reqteam.RequiredSize);
                    ReadEnum(reqObj, "Operator", out reqteam.Operator);
                    output = reqteam;
                    break;

                case "Req_Time":
                    Req_Time reqtime = ScriptableObject.CreateInstance<Req_Time>();
                    ReadFloat(reqObj, "BeginTime", out reqtime.BeginTime);
                    ReadFloat(reqObj, "EndTime", out reqtime.EndTime);
                    output = reqtime;
                    break;

                case "Req_True":
                    Req_True reqtrue = ScriptableObject.CreateInstance<Req_True>();
                    output = reqtrue;
                    break;

                case "Req_World":
                    Req_World reqwo = ScriptableObject.CreateInstance<Req_World>();
                    ReadInt(reqObj, "RequiredWorld", out reqwo.RequiredWorld);
                    output = reqwo;
                    break;

                default:
                    Content_Requirement empty = ScriptableObject.CreateInstance<Content_Requirement>();
                    empty.name = "[Empty:NotFound]";
                    return empty;
            }

            //Read base class fields
            ReadBool(reqObj, "ValidForPlayer", out output.ValidForPlayer);
            ReadBool(reqObj, "ValidForRelevant", out output.ValidForRelevant);
            ReadInt(reqObj, "ControlLocationX", out output.ControlLocationX);
            ReadInt(reqObj, "ControlLocationY", out output.ControlLocationY);

            output.name = reqObj.Name;
            return output;
        }
        /*

                    T ExtractBaseRequirement<T>(WrappedPackageObject reqObj) where T : Content_Requirement
        {
            var obj = ScriptableObject.CreateInstance<T>();
            ReadBool(reqObj, "ValidForPlayer", out obj.ValidForPlayer);
            ReadBool(reqObj, "ValidForRelevant", out obj.ValidForRelevant);
            return obj;
        }
        */

        protected Req_And getReqAnd(WrappedPackageObject reqObj, SBResources resources, PackageWrapper pW, UnityEngine.Object assetObj)
        {
            var output = ScriptableObject.CreateInstance<Req_And>();
            var randReqProp = reqObj.FindProperty("Requirements");
            if (randReqProp != null)
            {
                output.Requirements = new List<Content_Requirement>();
                foreach (var randReq in randReqProp.IterateInnerProperties())
                {
                    var randReqObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(randReq.GetValue<string>());

                    /*
                    string packageString;
                    if (randReqObj.sbObject.Package == null) {
                        packageString = "";
                    }
                    else packageString = randReqObj.sbObject.Package + ".";
                    //output.requirementRefs.Add(packageString + randReqObj.Name);
                    */

                    var newReq = getReq(randReqObj, resources, pW, assetObj);
                    newReq.name = reqObj.Name + "." + randReqObj.Name;
                    output.Requirements.Add(newReq);
                    if (assetObj != null)
                        AssetDatabase.AddObjectToAsset(newReq, assetObj);
                }
            }
            return output;
        }

        #endregion

        #region Events

        protected Content_Event ExtractEvent(WrappedPackageObject eventObject, SBResources res, SBLocalizedStrings locStrings, PackageWrapper pW, UnityEngine.Object assetObj)
        {
            switch (eventObject.sbObject.ClassName.Replace("\0", string.Empty).Replace("SBGamePlay.", string.Empty))
            {
                case "EV_Claustroport":
                    var evcl = ScriptableObject.CreateInstance<EV_Claustroport>();
                    ReadString(eventObject, "DestinationTag", out evcl.DestinationTag);
                    ReadFloat(eventObject, "MaxDistance", out evcl.MaxDistance);
                    ReadBool(eventObject, "UseOrientation", out evcl.UseOrientation);
                    return evcl;
                case "EV_ClientEvent":
                    var evcle = ScriptableObject.CreateInstance<EV_ClientEvent>();
                    ReadString(eventObject, "EventTag", out evcle.EventTag);
                    return evcle;
                case "EV_Conversation":
                    var evco = ScriptableObject.CreateInstance<EV_Conversation>();
                    var convWPO = findWPOFromObjProp(eventObject, "Conversation");
                    evco.Conversation = getConvTopicFull(convWPO, res, locStrings, pW);
                    AssetDatabase.AddObjectToAsset(evco.Conversation, assetObj);
                    return evco;
                case "EV_Die":
                    var evdie = ScriptableObject.CreateInstance<EV_Die>();
                    return evdie;
                case "EV_EffectsApply":
                    var evea = ScriptableObject.CreateInstance<EV_EffectsApply>();
                    var effectsArrProp = eventObject.FindProperty("Effects");
                    if (effectsArrProp != null)
                    {
                        foreach (var effect in effectsArrProp.IterateInnerProperties())
                        {
                            var fxName = effect.GetValue<string>();
                            evea.temporaryEffectsNames.Add(fxName);
                            evea.effectIDs.Add(res.GetResourceID(fxName));
                        }
                    }
                    ReadBool(eventObject, "ApplyToObject", out evea.ApplyToObject);
                    ReadBool(eventObject, "ApplyToSubject", out evea.ApplyToSubject);
                    ReadString(eventObject, "Tag", out evea.Tag);
                    ReadBool(eventObject, "SubjectEffectIsPermanent", out evea.SubjectEffectIsPermanent);
                    return evea;
                case "EV_EffectsRemove":
                    var ever = ScriptableObject.CreateInstance<EV_EffectsRemove>();
                    ReadBool(eventObject, "RemoveFromObject", out ever.RemoveFromObject);
                    ReadBool(eventObject, "RemoveFromSubject", out ever.RemoveFromSubject);
                    ReadString(eventObject, "Tag", out ever.Tag);
                    return ever;
                case "EV_Emote":
                    var evemo = ScriptableObject.CreateInstance<EV_Emote>();
                    ReadEnum(eventObject, "Emote", out evemo.Emote);
                    return evemo;
                case "EV_FinishQuest":
                    var evfq = ScriptableObject.CreateInstance<EV_FinishQuest>();
                    ReadString(eventObject, "quest", out evfq.quest);
                    return evfq;
                case "EV_GiveItem":
                    var evgi = ScriptableObject.CreateInstance<EV_GiveItem>();
                    var ciProp = eventObject.FindProperty("Items");
                    if (ciProp != null)
                    {
                        var ciObject = extractorWindowRef.ActiveWrapper.FindObjectWrapper(ciProp.GetValue<string>());
                        if (ciObject != null)
                        {
                            var ci = new Content_Inventory();
                            var eachItem = ciObject.FindProperty("Items");
                            if (eachItem != null)
                            {
                                foreach (var item in eachItem.IterateInnerProperties())
                                {
                                    var cItem = new Content_Inventory.ContentItem();
                                    var itemNameProp = item.GetInnerProperty("Item");
                                    if (itemNameProp != null)
                                    {
                                        cItem.temporaryItemName = itemNameProp.GetValue<string>();
                                        cItem.itemID = res.GetResourceID(cItem.temporaryItemName);
                                    }
                                    var stackSizeProp = item.GetInnerProperty("StackSize");
                                    if (stackSizeProp != null)
                                    {
                                        cItem.StackSize = stackSizeProp.GetValue<int>();
                                    }
                                    var ColorOneProp = item.GetInnerProperty("Color1");
                                    if (ColorOneProp != null)
                                    {
                                        cItem.Color1 = ColorOneProp.GetValue<byte>();
                                    }
                                    var ColorTwoProp = item.GetInnerProperty("Color2");
                                    if (ColorTwoProp != null)
                                    {
                                        cItem.Color2 = ColorTwoProp.GetValue<byte>();
                                    }
                                    ci.Items.Add(cItem);
                                }
                            }
                            evgi.Items = ci;
                        }
                    }
                    return evgi;
                case "EV_GiveSkill":
                    var evgsk = ScriptableObject.CreateInstance<EV_GiveSkill>();
                    ReadString(eventObject, "Skill", out evgsk.temporarySkillName);
                    evgsk.skillID = res.GetResourceID(evgsk.temporarySkillName);
                    return evgsk;
                case "EV_NPC":
                    var evnpc = ScriptableObject.CreateInstance<EV_NPC>();
                    ReadString(eventObject, "NPC", out evnpc.temporaryNPCname);
                    evnpc.npcID = res.GetResourceID(evnpc.temporaryNPCname);
                    var eventName = eventObject.FindProperty("NPCAction");
                    if (eventName != null)
                    {
                        var npcEvObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(eventName.GetValue<string>());
                        evnpc.NPCAction = ExtractEvent(npcEvObj, res, locStrings, pW, assetObj);
                    }
                    ReadFloat(eventObject, "Radius", out evnpc.Radius);
                    return evnpc;
                case "EV_ObtainQuest":
                    var evoq = ScriptableObject.CreateInstance<EV_ObtainQuest>();
                    ReadString(eventObject, "quest", out evoq.quest);
                    return evoq;
                case "EV_Other":
                    var evother = ScriptableObject.CreateInstance<EV_Other>();
                    var otheractionName = eventObject.FindProperty("OtherAction");
                    if (otheractionName != null)
                    {
                        var otherActionObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(otheractionName.GetValue<string>());
                        evother.OtherAction = ExtractEvent(otherActionObj, res, locStrings, pW, assetObj);
                    }
                    return evother;
                case "EV_Party":
                    var evpa = ScriptableObject.CreateInstance<EV_Party>();
                    ReadFloat(eventObject, "Range", out evpa.Range);
                    var reqInfo = eventObject.FindProperty("Requirements");
                    if (reqInfo != null)
                    {
                        foreach (var reqProp in reqInfo.IterateInnerProperties())
                        {
                            var reqInfoObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(reqProp.GetValue<string>());
                            var reqListItem = getReq(reqInfoObj, res, pW, assetObj);
                            reqListItem.name = eventObject.Name + "." + reqInfoObj.Name;
                            evpa.Requirements.Add(reqListItem);
                            if (assetObj != null)
                                AssetDatabase.AddObjectToAsset(reqListItem, assetObj);
                        }
                    }
                    var paAction = eventObject.FindProperty("PartyAction");
                    if (paAction != null)
                    {
                        var paActionObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(paAction.GetValue<string>());
                        evpa.PartyAction = ExtractEvent(paActionObj, res, locStrings, pW, assetObj);
                    }
                    return evpa;
                case "EV_PartyProgress":
                    var evpp = ScriptableObject.CreateInstance<EV_PartyProgress>();
                    ReadObject(eventObject, "quest", res, out evpp.quest);
                    ReadInt(eventObject, "ObjectiveNr", out evpp.ObjectiveNr);
                    return evpp;
                case "EV_PartyTeleport":
                    var evpt = ScriptableObject.CreateInstance<EV_PartyTeleport>();
                    ReadInt(eventObject, "TargetWorld", out evpt.TargetWorld);
                    ReadString(eventObject, "portalName", out evpt.portalName);
                    return evpt;
                case "EV_PersistentValue":
                    var evpv = ScriptableObject.CreateInstance<EV_PersistentValue>();

                    //Valshaaran - experimenting with player's active zone 
                    //as contextID for now
                    //evpv.context = (int)getCurMapID();

                    ReadInt(eventObject, "VariableID", out evpv.VariableID);
                    ReadInt(eventObject, "Value", out evpv.Value);
                    return evpv;
                case "EV_QuestProgress":
                    var evqp = ScriptableObject.CreateInstance<EV_QuestProgress>();
                    ReadObject(eventObject, "quest", res, out evqp.quest);
                    ReadInt(eventObject, "TargetNr", out evqp.TargetNr);
                    ReadInt(eventObject, "Progress", out evqp.Progress);
                    return evqp;
                case "EV_RemoveItem":
                    var evrit = ScriptableObject.CreateInstance<EV_RemoveItem>();
                    var cirProp = eventObject.FindProperty("Items");
                    if (cirProp != null)
                    {
                        var cirObject = extractorWindowRef.ActiveWrapper.FindObjectWrapper(cirProp.GetValue<string>());
                        if (cirObject != null)
                        {
                            var ci = new Content_Inventory();
                            var eachrItem = cirObject.FindProperty("Items");
                            if (eachrItem != null)
                            {
                                foreach (var item in eachrItem.IterateInnerProperties())
                                {
                                    var cItem = new Content_Inventory.ContentItem();
                                    var itemNameProp = item.GetInnerProperty("Item");
                                    if (itemNameProp != null)
                                    {
                                        cItem.temporaryItemName = itemNameProp.GetValue<string>();
                                        cItem.itemID = res.GetResourceID(cItem.temporaryItemName);
                                    }
                                    var stackSizeProp = item.GetInnerProperty("StackSize");
                                    if (stackSizeProp != null)
                                    {
                                        cItem.StackSize = stackSizeProp.GetValue<int>();
                                    }
                                    var ColorOneProp = item.GetInnerProperty("Color1");
                                    if (ColorOneProp != null)
                                    {
                                        cItem.Color1 = ColorOneProp.GetValue<byte>();
                                    }
                                    var ColorTwoProp = item.GetInnerProperty("Color2");
                                    if (ColorTwoProp != null)
                                    {
                                        cItem.Color2 = ColorTwoProp.GetValue<byte>();
                                    }
                                    ci.Items.Add(cItem);
                                }
                            }
                            evrit.Items = ci;
                        }
                    }
                    return evrit;
                case "EV_RemoveMoney":
                    var evrm = ScriptableObject.CreateInstance<EV_RemoveMoney>();
                    ReadInt(eventObject, "Amount", out evrm.Amount);
                    return evrm;
                case "EV_Scenario":
                    var evsc = ScriptableObject.CreateInstance<EV_Scenario>();
                    ReadString(eventObject, "ScenarioTag", out evsc.ScenarioTag);
                    return evsc;
                case "EV_Self":
                    var evslf = ScriptableObject.CreateInstance<EV_Self>();
                    var slfActionProp = eventObject.FindProperty("SelfAction");
                    if (slfActionProp != null)
                    {
                        var alfAcObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(slfActionProp.GetValue<string>());
                        evslf.SelfAction = ExtractEvent(alfAcObj, res, locStrings, pW, assetObj);
                    }
                    return evslf;
                case "EV_SetClass":
                    var evscl = ScriptableObject.CreateInstance<EV_SetClass>();
                    ReadEnum(eventObject, "DesiredClass", out evscl.DesiredClass);
                    return evscl;
                case "EV_SetFaction":
                    var evsf = ScriptableObject.CreateInstance<EV_SetFaction>();
                    ReadString(eventObject, "DesiredFaction", out evsf.temporaryFactionName);
                    evsf.taxonomyID = res.GetResourceID(evsf.temporaryFactionName);
                    return evsf;
                case "EV_SetHealth":
                    var evsh = ScriptableObject.CreateInstance<EV_SetHealth>();
                    ReadEnum(eventObject, "HealthMode", out evsh.HealthMode);
                    ReadFloat(eventObject, "HealthValue", out evsh.HealthValue);
                    return evsh;
                case "EV_ShowTutorial":
                    var evst = ScriptableObject.CreateInstance<EV_ShowTutorial>();
                    ReadObject(eventObject, "Article", res, out evst.Article);
                    return evst;
                case "EV_Sit":
                    var evsit = ScriptableObject.CreateInstance<EV_Sit>();
                    //ReadString(eventObject, "Chair", out evsit.Chair);
                    ReadVector3(eventObject, "Offset", out evsit.Offset);
                    return evsit;
                case "EV_Skill":
                    var evskill = ScriptableObject.CreateInstance<EV_Skill>();
                    ReadString(eventObject, "Skill", out evskill.temporarySkillName);
                    evskill.skillID = res.GetResourceID(evskill.temporarySkillName);
                    return evskill;
                case "EV_SkillEffects":
                    var evsfx = ScriptableObject.CreateInstance<EV_SkillEffects>();
                    ReadString(eventObject, "Skill", out evsfx.temporarySkillName);
                    evsfx.skillID = res.GetResourceID(evsfx.temporarySkillName);
                    return evsfx;
                case "EV_SkillEffectsTargeted":
                    var evsfxt = ScriptableObject.CreateInstance<EV_SkillEffectsTargeted>();
                    ReadString(eventObject, "Skill", out evsfxt.temporarySkillName);
                    evsfxt.skillID = res.GetResourceID(evsfxt.temporarySkillName);
                    return evsfxt;
                case "EV_SkillEvent":
                    var evsev = ScriptableObject.CreateInstance<EV_SkillEvent>();
                    ReadString(eventObject, "duffEvent", out evsev.temporaryDuffEventName);
                    evsev.duffEventID = res.GetResourceID(evsev.temporaryDuffEventName);
                    return evsev;
                case "EV_SkillTargeted":
                    var evstar = ScriptableObject.CreateInstance<EV_SkillTargeted>();
                    ReadString(eventObject, "Skill", out evstar.temporarySkillName);
                    evstar.skillID = res.GetResourceID(evstar.temporarySkillName);
                    return evstar;
                case "EV_Stand":
                    var evstand = ScriptableObject.CreateInstance<EV_Stand>();
                    return evstand;
                case "EV_Swap":
                    var evsw = ScriptableObject.CreateInstance<EV_Swap>();
                    var swAcProp = eventObject.FindProperty("SwappedAction");
                    if (swAcProp != null)
                    {
                        var swAcObj = extractorWindowRef.ActiveWrapper.FindObjectWrapper(swAcProp.GetValue<string>());
                        evsw.SwappedAction = ExtractEvent(swAcObj, res, locStrings, pW, assetObj);
                    }
                    return evsw;
                case "EV_Teleport":
                    var evtel = ScriptableObject.CreateInstance<EV_Teleport>();
                    ReadInt(eventObject, "TargetWorld", out evtel.TargetWorld);
                    ReadString(eventObject, "Parameter", out evtel.Parameter);
                    ReadBool(eventObject, "IsInstance", out evtel.IsInstance);
                    return evtel;
                case "EV_TriggerEvent":
                    var evtev = ScriptableObject.CreateInstance<EV_TriggerEvent>();
                    ReadString(eventObject, "EventTag", out evtev.EventTag);
                    return evtev;
                case "EV_UntriggerEvent":
                    var evuev = ScriptableObject.CreateInstance<EV_UntriggerEvent>();
                    ReadString(eventObject, "EventTag", out evuev.EventTag);
                    return evuev;
            }
            var emptyEvent = ScriptableObject.CreateInstance<Content_Event>();
            emptyEvent.name = "[Empty:NotFound]";
            return emptyEvent;
        }

        #endregion

        #region Conversations

        protected SBResource getConvTopicRef(WrappedPackageObject CTObj, SBResources resources, string pwName)
        {
            if (CTObj == null)
            {
                return null;
            }
                return GetResource(CTObj, resources, pwName);
        }

        protected ConversationTopic getConvTopicFull(WrappedPackageObject CTObj, SBResources resources, SBLocalizedStrings locStrings, PackageWrapper pW)
        {
            ConversationTopic output; // = ScriptableObject.CreateInstance<ConversationTopic>();           


            switch (CTObj.sbObject.ClassName.Replace("SBGamePlay.", string.Empty))
            {
                //TODO : switch ConversationTopic subclasses once implemented
                case "CT_ProvideQuest":
                    output = getCTProvide(CTObj, resources, pW, locStrings);
                    break;

                case "CT_MidQuest":
                    output = ScriptableObject.CreateInstance<CT_MidQuest>();
                    output.TopicType = EConversationType.ECT_Mid;
                    break;

                case "CT_FinishQuest":
                    output = ScriptableObject.CreateInstance<CT_FinishQuest>();
                    output.TopicType = EConversationType.ECT_Finish;
                    break;

                case "CT_TalkTarget":
                    output = ScriptableObject.CreateInstance<CT_TalkTarget>();
                    output.TopicType = EConversationType.ECT_Talk; //TODO: check
                    break;

                case "CT_Chat":
                    output = ScriptableObject.CreateInstance<CT_Chat>();
                    output.TopicType = EConversationType.ECT_Free; //TODO : Not sure about this
                    break;

                case "CT_Greeting":
                    output = getCTGreeting(CTObj, locStrings);
                    break;

                case "CT_Victory":
                    output = ScriptableObject.CreateInstance<CT_Victory>();
                    output.TopicType = EConversationType.ECT_Victory;
                    break;

                case "CT_Deliver":
                    output = ScriptableObject.CreateInstance<CT_Deliver>();
                    output.TopicType = EConversationType.ECT_Deliver;
                    break;

                case "CT_LastWords":
                    output = ScriptableObject.CreateInstance<CT_LastWords>();
                    output.TopicType = EConversationType.ECT_LastWords;
                    break;

                case "CT_Quest": //Probably unnecessary, appears to be abstract class
                    output = ScriptableObject.CreateInstance<CT_Quest>();
                    output.TopicType = EConversationType.ECT_Talk; //TODO: check
                    break;

                case "CT_Target":
                    output = ScriptableObject.CreateInstance<CT_Target>();
                    output.TopicType = EConversationType.ECT_Talk; //TODO: check
                    break;

                case "CT_Thanks":
                    output = ScriptableObject.CreateInstance<CT_Thanks>();
                    output.TopicType = EConversationType.ECT_Thanks;
                    break;

                default:
                    return null;
            }

            //Read TopicText
            int ttLocStringID;
            if (ReadInt(CTObj, "TopicText", out ttLocStringID))
            {
                //Debug.Log("Read ttLocStringID " + ttLocStringID);
                output.TopicText = locStrings.GetString(ttLocStringID);
            }

            string packageName;
            if (CTObj.sbObject.Package != null)
                packageName = CTObj.sbObject.Package + ".";
            else
                packageName = "";

            var ctName = pW.Name + "." + packageName + CTObj.sbObject.Name;
            output.name = ctName;
            output.internalName = CTObj.Name;
            output.resource = getConvTopicRef(CTObj, resources, pW.Name);


            //Set up nodes
            output.startNodes = getStartNodesRes(CTObj, resources, pW);

            List<SBObject> allNodesAndResps;

            //Recursively get all the nodes and responses - put retrieved WPO refs into exclusion list so no infinite loops
            var exclusionList = new List<int>(); //list of excluded resource IDs
            allNodesAndResps = fillCTRecursive(CTObj.sbObject, pW, resources, exclusionList);

            //Debug.Log(CTObj.sbObject.Package + "." + CTObj.Name + " : " + allNodesAndResps.Count + " nodes and responses");

            //Sort nodes and responses           
            foreach (var nodeOrResp in allNodesAndResps)
            {
                if (nodeOrResp != null)
                {
                    var nORClassName = nodeOrResp.ClassName;

                    //TODO: Handle other node/response classes

                    //Nodes
                    if (nORClassName == "SBGame.Conversation_Node")
                    {
                        output.allNodes.Add(getNodeFromSBO(nodeOrResp, resources, pW.Name, locStrings));
                    }
                    //Responses
                    else if (nORClassName == "SBGame.Conversation_Response" ||
                             nORClassName == "SBGame.Conversation_Continue" ||
                             nORClassName == "SBGamePlay.CR_Accept" ||
                             nORClassName == "SBGamePlay.CR_Decline")
                    {
                        output.allResponses.Add(getRespFromSBO(nodeOrResp, resources, pW.Name, locStrings));
                    }
                }
            }

            //TODO : implement event extractor and use it to populate this topic's events list

            return output;
        }

        protected CT_Greeting getCTGreeting(WrappedPackageObject CTObj, SBLocalizedStrings locStrings)
        {
            var output = ScriptableObject.CreateInstance<CT_Greeting>();
            output.TopicType = EConversationType.ECT_Greeting;
            int dtLocStringID;
            if (ReadInt(CTObj, "DefaultText", out dtLocStringID))
            {
                output.DefaultText = locStrings.GetString(dtLocStringID);
            }
            return output;
        }

        protected CT_ProvideQuest getCTProvide(WrappedPackageObject CTObj, SBResources resources, PackageWrapper pW, SBLocalizedStrings locStrings)
        {
            var output = ScriptableObject.CreateInstance<CT_ProvideQuest>();
            output.TopicType = EConversationType.ECT_Provide;

            var acceptWPO = pW.FindObjectWrapper(CTObj.FindProperty("Accept").GetValue<string>());
            var declineWPO = pW.FindObjectWrapper(CTObj.FindProperty("Decline").GetValue<string>());

            output.Accept = getRespFromSBO(acceptWPO.sbObject, resources, pW.Name, locStrings);
            output.Decline = getRespFromSBO(declineWPO.sbObject, resources, pW.Name, locStrings);

            return output;
        }


        protected List<SBResource> getStartNodesRes(WrappedPackageObject CTObj, SBResources resources, PackageWrapper pW)
        {
            var output = new List<SBResource>();

            var conversations = CTObj.FindProperty("Conversations");
            if (conversations != null)
            {
                foreach (var conv in conversations.IterateInnerProperties())
                {
                    var convName = pW.Name + "." + conv.Value;
                    output.Add(resources.GetResource(convName));
                }
            }

            return output;
        }

        protected ConversationNode getNodeFromSBO(SBObject node, SBResources resources, string pwName, SBLocalizedStrings locStrings)
        {
            var cn = new ConversationNode();

            foreach (var prop in node.IterateProperties())
            {
                if (prop.Name == "Text")
                {
                    cn.textLocStr = locStrings.GetString(int.Parse(prop.Value));
                }

                if (prop.Name == "Responses")
                {
                    foreach (var response in prop.IterateInnerProperties())
                    {
                        cn.responses.Add(resources.GetResource(pwName + "." + response.Value));
                        //Debug.Log("Adding response resource " + respName);
                    }
                }
            }

            var fullName = pwName + "." + node.Package + "." + node.Name;
            cn.resource = resources.GetResource(fullName);

            return cn;
        }

        protected ConversationResponse getRespFromSBO(SBObject resp, SBResources resources, string pwName, SBLocalizedStrings locStrings)
        {
            var cr = new ConversationResponse();

            foreach (var prop in resp.IterateProperties())
            {
                if (prop.Name == "Text")
                {
                    cr.textLocStr = locStrings.GetString(int.Parse(prop.Value));
                }

                if (prop.Name == "Conversations")
                {
                    foreach (var conv in prop.IterateInnerProperties())
                    {
                        cr.conversations.Add(resources.GetResource(pwName + "." + conv.Value));
                        //Debug.Log("Adding response resource " + respName);
                    }
                }
            }

            var fullName = pwName + "." + resp.Package + "." + resp.Name;
            cr.resource = resources.GetResource(fullName);

            return cr;
        }

        protected List<SBObject> fillCTRecursive(SBObject input, PackageWrapper pW, SBResources resources, List<int> exclusionList)
        {
            if (input == null)
            {
                return new List<SBObject>();
            }


            List<SBObject> output;
            output = new List<SBObject>();

            //Add the original input to the output as long as it's a node/response (not a conversation topic start)
            if ((input.ClassName != "SBGamePlay.CT_ProvideQuest") &&
                (input.ClassName != "SBGamePlay.CT_MidQuest") &&
                (input.ClassName != "SBGamePlay.CT_FinishQuest") &&
                (input.ClassName != "SBGamePlay.CT_TalkTarget"))
            {
                output.Add(input);
            }


            //Debug.Log("Adding WPO " + input.Name + " to output");

            SBProperty props = null;
            foreach (var outerProp in input.IterateProperties())
            {
                if ((outerProp.Name == "Conversations") || (outerProp.Name == "Responses"))
                {
                    props = outerProp;
                    break;
                }
            }

            if (props == null)
            {
                //Debug.Log("No Conversations or Responses property found");
            }
            else
            {
                //Debug.Log("Found property " + props.Name);
                List<SBObject> workingSet;
                workingSet = new List<SBObject>();

                foreach (var prop in props.IterateInnerProperties())
                {
                    if (prop.Value != null)
                    {
                        //Debug.Log("Adding WPO " + prop.Value + " to working set");

                        /*
                        //SBObject name equals last part of prop value
                        //SBObject parent equals topic internal name/2nd-last part of prop value
                        var propValueParts = prop.Value.Split('.');
                        var objName = propValueParts[propValueParts.Length - 1];
                        string objParent;

                        if (propValueParts.Length < 2)
                        {
                            objParent = "";
                        }
                        else if (propValueParts.Length < 3)
                        {
                            objParent = propValueParts[propValueParts.Length - 2];
                        }
                        else
                        {
                            objParent = propValueParts[propValueParts.Length - 3] + "." + propValueParts[propValueParts.Length - 2];
                        }
                        ;

                        Log("Finding propSBO - objName = " + objName + ", objParent = " + objParent + "...", Color.yellow);
                        var propSBO = pW.FindReferencedObject(objName, objParent);
                        */

                        var propValueParts = prop.Value.Split('.');

                        //build full package name from value parts
                        var packageName = propValueParts[0];
                        for (int n = 1; n < (propValueParts.Length - 1); n++)
                        {
                            packageName += "." + propValueParts[n]; 
                        }

                        var objName = propValueParts[propValueParts.Length - 1];
                        var propSBO = pW.FindObject(objName, packageName);


                        if (propSBO == null)
                        {
                            Log(prop.Value + " SBO not found in this package, couldn't add", Color.red);
                        }
                        else
                        {
                            Log("..." + propSBO.Package + "." + propSBO.Name + " SBO found", Color.green);

                            var propRefID = resources.GetResourceID(pW.Name + "." + prop.Value);
                            if (propRefID == -1)
                            {
                                Log(prop.Value + " Res ID not found, couldn't add", Color.red);
                            }                       

                            //Only add to working set if not in exclusion list
                            if (!exclusionList.Contains(propRefID))
                            {
                                workingSet.Add(propSBO);
                                exclusionList.Add(propRefID);
                            }
                            else {
                                //Only add to working set if not in exclusion list
                                if (!exclusionList.Contains(propRefID))
                                {
                                    workingSet.Add(propSBO);
                                    exclusionList.Add(propRefID);
                                }
                            }
                        }
                    }
                }
                //add the working set to output
                foreach (var workingObj in workingSet)
                {
                    if (!output.Contains(workingObj))
                        output.Add(workingObj);
                }

                //workingSet now contains the WPOs referenced by the inner properties of Conversations or Responses
                //recursively call fillCTRecursive on each of workingSet, and add the output to output
                foreach (var wsMember in workingSet)
                {
                    foreach (var referenced in fillCTRecursive(wsMember, pW, resources, exclusionList))
                    {
                        if (!output.Contains(referenced))
                        {
                            output.Add(referenced);
                        }
                    }
                }
            }
            return output;
        }

        //Get resource from WPO
        protected SBResource GetResource(WrappedPackageObject wpo, SBResources resources, string pwName)
        {
            var wpoObj = wpo.sbObject;
            var packageString = pwName + "." + wpoObj.Package;
            var output = resources.GetResource(packageString + "." + wpoObj.Name);
            if (output == null)
            {
                //Debug.Log("Failed to match an SBResource to " + packageString + "." + wpoObj.Name);
            }
            return output;
        }

        #endregion

        /// <summary>
        /// Returns a corresponding MapIDs enum if the package is a map file
        /// Otherwise returns 0
        /// </summary>
        /// <returns></returns>
        protected MapIDs getCurMapID()
        {
            MapIDs id = (MapIDs)Enum.Parse(typeof(MapIDs), extractorWindowRef.ActiveWrapper.Name, true);
            return id;
        }


        #endregion
    }
}