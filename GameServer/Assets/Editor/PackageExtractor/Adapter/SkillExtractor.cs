#pragma warning disable 0414

using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Database.Static;
using Gameplay.Skills;
using Gameplay.Skills.Effects;
using Gameplay.Skills.Events;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class SkillExtractor : ExtractorAdapter
    {
        List<SkillEffectCollection> effectCollections = new List<SkillEffectCollection>();

        List<FSkill> extractedSkills = new List<FSkill>();

        Dictionary<string, Action<WrappedPackageObject, SBResources, SBLocalizedStrings>> extractorHandlers =
            new Dictionary<string, Action<WrappedPackageObject, SBResources, SBLocalizedStrings>>();

        List<ScriptableObject> objectsToSave = new List<ScriptableObject>();

        SkillCollection skillCollection;

        public override string Name
        {
            get { return "Skill Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract Skills into the provided Collection (which needs to be newly created!)"; }
        }

        public override bool IsCompatible(PackageWrapper p)
        {
            return base.IsCompatible(p) && p.Name.Contains("Skill");
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Skill collection asset:");
            skillCollection = EditorGUILayout.ObjectField(skillCollection, typeof (SkillCollection), false) as SkillCollection;
            GUILayout.Label("AVEffectCollections (for references):", EditorStyles.helpBox);
            var avCol = EditorGUILayout.ObjectField(null, typeof (SkillEffectCollection), false) as SkillEffectCollection;
            if (avCol != null && !effectCollections.Contains(avCol))
            {
                effectCollections.Add(avCol);
                var sPath = AssetDatabase.GetAssetPath(avCol);
                sPath = sPath.Replace(Path.GetFileName(sPath), string.Empty);
                sPath = sPath.Replace("Assets/Resources/", string.Empty);
                var sec = Resources.LoadAll<SkillEffectCollection>(sPath);
                for (var i = 0; i < sec.Length; i++)
                {
                    if (!effectCollections.Contains(sec[i]))
                    {
                        effectCollections.Add(sec[i]);
                    }
                }
            }
            for (var i = 0; i < effectCollections.Count; i++)
            {
                if (GUILayout.Button(effectCollections[i].name))
                {
                    effectCollections.RemoveAt(i);
                    break;
                }
            }
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (skillCollection == null)
            {
                Log("No collection assigned!", Color.yellow);
                return;
            }
            RegisterHandlers();
            extractedSkills.Clear();
            objectsToSave.Clear();
            var p = extractorWindowRef.ActiveWrapper;
            foreach (var wpo in p.IterateObjects())
            {
                Action<WrappedPackageObject, SBResources, SBLocalizedStrings> handler;
                if (extractorHandlers.TryGetValue(wpo.sbObject.ClassName.Replace("\0", string.Empty), out handler))
                {
                    handler(wpo, resources, localizedStrings);
                }
            }
            foreach (var so in objectsToSave)
            {
                if (so is FSkill && extractedSkills.Contains(so as FSkill))
                {
                    continue;
                }
                so.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                AssetDatabase.AddObjectToAsset(so, skillCollection);
            }
            foreach (var s in extractedSkills)
            {
                AssetDatabase.AddObjectToAsset(s, skillCollection);
                var res = resources.GetResource(p.Name, s.internalName);
                if (res != null)
                {
                    s.resourceID = res.ID;
                }
                else
                {
                    Log("ResourceID not found for: " + s.internalName, Color.cyan);
                }
                skillCollection.skills.Add(s);
            }
            EditorUtility.SetDirty(skillCollection);
        }

        void RegisterHandlers()
        {
            extractorHandlers.Clear();
            extractorHandlers.Add("SBGame.FSkill_Type", HandleExtractSkill);
            extractorHandlers.Add("SBGame.FSkill_Type_BodySlot", HandleExtractSkillBodySlot);
            extractorHandlers.Add("SBGame.FSkill_Type_Combo", HandleExtractSkillCombo);
            extractorHandlers.Add("SBGame.FSkill_Type_Consumable", HandleExtractSkillConsumable);
        }

        T Create<T>(WrappedPackageObject wpo, SBResources res) where T : FSkill
        {
            var obj = ScriptableObject.CreateInstance<T>();
            objectsToSave.Add(obj);
            obj.name = wpo.Name;
            obj.internalName = wpo.Name;
            var r = res.GetResource(extractorWindowRef.ActiveWrapper.Name, wpo.Name);
            if (res != null)
            {
                obj.resourceID = r.ID;
            }
            else
            {
                Log("ResourceID not found for: " + wpo.Name, Color.cyan);
            }
            return obj;
        }

        void HandleExtractSkill(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var s = Create<FSkill>(wpo, resources);
            ExtractBasicSkill(s, wpo, resources, strings);
            extractedSkills.Add(s);
        }

        void HandleExtractSkillBodySlot(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sb = Create<SkillBodySlot>(wpo, resources);
            ExtractBasicSkill(sb, wpo, resources, strings);
            ReadBool(wpo, "IsPlayerStartable", out sb.IsPlayerStartable);
            extractedSkills.Add(sb);
        }

        void HandleExtractSkillCombo(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sc = Create<SkillCombo>(wpo, resources);
            ExtractBasicSkill(sc, wpo, resources, strings);
            ReadEnum(wpo, "OpenerComboType", out sc.OpenerComboType);
            extractedSkills.Add(sc);
        }

        void HandleExtractSkillConsumable(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sc = Create<SkillConsumable>(wpo, resources);
            ExtractBasicSkill(sc, wpo, resources, strings);
            extractedSkills.Add(sc);
        }

        void ExtractBasicSkill<T>(T s, WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings) where T : FSkill
        {
            ReadString(wpo, "Group", out s.group, false);
            ReadEnum(wpo, "Category", out s.category, false);
            ReadFloat(wpo, "CooldownTime", out s.cooldown, false);
            ReadFloat(wpo, "AttackSpeed", out s.attackSpeed, false);
            ReadEnum(wpo, "AttackType", out s.attackType, false);
            ReadEnum(wpo, "MagicType", out s.magicType, false);
            ReadEnum(wpo, "LinkedAttribute", out s.linkedAttribute, false);
            ReadEnum(wpo, "SkillComboType", out s.skillComboType, false);
            SBProperty arrayProp;
            if (wpo.sbObject.Properties.TryGetValue("UsableByClass", out arrayProp))
            {
                foreach (var sbp in arrayProp.Array.Values)
                {
                    byte b;
                    if (TryParse(sbp.Value, out b))
                    {
                        s.usableByClass.Add((EContentClass) b);
                    }
                }
            }
            if (wpo.sbObject.Properties.TryGetValue("LegalSkillTokens", out arrayProp))
            {
                foreach (var sbp in arrayProp.Array.Values)
                {
                    s.temporaryLegalSkillTokens.Add(sbp.Value);
                }
            }
            ReadBool(wpo, "LegalSkillTokensUpdate", out s.legalSkillTokensUpdate, false);
            ReadInt(wpo, "MinSkillTier", out s.minSkillTier, false);
            ReadLocalizedString(wpo, "Name", strings, out s.skillname, false);
            ReadLocalizedString(wpo, "Description", strings, out s.description, false);
            ReadEnum(wpo, "Animation", out s.animation, false);
            ReadEnum(wpo, "Animation2", out s.animation2, false);
            ReadInt(wpo, "AnimationVariation", out s.animationVariation, false);
            ReadEnum(wpo, "RequiredWeapon", out s.requiredWeapon, false);
            ReadFloat(wpo, "AnimationSpeed", out s.animationSpeed, false);
            ReadFloat(wpo, "AnimationTweenTime", out s.animationTweenTime, false);
            ReadBool(wpo, "WeaponTracer", out s.weaponTracer, false);
            ReadBool(wpo, "FreezePawnMovement", out s.freezePawnMovement, false);
            ReadBool(wpo, "FreezePawnRotation", out s.freezePawnRotation, false);
            ReadFloat(wpo, "AnimationMovementForward", out s.animationMovementForward, false);
            ReadFloat(wpo, "AnimationMovementLeft", out s.animationMovementLeft, false);
            ReadBool(wpo, "PaintLocation", out s.paintLocation, false);
            ReadFloat(wpo, "PaintLocationMinDistance", out s.paintLocationMinDistance, false);
            ReadFloat(wpo, "PaintLocationMaxDistance", out s.paintLocationMaxDistance, false);
            ReadEnum(wpo, "SkillClassification", out s.classification);
            ReadEnum(wpo, "RequiredTarget", out s.requiredTarget, false);
            ReadFloat(wpo, "MinDistance", out s.minDistance, false);
            ReadFloat(wpo, "MaxDistance", out s.maxDistance, false);
            ReadFloat(wpo, "TargetDelay", out s.targetDelay, false);
            ReadFloat(wpo, "TargetCone", out s.targetCone, false);
            ReadEnum(wpo, "StackType", out s.stackType, false);
            ReadInt(wpo, "StackCount", out s.stackCount, false);
            ReadFloat(wpo, "LeetnessRating", out s.leetnessRating, false);
            ReadBool(wpo, "SkillRollsCombatBar", out s.skillRollsCombatBar, false);
            ReadBool(wpo, "SkillRequiresEquippedWeapon", out s.skillRequiresEquippedWeapon, false);
            s.keyFrames.AddRange(ExtractKeyFrames(wpo, resources, strings));
        }

        List<FSkill.SkillKeyFrame> ExtractKeyFrames(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var keyFrames = new List<FSkill.SkillKeyFrame>();
            SBProperty arrayProp;
            if (wpo.sbObject.Properties.TryGetValue("KeyFrames", out arrayProp))
            {
                foreach (var keyFrameProp in arrayProp.Array.Values)
                {
                    var kf = new FSkill.SkillKeyFrame();
                    kf.Name = keyFrameProp.StructName;
                    SBProperty partProp;
                    if (keyFrameProp.Array.TryGetValue("Time", out partProp))
                    {
                        TryParse(partProp.Value, out kf.Time);
                    }
                    if (keyFrameProp.Array.TryGetValue("Group", out partProp))
                    {
                        var group = FindReferencedObject(partProp);
                        if (group != null)
                        {
                            kf.EventGroup = ExtractSkillEventsFromGroup(group, resources, strings);
                            kf.EventGroup.name = group.Name;
                        }
                    }
                    keyFrames.Add(kf);
                }
            }
            return keyFrames;
        }

        SkillEventGroup ExtractSkillEventsFromGroup(WrappedPackageObject group, SBResources resources, SBLocalizedStrings strings)
        {
            var seg = ScriptableObject.CreateInstance<SkillEventGroup>();
            objectsToSave.Add(seg);
            SBProperty eventProp;
            if (group.sbObject.Properties.TryGetValue("Events", out eventProp))
            {
                foreach (var eventName in eventProp.IterateInnerProperties())
                {
                    var sEvent = FindReferencedObject(eventName);
                    if (sEvent == null)
                    {
                        //Log("SkillEvent reference not found! -"+ eventName.Name, Color.red);
                        continue;
                    }
                    var se = SelectAndExtractSkillEventByType(sEvent, resources, strings);
                    if (se != null)
                    {
                        seg.events.Add(se);
                    }
                }
            }
            return seg;
        }

        SkillEvent SelectAndExtractSkillEventByType(WrappedPackageObject eventObj, SBResources resources, SBLocalizedStrings strings)
        {
            SkillEvent se;
            switch (eventObj.sbObject.ClassName.Replace("\0", string.Empty))
            {
                case "SBGame.FSkill_Event":
                    se = CreateReadEvent<SkillEvent>(eventObj);
                    break;
                case "SBGame.FSkill_Event_BodySlot":
                    se = ExtractBodySlotEvent(eventObj, resources, strings);
                    break;
                case "SBGame.FSkill_Event_Chain":
                    se = ExtractEventChain(eventObj, resources, strings);
                    break;
                case "SBGame.FSkill_Event_Direct":
                    se = CreateReadDirectEvent<SkillEventDirect>(eventObj, resources, strings);
                    break;
                case "SBGame.FSkill_Event_DirectAdvanced":
                    se = ExtractDirectAdvancedEvent(eventObj, resources, strings);
                    break;
                case "SBGame.FSkill_Event_Duff":
                    se = CreateReadDuffEvent<SkillEventDuff>(eventObj, resources, strings);
                    break;
                //case "SBGame.FSkill_Event_Duff_CondEv":
                //    se = null; //handled separately
                //    break;
                //case "SBGame.FSkill_Event_Duff_DirectEff":
                //    se = null; //handled separately
                //    break
                //case "SBGame.FSkill_Event_Duff_DuffEff":
                //    return null; //handled separately
                case "SBGame.FSkill_Event_FX":
                    se = CreateReadFXEvent<SkillEventFX>(eventObj, resources);
                    break;
                case "SBGame.FSkill_Event_FX_Advanced":
                    se = ExtractFXAdvancedEvent(eventObj, resources, strings);
                    break;
                case "SBGame.FSkill_Event_Summon":
                    se = ExtractSummonEvent(eventObj, resources, strings);
                    break;
                case "SBGame.FSkill_Event_Target":
                    se = CreateReadTargetEvent<SkillEventTarget>(eventObj, resources, strings);
                    break;
                default:
                    if (eventObj.Name.Replace("\0", string.Empty).Contains("FSkill_Event"))
                    {
                        Debug.LogWarning(eventObj.Name.Replace("\0", string.Empty) + " not parseable(?)!");
                    }
                    se = null;
                    break;
            }
            if (se != null)
            {
                var eventName = string.Format("{0}.{1}.{2}", extractorWindowRef.ActiveWrapper.Name, eventObj.sbObject.Package, eventObj.Name);
                //se.resourceID = resources.GetResourceID(extractorWindowRef.ActiveWrapper.Name, eventObj.sbObject.Package, eventObj.Name);
                se.resourceID = resources.GetResourceID(eventName);
            }
            return se;
        }

        SkillEventDuffDirectEff ExtractDuffDirectEff(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sede = ScriptableObject.CreateInstance<SkillEventDuffDirectEff>();
            sede.internalName = wpo.Name;
            sede.name = wpo.Name;
            sede.resourceID = resources.GetResourceID(extractorWindowRef.ActiveWrapper.Name, wpo.sbObject.Package, wpo.Name);
            objectsToSave.Add(sede);
            TryAssignFXField(wpo, "effect", out sede.effect);
            ReadFloat(wpo, "Interval", out sede.Interval);
            ReadFloat(wpo, "Delay", out sede.Delay);
            ReadInt(wpo, "RepeatCount", out sede.RepeatCount);
            var fxObj = FindReferencedObject(wpo, "ExecuteFXEvent");
            if (fxObj != null)
            {
                sede.ExecuteFXEvent = SelectAndExtractSkillEventByType(fxObj, resources, strings) as SkillEventFX;
            }
            return sede;
        }

        SkillEventDuffDuffEff ExtractDuffDuffEff(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedd = ScriptableObject.CreateInstance<SkillEventDuffDuffEff>();
            sedd.internalName = wpo.Name;
            sedd.name = wpo.Name;
            sedd.resourceID = resources.GetResourceID(extractorWindowRef.ActiveWrapper.Name, wpo.sbObject.Package, wpo.Name);
            objectsToSave.Add(sedd);
            TryAssignFXField(wpo, "Effect", out sedd.effect);
            ReadFloat(wpo, "Interval", out sedd.Interval);
            ReadFloat(wpo, "Delay", out sedd.Delay);
            ReadInt(wpo, "RepeatCount", out sedd.RepeatCount);
            var fxObj = FindReferencedObject(wpo, "ExecuteFXEvent");
            if (fxObj != null)
            {
                sedd.ExecuteFXEvent = SelectAndExtractSkillEventByType(fxObj, resources, strings) as SkillEventFX;
            }
            return sedd;
        }

        SkillEventDuffCondEv ExtractDuffCondEv(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sede = ScriptableObject.CreateInstance<SkillEventDuffCondEv>();
            sede.internalName = wpo.Name;
            sede.name = wpo.Name;
            sede.resourceID = resources.GetResourceID(extractorWindowRef.ActiveWrapper.Name, wpo.sbObject.Package, wpo.Name);
            objectsToSave.Add(sede);
            var eventObj = FindReferencedObject(wpo, "Event");
            if (eventObj != null)
            {
                sede.Event = SelectAndExtractSkillEventByType(eventObj, resources, strings);
            }
            ReadInt(wpo, "Uses", out sede.Uses);
            ReadInt(wpo, "MaximumTriggersPerUse", out sede.MaximumTriggersPerUse);
            ReadFloat(wpo, "Interval", out sede.Interval);
            ReadFloat(wpo, "Delay", out sede.Delay);
            ReadFloat(wpo, "IncreasePerUse", out sede.IncreasePerUse);
            ReadEnum(wpo, "Condition", out sede.Condition);
            ReadEnum(wpo, "AttackType", out sede.AttackType);
            ReadEnum(wpo, "MagicType", out sede.MagicType);
            ReadEnum(wpo, "EffectType", out sede.EffectType);
            ReadEnum(wpo, "Target", out sede.Target);
            return sede;
        }

        T CreateReadEvent<T>(WrappedPackageObject wpo) where T : SkillEvent
        {
            var obj = ScriptableObject.CreateInstance<T>();
            objectsToSave.Add(obj);
            obj.name = wpo.Name;
            obj.internalName = wpo.Name;
            ReadFloat(wpo, "Delay", out obj.Delay);
            ReadBool(wpo, "TargetCountValueSpecifier", out obj.TargetCountValueSpecifier);
            ReadBasicArray(wpo, "PerEffectFameLevelBonus", out obj.PerEffectFameLevelBonus);
            ReadBasicArray(wpo, "PerEffectPepLevelBonus", out obj.PerEffectPepLevelBonus);
            return obj;
        }

        T FindEffectReference<T>(string FXName) where T : SkillEffect
        {
            for (var i = 0; i < effectCollections.Count; i++)
            {
                var avs = effectCollections[i].GetEffect(FXName) as T;
                if (avs != null)
                {
                    return avs;
                }
            }
            return null;
        }

        void TryAssignFXField<T>(WrappedPackageObject wpo, string fieldName, out T field) where T : SkillEffect
        {
            SBProperty fieldProp;
            if (wpo.sbObject.Properties.TryGetValue(fieldName, out fieldProp))
            {
                field = FindEffectReference<T>(fieldProp.Value);
                return;
            }
            field = null;
        }

        SkillEventBodySlot ExtractBodySlotEvent(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seb = CreateReadTargetEvent<SkillEventBodySlot>(wpo, resources, strings);
            return seb;
        }

        T CreateReadFXEvent<T>(WrappedPackageObject wpo, SBResources resources) where T : SkillEventFX
        {
            var obj = CreateReadEvent<T>(wpo);
            SBProperty fxProp;
            if (wpo.sbObject.Properties.TryGetValue("FX", out fxProp))
            {
                var clientFX = new SkillEventFX.Client_FX();
                SBProperty fieldProp;
                if (fxProp.Array.TryGetValue("Sound", out fieldProp))
                {
                    clientFX.Sound = FindEffectReference<AudioVisualSkillEffect>(fieldProp.Value.Replace("\0", string.Empty));
                }
                if (fxProp.Array.TryGetValue("CameraShake", out fieldProp))
                {
                    clientFX.CameraShake = FindEffectReference<AudioVisualSkillEffect>(fieldProp.Value.Replace("\0", string.Empty));
                }
                if (fxProp.Array.TryGetValue("Emitter", out fieldProp))
                {
                    clientFX.Emitter = FindEffectReference<AudioVisualSkillEffect>(fieldProp.Value.Replace("\0", string.Empty));
                }
                obj.FX = clientFX;
            }
            ReadEnum(wpo, "EmitterLocation", out obj.EmitterLocation);
            ReadBool(wpo, "ComboFinisherOnlyFX", out obj.ComboFinisherOnlyFX);
            return obj;
        }

        T CreateReadTargetEvent<T>(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings) where T : SkillEventTarget
        {
            var obj = CreateReadFXEvent<T>(wpo, resources);
            ReadInt(wpo, "MaxTargets", out obj.MaxTargets);
            ReadEnum(wpo, "TargetBase", out obj.TargetBase);
            ReadEnum(wpo, "TargetSelf", out obj.TargetSelf);
            ReadEnum(wpo, "TargetEnemies", out obj.TargetEnemies);
            ReadEnum(wpo, "TargetFriendlies", out obj.TargetFriendlies);
            ReadEnum(wpo, "TargetNeutrals", out obj.TargetNeutrals);
            ReadEnum(wpo, "TargetSpirits", out obj.TargetSpirits);
            ReadEnum(wpo, "TargetBloodlinks", out obj.TargetBloodlinks);
            ReadEnum(wpo, "TargetPartyMembers", out obj.TargetPartyMembers);
            ReadEnum(wpo, "TargetGuildMembers", out obj.TargetGuildMembers);
            SBProperty arrayProp;
            if (wpo.sbObject.Properties.TryGetValue("LimitToTaxonomy", out arrayProp))
            {
                foreach (var taxField in arrayProp.IterateInnerProperties())
                {
                    obj.temporaryLimitToTaxonomy.Add(taxField.Value.Replace("\0", string.Empty));
                }
            }
            ReadBool(wpo, "TargetAttached", out obj.TargetAttached, false);
            return obj;
        }

        SkillEventChain ExtractEventChain(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sec = CreateReadTargetEvent<SkillEventChain>(wpo, resources, strings);
            ReadInt(wpo, "MaxJumps", out sec.MaxJumps);
            ReadInt(wpo, "TargetsPerJump", out sec.TargetsPerJump);
            ReadInt(wpo, "MaxHitsPerTarget", out sec.MaxHitsPerTarget);
            TryAssignFXField(wpo, "Range", out sec.Range);
            ReadFloat(wpo, "Interval", out sec.Interval);
            ReadFloat(wpo, "IncreasePerJump", out sec.IncreasePerJump);
            ReadBool(wpo, "FairDistribution", out sec.FairDistribution);
            ReadBool(wpo, "TargetsPerJumpIsPerTarget", out sec.TargetsPerJumpIsPerTarget);
            var linkedEvent = FindReferencedObject(wpo, "Event");
            if (linkedEvent != null)
            {
                sec.Event = SelectAndExtractSkillEventByType(linkedEvent, resources, strings); //TODO: check for possible infinite recursion
            }
            else
            {
                Log("Linked Event from Chain not found!", Color.red);
            }
            ReadInt(wpo, "TargetHitMap", out sec.TargetHitMap, false);
            ReadInt(wpo, "TargetHitSet", out sec.TargetHitSet, false);
            return sec;
        }

        T CreateReadDirectEvent<T>(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings) where T : SkillEventDirect
        {
            var obj = CreateReadTargetEvent<T>(wpo, resources, strings);
            ReadInt(wpo, "RepeatCount", out obj.RepeatCount);
            ReadFloat(wpo, "Interval", out obj.Interval);
            ReadBool(wpo, "KeepTargets", out obj.KeepTargets);
            ReadInt(wpo, "TargetsPerRepeat", out obj.TargetsPerRepeat);
            TryAssignFXField(wpo, "Range", out obj.Range);
            TryAssignFXField(wpo, "Damage", out obj.Damage);
            TryAssignFXField(wpo, "Heal", out obj.Heal);
            TryAssignFXField(wpo, "_State", out obj._State);
            TryAssignFXField(wpo, "Drain", out obj.Drain);
            SBProperty fieldProp;
            if (wpo.sbObject.Properties.TryGetValue("Buff", out fieldProp))
            {
                var buffRef = FindReferencedObject(fieldProp);
                if (buffRef != null)
                {
                    obj.Buff = SelectAndExtractSkillEventByType(buffRef, resources, strings) as SkillEventDuff;
                }
            }
            if (wpo.sbObject.Properties.TryGetValue("Debuff", out fieldProp))
            {
                var debuffRef = FindReferencedObject(fieldProp);
                if (debuffRef != null)
                {
                    obj.Debuff = SelectAndExtractSkillEventByType(debuffRef, resources, strings) as SkillEventDuff;
                }
            }
            TryAssignFXField(wpo, "Teleport", out obj.Teleport);
            TryAssignFXField(wpo, "FireBodySlot", out obj.FireBodySlot);
            TryAssignFXField(wpo, "ShapeShift", out obj.ShapeShift);
            ReadFloat(wpo, "DamageMoraleBonus", out obj.DamageMoraleBonus);
            ReadBool(wpo, "PlayHurtSound", out obj.PlayHurtSound);
            ReadBool(wpo, "RepeatTargetFX", out obj.RepeatTargetFX);
            var missfxObj = FindReferencedObject(wpo, "MissFXEvent");
            if (missfxObj != null)
            {
                obj.MissFXEvent = SelectAndExtractSkillEventByType(missfxObj, resources, strings) as SkillEventFX;
            }
            var hitFXObj = FindReferencedObject(wpo, "HitFXEvent");
            if (hitFXObj != null)
            {
                obj.HitFXEvent = SelectAndExtractSkillEventByType(hitFXObj, resources, strings) as SkillEventFX;
            }
            return obj;
        }

        SkillEventDirectAdvanced ExtractDirectAdvancedEvent(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seda = CreateReadDirectEvent<SkillEventDirectAdvanced>(wpo, resources, strings);
            var missObj = FindReferencedObject(wpo, "MissEvent");
            if (missObj != null)
            {
                seda.MissEvent = SelectAndExtractSkillEventByType(missObj, resources, strings);
            }
            var reactObj = FindReferencedObject(wpo, "ReactionEvent");
            if (reactObj != null)
            {
                seda.ReactionEvent = SelectAndExtractSkillEventByType(reactObj, resources, strings);
            }
            var triggerObj = FindReferencedObject(wpo, "TriggerEvent");
            if (triggerObj != null)
            {
                seda.TriggerEvent = SelectAndExtractSkillEventByType(triggerObj, resources, strings);
            }
            return seda;
        }

        T CreateReadDuffEvent<T>(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings) where T : SkillEventDuff
        {
            var obj = CreateReadFXEvent<T>(wpo, resources);
            SBProperty effProp;
            if (wpo.sbObject.Properties.TryGetValue("DirectEffects", out effProp))
            {
                foreach (var dEff in effProp.IterateInnerProperties())
                {
                    var effect = FindReferencedObject(dEff);
                    if (effect != null)
                    {
                        obj.DirectEffects.Add(ExtractDuffDirectEff(effect, resources, strings));
                    }
                }
            }
            if (wpo.sbObject.Properties.TryGetValue("DuffEffects", out effProp))
            {
                foreach (var dd in effProp.IterateInnerProperties())
                {
                    var duff = FindReferencedObject(dd);
                    if (duff != null)
                    {
                        obj.DuffEffects.Add(ExtractDuffDuffEff(duff, resources, strings));
                    }
                }
            }
            var eventObj = FindReferencedObject(wpo, "Event");
            if (eventObj != null)
            {
                obj.Event = SelectAndExtractSkillEventByType(eventObj, resources, strings);
            }
            ReadFloat(wpo, "EventInterval", out obj.EventInterval);
            ReadInt(wpo, "EventRepeatCount", out obj.EventRepeatCount);
            SBProperty duffCondEvArray;
            if (wpo.sbObject.Properties.TryGetValue("ConditionalEvents", out duffCondEvArray))
            {
                foreach (var condEvProp in duffCondEvArray.IterateInnerProperties())
                {
                    var condEffObjRef = FindReferencedObject(condEvProp);
                    if (condEffObjRef != null)
                    {
                        var scd = ExtractDuffCondEv(condEffObjRef, resources, strings);
                        if (scd != null)
                        {
                            obj.ConditionalEvents.Add(scd);
                        }
                        else
                        {
                            Log(condEffObjRef.Name + " could not be extracted!", Color.red);
                        }
                    }
                }
            }
            ReadLocalizedString(wpo, "Name", strings, out obj.Name);
            ReadBool(wpo, "Visible", out obj.Visible);
            ReadLocalizedString(wpo, "Description", strings, out obj.Description);
            ReadFloat(wpo, "Duration", out obj.Duration);
            ReadEnum(wpo, "StackType", out obj.StackType);
            ReadInt(wpo, "StackCount", out obj.StackCount);
            ReadEnum(wpo, "Priority", out obj.Priority);
            ReadBool(wpo, "RunUntilAbort", out obj.RunUntilAbort);
            return obj;
        }

        SkillEventFXAdvanced ExtractFXAdvancedEvent(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sefa = CreateReadFXEvent<SkillEventFXAdvanced>(wpo, resources);
            SBProperty emitterArrayProp;
            if (wpo.sbObject.Properties.TryGetValue("Emitters", out emitterArrayProp))
            {
                foreach (var emitterStructsProp in emitterArrayProp.IterateInnerProperties())
                {
                    var ae = new SkillEventFXAdvanced.AdvancedEmitter();
                    SBProperty fieldRef;
                    if (emitterStructsProp.Array.TryGetValue("Emitter", out fieldRef))
                    {
                        ae.Emitter = FindEffectReference<AudioVisualSkillEffect>(fieldRef.Value.Replace("\0", string.Empty));
                    }
                    if (emitterStructsProp.Array.TryGetValue("Delay", out fieldRef))
                    {
                        TryParse(fieldRef.Value.Replace("\0", string.Empty), out ae.Delay);
                    }
                    if (emitterStructsProp.Array.TryGetValue("Location", out fieldRef))
                    {
                        byte loc;
                        TryParse(fieldRef.Value.Replace("\0", string.Empty), out loc);
                        ae.Location = (EEmitterOverwrite) loc;
                    }
                    sefa.Emitters.Add(ae);
                }
            }
            return sefa;
        }

        SkillEventSummon ExtractSummonEvent(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var ses = CreateReadFXEvent<SkillEventSummon>(wpo, resources);
            SBProperty summonProp;
            if (wpo.sbObject.Properties.TryGetValue("SummonEmitter", out summonProp))
            {
                ses.SummonEmitter = FindEffectReference<AudioVisualSkillEffect>(summonProp.Value.Replace("\0", string.Empty));
            }
            ReadString(wpo, "NPC", out ses.temporaryNPCName);
            ReadBool(wpo, "SpawnedPet", out ses.SpawnedPet);
            return ses;
        }
    }
}