using System.Collections.Generic;
using System.IO;
using Database.Static;
using Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    internal class SkillDeckExtractor : ExtractorAdapter
    {
        string folder;

        List<SkillCollection> skillCollections = new List<SkillCollection>();

        public override string Name
        {
            get { return "SkillDeck Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract the SkillDecks with their skills into the chosen folder"; }
        }

        public override void DrawGUI(Rect r)
        {
            var sc = EditorGUILayout.ObjectField(null, typeof (SkillCollection), false) as SkillCollection;
            if (sc != null && !skillCollections.Contains(sc))
            {
                skillCollections.Add(sc);
                var sPath = AssetDatabase.GetAssetPath(sc);
                sPath = sPath.Replace(Path.GetFileName(sPath), string.Empty);
                sPath = sPath.Replace("Assets/Resources/", string.Empty);
                var sec = Resources.LoadAll<SkillCollection>(sPath);
                for (var i = 0; i < sec.Length; i++)
                {
                    if (!skillCollections.Contains(sec[i]))
                    {
                        skillCollections.Add(sec[i]);
                    }
                }
            }
            for (var i = 0; i < skillCollections.Count; i++)
            {
                if (GUILayout.Button(skillCollections[i].name))
                {
                    skillCollections.RemoveAt(i);
                    break;
                }
            }
        }

        SkillCollection GetCollection(SkillCollection.SkillCollectionType type)
        {
            for (var i = 0; i < skillCollections.Count; i++)
            {
                if (skillCollections[i].type == type)
                {
                    return skillCollections[i];
                }
            }
            return null;
        }

        FSkill GetSkillFromPackage(string fullRefName)
        {
            var parts = fullRefName.Split('.');
            SkillCollection selected = null;
            if (parts[0].Contains("NPC"))
            {
                selected = GetCollection(SkillCollection.SkillCollectionType.NPC);
            }
            else if (parts[0].Contains("Player"))
            {
                selected = GetCollection(SkillCollection.SkillCollectionType.Player);
            }
            else if (parts[0].Contains("Combo"))
            {
                selected = GetCollection(SkillCollection.SkillCollectionType.Combo);
            }
            else if (parts[0].Contains("Test"))
            {
                selected = GetCollection(SkillCollection.SkillCollectionType.Test);
            }
            else if (parts[0].Contains("Event"))
            {
                selected = GetCollection(SkillCollection.SkillCollectionType.Event);
            }
            else if (parts[0].Contains("Item"))
            {
                selected = GetCollection(SkillCollection.SkillCollectionType.Item);
            }
            if (selected != null)
            {
                return selected.FindSkill(parts[parts.Length - 1]);
            }
            return null;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (skillCollections.Count == 0)
            {
                Log("No skillCollections assigned", Color.yellow);
                return;
            }
            var path = EditorUtility.OpenFolderPanel("Select Folder to save into", "Assets/Resources/Skills/", "Decks");
            if (string.IsNullOrEmpty(path))
            {
                Log("Invalid Path", Color.yellow);
                return;
            }
            folder = "Assets" + path.Substring(Application.dataPath.Length);
            var p = extractorWindowRef.ActiveWrapper;
            foreach (var wpo in p.IterateObjects())
            {
                if (wpo.sbObject.ClassName.Replace("\0", string.Empty) == "SBGame.NPC_SkillDeck")
                {
                    var newDeck = ScriptableObject.CreateInstance<SkillDeck>();
                    var tierArrayProp = wpo.FindProperty("Tiers");
                    if (tierArrayProp == null)
                    {
                        Log("No DeckTiers available for: " + wpo.Name, Color.yellow);
                        continue;
                    }
                    foreach (var tierProp in tierArrayProp.IterateInnerProperties())
                    {
                        var tierObject = FindReferencedObject(tierProp);
                        if (tierObject == null)
                        {
                            Log("Referenced Tier not found", Color.yellow);
                            continue;
                        }
                        var newTier = new SkillDeck.SkillDeckTier();
                        var skillsArrayProp = tierObject.FindProperty("skills");
                        if (skillsArrayProp != null)
                        {
                            var currentSkillIndex = 0;
                            foreach (var skillRefProp in skillsArrayProp.IterateInnerProperties())
                            {
                                var searchName = skillRefProp.Value.Replace("\0", string.Empty);
                                if (searchName != "null")
                                {
                                    var skill = GetSkillFromPackage(searchName);
                                    if (skill != null)
                                    {
                                        newTier[currentSkillIndex] = skill;
                                    }
                                }
                                currentSkillIndex++;
                            }
                            newDeck.Tiers.Add(newTier);
                        }
                    }
                    SaveDeck(newDeck, wpo.Name);
                }
            }
        }

        void SaveDeck(SkillDeck deck, string deckName)
        {
            AssetDatabase.CreateAsset(deck, folder + "/" + deckName + ".asset");
            EditorUtility.SetDirty(deck);
        }
    }
}