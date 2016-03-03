using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Database.Static;
using Gameplay.Skills;
using Gameplay.Skills.Effects;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class GameplayEffectExtractor : ExtractorAdapter
    {
        List<SkillEffectCollection> avcollections = new List<SkillEffectCollection>();

        List<SkillEffect> extractedEffects = new List<SkillEffect>();

        Dictionary<string, Action<WrappedPackageObject, SBResources, SBLocalizedStrings>> extractorHandlers =
            new Dictionary<string, Action<WrappedPackageObject, SBResources, SBLocalizedStrings>>();

        SkillEffectCollection GameplayfxCollection;

        public override string Name
        {
            get { return "Gameplay Effect Extractor"; }
        }

        public override string Description
        {
            get
            {
                return
                    "Tries to extract the Gameplay related Effects (Damage, Duffs etc) into the provided collection and link existing objects, the collection needs to be newly created!";
            }
        }

        public override bool IsCompatible(PackageWrapper p)
        {
            return base.IsCompatible(p) && p.Name.Contains("Effects") && p.Name.Contains("GP");
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Effect collection asset:");
            GameplayfxCollection = EditorGUILayout.ObjectField(GameplayfxCollection, typeof (SkillEffectCollection), false) as SkillEffectCollection;
            GUILayout.Label("AVEffectCollections (for references):", EditorStyles.helpBox);
            var avCol = EditorGUILayout.ObjectField(null, typeof (SkillEffectCollection), false) as SkillEffectCollection;
            if (avCol != null && !avcollections.Contains(avCol) && avCol.name.Contains("AVGP"))
            {
                avcollections.Add(avCol);
                var sPath = AssetDatabase.GetAssetPath(avCol);
                sPath = sPath.Replace(Path.GetFileName(sPath), string.Empty);
                sPath = sPath.Replace("Assets/Resources/", string.Empty);
                var sec = Resources.LoadAll<SkillEffectCollection>(sPath);
                for (var i = 0; i < sec.Length; i++)
                {
                    if (!avcollections.Contains(sec[i]) && sec[i].name.Contains("AVGP"))
                    {
                        avcollections.Add(sec[i]);
                    }
                }
            }
            for (var i = 0; i < avcollections.Count; i++)
            {
                if (GUILayout.Button(avcollections[i].name))
                {
                    avcollections.RemoveAt(i);
                    break;
                }
            }
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (GameplayfxCollection == null)
            {
                Log("No collection assigned!", Color.yellow);
                return;
            }
            RegisterHandlers();
            var p = extractorWindowRef.ActiveWrapper;
            foreach (var wpo in p.IterateObjects())
            {
                Action<WrappedPackageObject, SBResources, SBLocalizedStrings> handler;
                if (extractorHandlers.TryGetValue(wpo.sbObject.ClassName.Replace("\0", string.Empty), out handler))
                {
                    handler(wpo, resources, localizedStrings);
                }
            }
            foreach (var se in extractedEffects)
            {
                se.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                AssetDatabase.AddObjectToAsset(se, GameplayfxCollection);
                GameplayfxCollection.effects.Add(se);
            }
            EditorUtility.SetDirty(GameplayfxCollection);
        }

        void RegisterHandlers()
        {
            extractorHandlers.Clear();
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DirectDamage", ExtractDirectDamage);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DirectDrain", ExtractDirectDrain);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DirectFireBodySlot", ExtractDirectFireBodySlot); //TODO: no values to read?
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DirectHeal", ExtractDirectHeal);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DirectState", ExtractDirectState);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DirectShapeShift", ExtractDirectShapeShift);
            extractorHandlers.Add("SBAI.FSkill_EffectClass_DirectTeleportAI", ExtractDirectTeleport);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffAffinity", ExtractDuffAffinity);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffAlterEffectInOutput", ExtractDuffAlterEffectInOutput);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffAttackSpeed", ExtractDuffAttackSpeed);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffAttribute", ExtractDuffAttribute);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffDegeneration", ExtractDuffDegeneration);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffDirectionalDamage", ExtractDuffDirectionalDamage);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffDisableSkillUse", ExtractDuffDisableSkillUse);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffDrain", ExtractDuffDrain);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffFreeze", ExtractDuffFreeze);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffGfx", ExtractDuffGfx);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffImmunity", ExtractDuffImmunity);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffMaxHealth", ExtractDuffMaxHealth);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffMovementSpeed", ExtractDuffMovementSpeed);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffPePRank", ExtractDuffPePRank);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffRegeneration", ExtractDuffRegeneration);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffResistance", ExtractDuffResistance);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffReturnReflect", ExtractDuffReturnReflect);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffShare", ExtractDuffShare);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_DuffState", ExtractDuffState);
            extractorHandlers.Add("SBGame.FSkill_EffectClass_Range", ExtractRange);
        }

        T Create<T>(WrappedPackageObject wpo, SBResources res) where T : SkillEffect
        {
            var obj = ScriptableObject.CreateInstance<T>();
            obj.name = wpo.Name;
            obj.referenceName = wpo.Name;
            var r = res.GetResource(extractorWindowRef.ActiveWrapper.Name, wpo.Name);
            if (res != null)
            {
                obj.resourceID = r.ID;
            }
            else
            {
                Log("ResourceID not found for: " + wpo.Name, Color.cyan);
            }
            var sduff = obj as SkillEffectDuff;
            if (sduff)
            {
                ReadFloat(wpo, "ComboEffectDuration", out sduff.comboEffectDuration);
            }
            return obj;
        }

        void ExtractDirectDamage(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedd = Create<SkillEffectDirectDamage>(wpo, resources);
            sedd.damage = ExtractValueSpecifier(FindReferencedObject(wpo, "Damage"));
            ReadBool(wpo, "IgnoreResist", out sedd.ignoreResist, false);
            ReadFloat(wpo, "RearIncrease", out sedd.rearIncrease, false);
            ReadVector3(wpo, "Momentum", out sedd.momentum, false);
            ReadFloat(wpo, "AggroMultiplier", out sedd.aggroMultiplier, false);
            extractedEffects.Add(sedd);
        }

        void ExtractDirectDrain(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedd = Create<SkillEffectDirectDrain>(wpo, resources);
            ReadEnum(wpo, "DrainedCharacterStat", out sedd.drainedCharacterStat, false);
            ReadEnum(wpo, "GainedCharacterStat", out sedd.gainedCharacterStat, false);
            sedd.drainedAmount = ExtractValueSpecifier(FindReferencedObject(wpo, "DrainedAmount"));
            ReadFloat(wpo, "Multiplier", out sedd.multiplier, false);
            var multiplierVS = FindReferencedObject(wpo, "MultiplierVS");
            if (multiplierVS != null)
            {
                sedd.multiplierVS = ExtractValueSpecifier(multiplierVS);
            }
            extractedEffects.Add(sedd);
        }

        void ExtractDirectState(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seds = Create<SkillEffectDirectState>(wpo, resources);
            ReadEnum(wpo, "State", out seds.state);
            var valueObj = FindReferencedObject(wpo, "Value");
            if (valueObj != null)
            {
                seds.value = ExtractValueSpecifier(valueObj);
            }
            extractedEffects.Add(seds);
        }

        void ExtractDirectFireBodySlot(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedfb = Create<SkillEffectDirectFireBodySlot>(wpo, resources);
            extractedEffects.Add(sedfb);
        }

        void ExtractDirectHeal(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedh = Create<SkillEffectDirectHeal>(wpo, resources);
            sedh.heal = ExtractValueSpecifier(FindReferencedObject(wpo, "Heal"));
            ReadFloat(wpo, "AggroMultiplier", out sedh.aggroMultiplier, false);
            extractedEffects.Add(sedh);
        }

        void ExtractDirectShapeShift(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seds = Create<SkillEffectDirectShapeShift>(wpo, resources);
            ReadString(wpo, "Shape", out seds.temporaryShapeName);
            seds.shapeShiftValue = ExtractValueSpecifier(FindReferencedObject(wpo, "_ShapeShiftValue"));
            extractedEffects.Add(seds);
        }

        void ExtractDirectTeleport(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedt = Create<SkillEffectDirectTeleport>(wpo, resources);
            ReadEnum(wpo, "Mode", out sedt.mode);
            ReadEnum(wpo, "Rotation", out sedt.rotation);
            ReadFloat(wpo, "Offset", out sedt.offset);
            sedt.teleportValue = ExtractValueSpecifier(FindReferencedObject(wpo, "_TeleportValue"));
            extractedEffects.Add(sedt);
        }

        void ExtractDuffAffinity(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seda = Create<SkillEffectDuffAffinity>(wpo, resources);
            ReadEnum(wpo, "MagicType", out seda.magicType);
            seda.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(seda);
        }

        void ExtractDuffAlterEffectInOutput(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedae = Create<SkillEffectDuffAlterEffectInOutput>(wpo, resources);
            ReadEnum(wpo, "Mode", out sedae.mode);
            ReadEnum(wpo, "AttackType", out sedae.attackType);
            ReadEnum(wpo, "MagicType", out sedae.magicType);
            ReadEnum(wpo, "EffectType", out sedae.effectType);
            sedae.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            ReadEnum(wpo, "ValueMode", out sedae.valueMode);
            ReadBool(wpo, "IgnoreMultiplier", out sedae.ignoreMultiplier);
            ReadFloat(wpo, "UseInterval", out sedae.useInterval);
            ReadInt(wpo, "Uses", out sedae.uses);
            ReadFloat(wpo, "IncreasePeruse", out sedae.increasePerUse);
            sedae.alterEffectValue = ExtractValueSpecifier(FindReferencedObject(wpo, "_AlterEffectValue"));
            extractedEffects.Add(sedae);
        }

        void ExtractDuffAttackSpeed(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedas = Create<SkillEffectDuffAttackSpeed>(wpo, resources);
            sedas.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(sedas);
        }

        void ExtractDuffAttribute(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seda = Create<SkillEffectDuffAttribute>(wpo, resources);
            ReadEnum(wpo, "Attribute", out seda.attribute);
            seda.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(seda);
        }

        void ExtractDuffDegeneration(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedd = Create<SkillEffectDuffDegeneration>(wpo, resources);
            sedd.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(sedd);
        }

        void ExtractDuffDirectionalDamage(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedd = Create<SkillEffectDuffDirectionalDamage>(wpo, resources);
            ReadEnum(wpo, "Mode", out sedd.mode);
            sedd.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(sedd);
        }

        void ExtractDuffDisableSkillUse(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seds = Create<SkillEffectDuffDisableSkillUse>(wpo, resources);
            ReadEnum(wpo, "ByAttackType", out seds.byAttackType);
            ReadEnum(wpo, "ByMagicType", out seds.byMagicType);
            seds.disableSkillUseValue = ExtractValueSpecifier(FindReferencedObject(wpo, "DisableSkillUseValue"));
            extractedEffects.Add(seds);
        }

        void ExtractDuffDrain(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedd = Create<SkillEffectDuffDrain>(wpo, resources);
            ReadEnum(wpo, "DrainedCharacterStat", out sedd.drainedCharacterStat);
            ReadEnum(wpo, "GainedCharacterStat", out sedd.gainedCharacterStat);
            sedd.drainedAmount = ExtractValueSpecifier(FindReferencedObject(wpo, "DrainedAmount"));
            ReadFloat(wpo, "Multiplier", out sedd.multiplier);
            sedd.multiplierVS = ExtractValueSpecifier(FindReferencedObject(wpo, "MultiplierVS"));
            extractedEffects.Add(sedd);
        }

        void ExtractDuffFreeze(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedf = Create<SkillEffectDuffFreeze>(wpo, resources);
            ReadBool(wpo, "Movement", out sedf.movement);
            ReadBool(wpo, "Rotation", out sedf.rotation);
            ReadBool(wpo, "Animation", out sedf.animation);
            sedf.freezeValue = ExtractValueSpecifier(FindReferencedObject(wpo, "_FreezeValue"));
            extractedEffects.Add(sedf);
        }

        void ExtractDuffGfx(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedg = Create<SkillEffectDuffGfx>(wpo, resources);
            ReadString(wpo, "TargetFX", out sedg.temporaryTargetFXName);
            var avs = FindEffectReference(sedg.temporaryTargetFXName);
            if (avs != null)
            {
                sedg.targetFX = avs;
            }
            extractedEffects.Add(sedg);
        }

        void ExtractDuffImmunity(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedi = Create<SkillEffectDuffImmunity>(wpo, resources);
            ReadEnum(wpo, "ByAttackType", out sedi.byAttackType);
            ReadEnum(wpo, "ByMagicType", out sedi.byMagicType);
            ReadEnum(wpo, "ByEffectType", out sedi.byEffectType);
            ReadString(wpo, "SourceFX", out sedi.temporarySourceFXName);
            var avs = FindEffectReference(sedi.temporarySourceFXName);
            if (avs != null)
            {
                sedi.sourceFX = avs;
            }
            sedi.immunityValue = ExtractValueSpecifier(FindReferencedObject(wpo, "_ImmunityValue"));
            extractedEffects.Add(sedi);
        }

        void ExtractDuffMaxHealth(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedm = Create<SkillEffectDuffMaxHealth>(wpo, resources);
            sedm.addedValue = ExtractValueSpecifier(FindReferencedObject(wpo, "AddedValue"));
            extractedEffects.Add(sedm);
        }

        void ExtractDuffMovementSpeed(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedm = Create<SkillEffectDuffMovementSpeed>(wpo, resources);
            sedm.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(sedm);
        }

        void ExtractDuffPePRank(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedp = Create<SkillEffectDuffPePRank>(wpo, resources);
            sedp.rankChange = ExtractValueSpecifier(FindReferencedObject(wpo, "RankChange"));
            extractedEffects.Add(sedp);
        }

        void ExtractDuffRegeneration(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedr = Create<SkillEffectDuffRegeneration>(wpo, resources);
            ReadEnum(wpo, "State", out sedr.state);
            sedr.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(sedr);
        }

        void ExtractDuffResistance(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedr = Create<SkillEffectDuffResistance>(wpo, resources);
            ReadEnum(wpo, "AttackType", out sedr.attackType);
            sedr.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(sedr);
        }

        void ExtractDuffReturnReflect(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var sedr = Create<SkillEffectDuffReturnReflect>(wpo, resources);
            ReadEnum(wpo, "Mode", out sedr.mode);
            ReadEnum(wpo, "AttackType", out sedr.attackType);
            ReadEnum(wpo, "MagicType", out sedr.magicType);
            ReadEnum(wpo, "EffectType", out sedr.effectType);
            ReadFloat(wpo, "Multiplier", out sedr.multiplier);
            ReadFloat(wpo, "UseInterval", out sedr.useInterval);
            ReadInt(wpo, "Uses", out sedr.uses);
            ReadFloat(wpo, "IncreasePerUse", out sedr.increasePerUse);
            ReadString(wpo, "SourceFX", out sedr.temporarySourceFXName);
            var avs = FindEffectReference(sedr.temporarySourceFXName);
            if (avs != null)
            {
                sedr.sourceFX = avs;
            }
            ReadString(wpo, "TargetFX", out sedr.temporaryTargetFXName);
            avs = FindEffectReference(sedr.temporaryTargetFXName);
            if (avs != null)
            {
                sedr.targetFX = avs;
            }
            sedr.returnReflectValue = ExtractValueSpecifier(FindReferencedObject(wpo, "_ReturnReflectValue"));
            extractedEffects.Add(sedr);
        }

        void ExtractDuffShare(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seds = Create<SkillEffectDuffShare>(wpo, resources);
            ReadEnum(wpo, "EffectType", out seds.effectType);
            ReadEnum(wpo, "AttackType", out seds.attackType);
            ReadEnum(wpo, "MagicType", out seds.magicType);
            ReadEnum(wpo, "Mode", out seds.mode);
            ReadFloat(wpo, "ShareRatio", out seds.shareRatio);
            ReadEnum(wpo, "Type", out seds.type);
            ReadBool(wpo, "IsBloodLink", out seds.isBloodLink);
            ReadFloat(wpo, "UseInterval", out seds.useInterval);
            ReadInt(wpo, "Uses", out seds.uses);
            ReadFloat(wpo, "IncreasePerUse", out seds.increasePerUse);
            ReadString(wpo, "SourceFX", out seds.temporarySourceFXName);
            var avs = FindEffectReference(seds.temporarySourceFXName);
            if (avs != null)
            {
                seds.sourceFX = avs;
            }
            ReadString(wpo, "TargetFX", out seds.temporaryTargetFXName);
            avs = FindEffectReference(seds.temporaryTargetFXName);
            if (avs != null)
            {
                seds.targetFX = avs;
            }
            ReadString(wpo, "Description", out seds.description);
            seds.shareValue = ExtractValueSpecifier(FindReferencedObject(wpo, "_ShareValue"));
            extractedEffects.Add(seds);
        }

        void ExtractDuffState(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var seds = Create<SkillEffectDuffState>(wpo, resources);
            ReadEnum(wpo, "Attribute", out seds.attribute);
            seds.value = ExtractValueSpecifier(FindReferencedObject(wpo, "Value"));
            extractedEffects.Add(seds);
        }

        void ExtractRange(WrappedPackageObject wpo, SBResources resources, SBLocalizedStrings strings)
        {
            var ser = Create<SkillEffectRange>(wpo, resources);
            ReadVector3(wpo, "LocationOffset", out ser.locationOffset);
            ReadInt(wpo, "RotationOffset", out ser.rotationOffset);
            ReadFloat(wpo, "Angle", out ser.angle);
            ReadFloat(wpo, "MinRadius", out ser.minRadius);
            ReadFloat(wpo, "MaxRadius", out ser.maxRadius);
            ReadEnum(wpo, "SortingMethod", out ser.sortingMethod);
            extractedEffects.Add(ser);
        }

        ValueSpecifier ExtractValueSpecifier(WrappedPackageObject valueSpec)
        {
            var vs = new ValueSpecifier();
            if (valueSpec == null)
            {
                return vs;
            }
            vs.referenceName = valueSpec.Name;
            ReadBool(valueSpec, "AddConstant", out vs.addConstant, false);
            ReadBool(valueSpec, "AddComboLength", out vs.addComboLength, false);
            ReadBool(valueSpec, "AddCharStatRelated", out vs.addCharStatRelated, false);
            ReadBool(valueSpec, "AddTargetCountRelated", out vs.addTargetCountRelated, false);
            ReadBool(valueSpec, "DivideValue", out vs.divideValue, false);
            ReadBool(valueSpec, "IgnoreFameModifier", out vs.ignoreFameModifier, false);
            ReadFloat(valueSpec, "AbsoluteMinimum", out vs.absoluteMinimum, false);
            ReadFloat(valueSpec, "AbsoluteMaximum", out vs.absoluteMaximum, false);
            ReadFloat(valueSpec, "LinkedAttributeModifier", out vs.linkedAttributeModifier, false);
            ReadFloat(valueSpec, "ConstantMinimum", out vs.constantMinimum, false);
            ReadFloat(valueSpec, "ConstantMaximum", out vs.constantMaximum, false);
            ReadFloat(valueSpec, "ComboLengthMinimum", out vs.comboLengthMinimum, false);
            ReadFloat(valueSpec, "ComboLengthMaximum", out vs.comboLengthMaximum, false);
            byte charStatistic;
            ReadByte(valueSpec, "CharacterStatistic", out charStatistic, false);
            vs.characterStatistic = (EVSCharacterStatistic) charStatistic;
            byte source;
            ReadByte(valueSpec, "Source", out source, false);
            vs.source = (EVSSource) source;
            ReadFloat(valueSpec, "CharStatMinimumMultiplier", out vs.charStatMinimumMultiplier, false);
            ReadFloat(valueSpec, "CharStatMaximumMultiplier", out vs.charStatMaximumMultiplier, false);
            ReadFloat(valueSpec, "TargetCountMinimumMultiplier", out vs.targetCountMinimumMultiplier, false);
            ReadFloat(valueSpec, "TargetCountMaximumMultiplier", out vs.targetCountMaximumMultiplier, false);
            ReadBool(valueSpec, "ApplyIncrease", out vs.applyIncrease, false);
            ReadFloat(valueSpec, "SpiritIncrease", out vs.spiritIncrease, false);
            ReadFloat(valueSpec, "PlayerIncrease", out vs.playerIncrease, false);
            ReadFloat(valueSpec, "NPCIncrease", out vs.NPCIncrease, false);
            SBProperty taxArrayProp;
            if (valueSpec.sbObject.Properties.TryGetValue("TaxonomyIncreases", out taxArrayProp))
            {
                foreach (var taxIncreaseProp in taxArrayProp.Array.Values)
                {
                    var tx = new ValueSpecifier.TaxonomyIncrease();
                    var taxName = taxIncreaseProp.Array["Node"].Value;
                    var increase = float.Parse(taxIncreaseProp.Array["Increase"].Value);
                    tx.temporaryTaxonomyName = taxName;
                    tx.increase = increase;
                    vs.taxonomyIncreases.Add(tx);
                }
            }
            return vs;
        }

        AudioVisualSkillEffect FindEffectReference(string FXName)
        {
            for (var i = 0; i < avcollections.Count; i++)
            {
                var avs = avcollections[i].GetEffect(FXName) as AudioVisualSkillEffect;
                if (avs != null)
                {
                    return avs;
                }
            }
            return null;
        }
    }
}