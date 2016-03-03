using System;
using System.Collections.Generic;
using Database.Static;
using Gameplay.Items;
using Gameplay.Items.ItemComponents;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PackageExtractor.Adapter
{
    public class ItemExtractor : ExtractorAdapter
    {
        const string sbg = "SBGame.";

        ItemCollection itc;

        public override string Name
        {
            get { return "Item Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract items into their respective collections"; }
        }

        public override bool IsCompatible(PackageWrapper p)
        {
            return base.IsCompatible(p) && p.Name.Contains("Item");
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            itc = ScriptableObject.CreateInstance<ItemCollection>();
            AssetDatabase.CreateAsset(itc, "Assets/GameData/Items/" + extractorWindowRef.ActiveWrapper.Name + ".asset");
            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                switch (wpo.sbObject.ClassName.Replace("\0", string.Empty))
                {
                    case "SBGamePlay.Item_ArmorBelt":
                        ExtractItem<Item_ArmorBelt>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorChest":
                        ExtractItem<Item_ArmorChest>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorHeadGear":
                        ExtractItem<Item_ArmorHeadGear>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorLeftGauntlet":
                        ExtractItem<Item_ArmorLeftGauntlet>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorLeftShin":
                        ExtractItem<Item_ArmorLeftShin>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorLeftShoulder":
                        ExtractItem<Item_ArmorLeftShoulder>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorLeftThigh":
                        ExtractItem<Item_ArmorLeftThigh>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorRightGauntlet":
                        ExtractItem<Item_ArmorRightGauntlet>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorRightShin":
                        ExtractItem<Item_ArmorRightShin>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorRightShoulder":
                        ExtractItem<Item_ArmorRightShoulder>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ArmorRightThigh":
                        ExtractItem<Item_ArmorRightThigh>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_BodySlot":
                        ExtractItem<Item_BodySlot>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_Broken":
                        ExtractItem<Item_Broken>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ClothChest":
                        ExtractItem<Item_ClothChest>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ClothLeftGlove":
                        ExtractItem<Item_ClothLeftGlove>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ClothPants":
                        ExtractItem<Item_ClothPants>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ClothRightGlove":
                        ExtractItem<Item_ClothRightGlove>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ClothShoes":
                        ExtractItem<Item_ClothShoes>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_Consumable":
                        ExtractItem<Item_Consumable>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ContainerExtraInventory":
                        ExtractItem<Item_ContainerExtraInventory>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ContainerSuitBag":
                        ExtractItem<Item_ContainerSuitBag>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_ItemToken":
                        ExtractItem<Item_ItemToken>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_JewelryNecklace":
                        ExtractItem<Item_JewelryNecklace>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_JewelryQualityToken":
                        ExtractItem<Item_JewelryQualityToken>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_JewelryRing":
                        ExtractItem<Item_JewelryRing>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_MiscellaneousKey":
                        ExtractItem<Item_MiscellaneousKey>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_MiscellaneousLabyrinthKey":
                        ExtractItem<Item_MiscellaneousLabyrinthKey>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_MiscellaneousTickets":
                        ExtractItem<Item_MiscellaneousTickets>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_QuestItem":
                        ExtractItem<Item_QuestItem>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_Recipe":
                        ExtractItem<Item_Recipe>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_Resource":
                        ExtractItem<Item_Resource>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_SkillToken":
                        ExtractItem<Item_SkillToken>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_Trophy":
                        ExtractItem<Item_Trophy>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_WeaponDoubleHanded":
                        ExtractItem<Item_WeaponDoubleHanded>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_WeaponDualWielding":
                        ExtractItem<Item_WeaponDualWielding>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_WeaponOneHanded":
                        ExtractItem<Item_WeaponOneHanded>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_WeaponQualityToken":
                        ExtractItem<Item_WeaponQualityToken>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_WeaponRanged":
                        ExtractItem<Item_WeaponRanged>(wpo, resources, localizedStrings);
                        break;
                    case "SBGamePlay.Item_WeaponShield":
                        ExtractItem<Item_WeaponShield>(wpo, resources, localizedStrings);
                        break;
                }
            }
            EditorUtility.SetDirty(itc);
        }

        T ExtractItem<T>(WrappedPackageObject wpo, SBResources res, SBLocalizedStrings strings) where T : Item_Type
        {
            var obj = ScriptableObject.CreateInstance<T>();
            obj.name = wpo.Name;
            itc.items.Add(obj);
            AssetDatabase.AddObjectToAsset(obj, itc);
            obj.internalName = FormatInternalName(wpo);
            obj.resourceID = res.GetResourceID(FormatSearchableName(wpo));
            if (obj.resourceID == -1)
            {
                Object.DestroyImmediate(obj);
                throw new Exception("Resource ID not found for: " + obj.internalName);
            }
            ReadEnum(wpo, "ItemType", out obj.ItemType);
            ReadInt(wpo, "StackableAmount", out obj.StackableAmount);
            ReadBool(wpo, "Tradable", out obj.Tradable);
            ReadBool(wpo, "RecyclableIntoMoney", out obj.RecyclableIntoMoney);
            ReadBool(wpo, "Sellable", out obj.Sellable);
            ReadEnum(wpo, "EquipmentSlot", out obj.equipmentSlot);
            ReadBool(wpo, "Equipable", out obj.Equipable);
            ReadBool(wpo, "BindOnPickup", out obj.BindOnPickup);
            ReadBool(wpo, "BindOnEquip", out obj.BindOnEquip);
            ReadEnum(wpo, "ItemRarity", out obj.ItemRarity);
            ReadInt(wpo, "BuyPriceValue", out obj.BuyPriceValue);
            ReadInt(wpo, "SellPriceValue", out obj.SellPriceValue);
            ReadInt(wpo, "RecyclePriceValue", out obj.RecyclePriceValue);
            ReadByte(wpo, "MinLevel", out obj.MinLevel);
            ReadLocalizedString(wpo, "Name", strings, out obj.Name);
            ReadLocalizedString(wpo, "Description", strings, out obj.Description);
            ReadFloat(wpo, "UseCooldown", out obj.UseCooldown);
            var componentProp = wpo.FindProperty("Components");
            if (componentProp != null)
            {
                foreach (var compRefProp in componentProp.IterateInnerProperties())
                {
                    var component = extractorWindowRef.ActiveWrapper.FindObjectWrapper(compRefProp.GetValue<string>());
                    if (component != null)
                    {
                        var floatingAssets = new List<ScriptableObject>();
                        var ic = ExtractComponent(component, res, floatingAssets);
                        if (ic != null)
                        {
                            ic.name = component.Name;
                            obj.Components.Add(ic);
                            ic.hideFlags = HideFlags.HideInHierarchy;
                            AssetDatabase.AddObjectToAsset(ic, obj);
                            foreach (var so in floatingAssets)
                            {
                                so.hideFlags = HideFlags.HideInHierarchy;
                                AssetDatabase.AddObjectToAsset(so, ic);
                                EditorUtility.SetDirty(so);
                            }
                            EditorUtility.SetDirty(ic);
                        }
                        else
                        {
                            throw new Exception("Component could not be extracted" + component.Name);
                        }
                    }
                    else
                    {
                        throw new Exception("Component referenced object could not be found: " + compRefProp.GetValue<string>());
                    }
                }
            }
            var requirementProp = wpo.FindProperty("Requirements");
            if (requirementProp != null)
            {
                foreach (var reqRefProp in requirementProp.IterateInnerProperties())
                {
                    var requirement = extractorWindowRef.ActiveWrapper.FindObjectWrapper(reqRefProp.GetValue<string>());
                    if (requirement != null)
                    {
                        var req = ExtractRequirement(requirement, res);
                        if (req != null)
                        {
                            req.name = requirement.Name;
                            obj.Requirements.Add(req);
                            req.hideFlags = HideFlags.HideInHierarchy;
                            AssetDatabase.AddObjectToAsset(req, obj);
                            EditorUtility.SetDirty(req);
                        }
                        else
                        {
                            throw new Exception("Requirement could not be extracted: " + requirement.Name);
                        }
                    }
                    else
                    {
                        throw new Exception("Requirement referenced object could not be found: " + reqRefProp.GetValue<string>());
                    }
                }
            }
            return obj;
        }

        Item_Component ExtractComponent(WrappedPackageObject componentObject, SBResources res, List<ScriptableObject> refFloatingAssets)
        {
            switch (componentObject.sbObject.ClassName.Replace("\0", string.Empty))
            {
                case sbg + "IC_Appearance":
                    var icap = ScriptableObject.CreateInstance<IC_Appearance>();
                    ReadString(componentObject, "CharacterAppearance", out icap.Appearance);
                    ReadInt(componentObject, "DyePrice", out icap.DyePrice);
                    return icap;
                case sbg + "IC_BodySlot":
                    var icbo = ScriptableObject.CreateInstance<IC_BodySlot>();
                    ReadString(componentObject, "FakeSkill", out icbo.temporaryFakeSkillName);
                    icbo.fakeSkillID = res.GetResourceID(icbo.temporaryFakeSkillName);
                    ReadBool(componentObject, "UserStartable", out icbo.UserStartable);
                    ReadEnum(componentObject, "Type", out icbo.Type);
                    ReadEnum(componentObject, "ForClass", out icbo.ForClass);
                    return icbo;
                case sbg + "IC_Broken":
                    var icbr = ScriptableObject.CreateInstance<IC_Broken>();
                    ReadString(componentObject, "Recipe", out icbr.temporaryRecipeName);
                    icbr.recipeID = res.GetResourceID(icbr.temporaryRecipeName);
                    return icbr;
                case sbg + "IC_Consumable":
                    var icco = ScriptableObject.CreateInstance<IC_Consumable>();
                    ReadEnum(componentObject, "Type", out icco.Type);
                    return icco;
                case sbg + "IC_Container":
                    var iccon = ScriptableObject.CreateInstance<IC_Container>();
                    ReadInt(componentObject, "ContainerSlots", out iccon.ContainerSlots);
                    return iccon;
                case sbg + "IC_EquipEffects":
                    var icee = ScriptableObject.CreateInstance<IC_EquipEffects>();
                    ReadString(componentObject, "EquipDuffEvent", out icee.temporaryEquipDuffEvent);
                    icee.equipDuffEventID = res.GetResourceID(icee.temporaryEquipDuffEvent);
                    ReadEnum(componentObject, "EquipTattooSet", out icee.EquipTattooSet);
                    ReadEnum(componentObject, "EquipTattooBodyPart", out icee.EquipTattooBodyPart);
                    return icee;
                case sbg + "IC_Equipment":
                    var iceeq = ScriptableObject.CreateInstance<IC_Equipment>();
                    return iceeq;
                case sbg + "IC_Key":
                    var ick = ScriptableObject.CreateInstance<IC_Key>();
                    ReadString(componentObject, "UnlockTag", out ick.UnlockTag);
                    return ick;
                case sbg + "IC_LabyrinthKey":
                    var iclk = ScriptableObject.CreateInstance<IC_LabyrinthKey>();
                    ReadInt(componentObject, "MinSpawnLevel", out iclk.MinSpawnLevel);
                    ReadInt(componentObject, "MaxSpawnLevel", out iclk.MaxSpawnLevel);
                    return iclk;
                case sbg + "IC_RangedWeapon":
                    var icr = ScriptableObject.CreateInstance<IC_RangedWeapon>();
                    return icr;
                case sbg + "IC_Recipe":
                    var icre = ScriptableObject.CreateInstance<IC_Recipe>();
                    var recipeCompProp = componentObject.FindProperty("Components");
                    if (recipeCompProp != null)
                    {
                        foreach (var recipeComponent in recipeCompProp.IterateInnerProperties())
                        {
                            var rcomp = new IC_Recipe.RecipeComponent();
                            var rcItemName = recipeComponent.GetInnerProperty("Item");
                            if (rcItemName != null)
                            {
                                rcomp.temporaryItemName = rcItemName.GetValue<string>();
                                rcomp.itemID = res.GetResourceID(rcomp.temporaryItemName);
                            }
                            var rcQuantity = recipeComponent.GetInnerProperty("Quantity");
                            if (rcQuantity != null)
                            {
                                rcomp.Quantity = rcQuantity.GetValue<int>();
                            }
                            icre.Components.Add(rcomp);
                        }
                    }
                    var recycleCompProp = componentObject.FindProperty("RecycleComponents");
                    if (recycleCompProp != null)
                    {
                        foreach (var recycleComponent in recycleCompProp.IterateInnerProperties())
                        {
                            var reccomp = new IC_Recipe.RecipeComponent();
                            var rcItemName = recycleComponent.GetInnerProperty("Item");
                            if (rcItemName != null)
                            {
                                reccomp.temporaryItemName = rcItemName.GetValue<string>();
                                reccomp.itemID = res.GetResourceID(reccomp.temporaryItemName);
                            }
                            var rcQuantity = recycleComponent.GetInnerProperty("Quantity");
                            if (rcQuantity != null)
                            {
                                reccomp.Quantity = rcQuantity.GetValue<int>();
                            }
                            icre.RecycleComponents.Add(reccomp);
                        }
                    }
                    ReadString(componentObject, "ProducedItem", out icre.temporaryProducedItemName);
                    icre.producedItemID = res.GetResourceID(icre.temporaryProducedItemName);
                    return icre;
                case sbg + "IC_Resource":
                    var icres = ScriptableObject.CreateInstance<IC_Resource>();
                    ReadEnum(componentObject, "ResourceType", out icres.ResourceType);
                    return icres;
                case sbg + "IC_Shield":
                    var icsh = ScriptableObject.CreateInstance<IC_Shield>();
                    return icsh;
                case sbg + "IC_Ticket":
                    var icti = ScriptableObject.CreateInstance<IC_Ticket>();
                    ReadEnum(componentObject, "AccessLevel", out icti.AccessLevel);
                    ReadBool(componentObject, "TeleportOnUse", out icti.TeleportOnUse);
                    return icti;
                case sbg + "IC_TokenItem":
                    var ictit = ScriptableObject.CreateInstance<IC_TokenItem>();
                    ReadInt(componentObject, "TokenRank", out ictit.TokenRank);
                    ReadByte(componentObject, "SlotType", out ictit.SlotType);
                    ReadInt(componentObject, "ForgePrice", out ictit.ForgePrice);
                    ReadInt(componentObject, "ForgeReplacePrice", out ictit.ForgeReplacePrice);
                    ReadInt(componentObject, "ForgeRemovePrice", out ictit.ForgeRemovePrice);
                    var effectProp = componentObject.FindProperty("EquipEffects");
                    if (effectProp != null)
                    {
                        foreach (var effect in effectProp.IterateInnerProperties())
                        {
                            var fxName = effect.GetValue<string>();
                            ictit.temporarySkillEffectDuffNames.Add(fxName);
                            ictit.equipEffectIDs.Add(res.GetResourceID(fxName));
                        }
                    }
                    return ictit;
                case sbg + "IC_TokenSkill":
                    var icts = ScriptableObject.CreateInstance<IC_TokenSkill>();
                    ReadInt(componentObject, "TokenRank", out icts.TokenRank);
                    ReadEnum(componentObject, "Effect1", out icts.Effect1);
                    ReadEnum(componentObject, "Effect2", out icts.Effect2);
                    ReadEnum(componentObject, "Effect3", out icts.Effect3);
                    ReadEnum(componentObject, "Effect4", out icts.Effect4);
                    ReadEnum(componentObject, "Effect5", out icts.Effect5);
                    ReadEnum(componentObject, "Effect6", out icts.Effect6);
                    ReadEnum(componentObject, "Effect7", out icts.Effect7);
                    ReadEnum(componentObject, "Effect8", out icts.Effect8);
                    ReadFloat(componentObject, "Value", out icts.Value);
                    ReadEnum(componentObject, "ValueMode", out icts.ValueMode);
                    return icts;
                case sbg + "IC_TokenSlot":
                    var ictsl = ScriptableObject.CreateInstance<IC_TokenSlot>();
                    var slotsProp = componentObject.FindProperty("slots");
                    if (slotsProp != null)
                    {
                        foreach (var slotInfo in slotsProp.IterateInnerProperties())
                        {
                            var ts = new IC_TokenSlot.TokenSlot();
                            var rankInfo = slotInfo.GetInnerProperty("rank");
                            if (rankInfo != null)
                            {
                                ts.rank = rankInfo.GetValue<int>();
                            }
                            var typeinfo = slotInfo.GetInnerProperty("SlotType");
                            if (typeinfo != null)
                            {
                                ts.SlotType = (IC_TokenSlot.ESigilSlotType) typeinfo.GetValue<byte>();
                            }
                            ictsl.slots.Add(ts);
                        }
                    }
                    return ictsl;
                case sbg + "IC_Use":
                    var icu = ScriptableObject.CreateInstance<IC_Use>();
                    var eventInfoProp = componentObject.FindProperty("Events");
                    if (eventInfoProp != null)
                    {
                        foreach (var eventProp in eventInfoProp.IterateInnerProperties())
                        {
                            var eventObject = extractorWindowRef.ActiveWrapper.FindObjectWrapper(eventProp.GetValue<string>());
                            if (eventObject != null)
                            {
                                var ce = ExtractEvent(eventObject, res);
                                if (ce != null)
                                {
                                    ce.name = eventObject.Name;
                                    icu.Events.Add(ce);
                                    refFloatingAssets.Add(ce);
                                }
                                else
                                {
                                    throw new Exception("Couldn't extract event: " + eventObject.Name);
                                }
                            }
                            else
                            {
                                throw new Exception("Couldn't find event: " + eventProp.GetValue<string>());
                            }
                        }
                    }
                    return icu;
                case sbg + "IC_Weapon":
                    var icw = ScriptableObject.CreateInstance<IC_Weapon>();
                    return icw;
            }
            return null;
        }
    }
}