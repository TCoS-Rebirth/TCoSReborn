using System;
using System.Collections.Generic;
using System.IO;
using Database.Static;
using Gameplay;
using Gameplay.Entities.NPCs;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class TaxonomyExtractor : ExtractorAdapter
    {
        List<NPCCollection> npcCols = new List<NPCCollection>();

        List<Taxonomy> previousTaxes = new List<Taxonomy>();

        public override string Name
        {
            get { return "Taxonomy Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract the Taxonomies and assign the references between them"; }
        }

        public override void DrawGUI(Rect r)
        {
            if (GUILayout.Button("Load References"))
            {
                Log("Disabled, unused", Color.red);
                //previousTaxes.Clear();


                //foreach (Taxonomy tsearch in previousTaxes)
                //{
                //    foreach (string like in tsearch.temporaryLikes)
                //    {
                //        Taxonomy t = FindTaxonomy(like, previousTaxes);
                //        if (t != null)
                //        {
                //            tsearch.likes.Add(t);
                //        }
                //        else
                //        {
                //            Log(like + " - like not found", Color.red);
                //        }
                //    }
                //    foreach (string dislike in tsearch.temporaryDislikes)
                //    {
                //        Taxonomy t = FindTaxonomy(dislike, previousTaxes); 
                //        if (t != null)
                //        {
                //            tsearch.dislikes.Add(t);
                //        }
                //        else
                //        {
                //            Log(dislike + " - dislike not found", Color.red);
                //        }
                //    }
                //    Taxonomy p = FindTaxonomy(tsearch.temporaryParentName, previousTaxes);
                //    if (p != null)
                //    {
                //        tsearch.parent = p;
                //    }
                //}
                //previousTaxes.Clear();
            }
        }

        Taxonomy FindTaxonomy(string tname, List<Taxonomy> taxes)
        {
            foreach (var t in taxes)
            {
                if (t.name.Equals(tname, StringComparison.OrdinalIgnoreCase))
                {
                    return t;
                }
            }
            return null;
        }

        public override bool IsCompatible(PackageWrapper p)
        {
            return base.IsCompatible(p) && p.Name.Contains("Taxonomy");
        }

        Taxonomy FindTaxonomy(string name)
        {
            foreach (var t in previousTaxes)
            {
                if (t.name.Equals(name.Replace("\0", string.Empty)))
                {
                    return t;
                }
            }
            return null;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            //Log("Disabled, unused", Color.red);
            //return;

            //Load Taxonomies
            var files = Directory.GetFiles(Application.dataPath + "/GameData/Taxonomies/");
            foreach (var f in files)
            {
                var t = AssetDatabase.LoadAssetAtPath<Taxonomy>("Assets" + f.Replace(Application.dataPath, string.Empty));
                if (t != null)
                {
                    previousTaxes.Add(t);
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

            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("NPC_Taxonomy"))
                {
                    var t = FindTaxonomy(wpo.sbObject.Name);

                    if (t == null)
                    {
                        #region newTaxAsset

                        //Generate the taxonomy if not found
                        var p = extractorWindowRef.ActiveWrapper;
                        var newT = ScriptableObject.CreateInstance<Taxonomy>();

                        string description, note;
                        ReadLocalizedString(wpo, "Description", localizedStrings, out description);
                        newT.Description = description;

                        ReadString(wpo, "Note", out note);
                        newT.Note = note;


                        newT.ID = resources.GetResource(string.Format("{0}.{1}", p.Name, wpo.Name)).ID;

                        if (!ReadInt(wpo, "CachedColorCloth1", out newT.cachedColorCloth1))
                        {
                            newT.cachedColorCloth1 = 0;
                        }
                        if (!ReadInt(wpo, "CachedColorCloth2", out newT.cachedColorCloth2))
                        {
                            newT.cachedColorCloth2 = 0;
                        }
                        if (!ReadInt(wpo, "CachedColorArmor1", out newT.cachedColorArmor1))
                        {
                            newT.cachedColorArmor1 = 0;
                        }
                        if (!ReadInt(wpo, "CachedColorArmor2", out newT.cachedColorArmor2))
                        {
                            newT.cachedColorArmor2 = 0;
                        }
                        SBProperty lootProp;
                        if (wpo.sbObject.Properties.TryGetValue("Loot", out lootProp))
                        {
                            foreach (var lp in lootProp.Array.Values)
                            {
                                newT.temporaryLootTableNames.Add(lp.Value.Replace("\0", string.Empty));
                            }
                        }
                        AssetDatabase.CreateAsset(newT, @"Assets/GameData/Taxonomies/" + wpo.Name + ".asset");
                        previousTaxes.Add(newT);
                        t = newT;

                        #endregion
                    }

                    #region existingTaxAsset

                    string parent;
                    string classesPackage;

                    //Find and assign classesPackage
                    if (ReadString(wpo, "ClassesPackage", out classesPackage))
                    {
                        foreach (var nc in npcCols)
                        {
                            if (nc.name == classesPackage)
                            {
                                t.classPackage = nc;
                            }
                        }
                    }

                    if (ReadString(wpo, "Parent", out parent))
                    {
                        var parentTax = FindTaxonomy(parent);
                        if (parentTax != null)
                        {
                            t.parent = parentTax;
                        }
                    }
                    var likeArray = wpo.FindProperty("Likes");
                    if (likeArray != null)
                    {
                        foreach (var like in likeArray.IterateInnerProperties())
                        {
                            var likeName = like.GetValue<string>();
                            var lt = FindTaxonomy(likeName);
                            if (lt != null)
                            {
                                t.likes.Add(lt);
                            }
                        }
                    }
                    var dislikeArray = wpo.FindProperty("Dislikes");
                    if (dislikeArray != null)
                    {
                        foreach (var dlike in dislikeArray.IterateInnerProperties())
                        {
                            var dlikeName = dlike.GetValue<string>();
                            var dlt = FindTaxonomy(dlikeName);
                            if (dlt != null)
                            {
                                t.likes.Add(dlt);
                            }
                        }
                    }
                    EditorUtility.SetDirty(t);

                    #endregion
                }
            }

            //previousTaxes.Clear();
            //    PackageWrapper p = extractorWindowRef.ActiveWrapper;
            //    foreach (WrappedPackageObject wo in p.wrappedObjects)
            //    {
            //        if (!wo.sbObject.ClassName.Contains("Taxonomy")) { continue; }
            //        Taxonomy t = ScriptableObject.CreateInstance<Taxonomy>();
            //        ReadString(wo, "ClassesPackage", out t.classPackage);
            //        ReadLocalizedString(wo, "Description", localizedStrings, out t.description);
            //        ReadString(wo, "Note", out t.note);
            //        SBProperty likeProp;
            //        if (wo.sbObject.Properties.TryGetValue("Likes", out likeProp))
            //        {
            //            foreach (SBProperty likesProp in likeProp.Array.Values)
            //            {
            //                t.temporaryLikes.Add(likesProp.Value.Replace("\0", string.Empty));
            //            }
            //        }
            //        SBProperty dislikeProp;
            //        if (wo.sbObject.Properties.TryGetValue("Dislikes", out dislikeProp))
            //        {
            //            foreach (SBProperty dislikesProp in dislikeProp.Array.Values)
            //            {
            //                t.temporaryDislikes.Add(dislikesProp.Value.Replace("\0", string.Empty));
            //            }
            //        }
            //        ReadString(wo, "Parent", out t.temporaryParentName);

            //        t.ID = resources.GetResource(string.Format("{0}.{1}", p.Name, wo.Name)).ID;

            //        if (!ReadInt(wo, "CachedColorCloth1", out t.cachedColorCloth1))
            //        {
            //            t.cachedColorCloth1 = 0;
            //        }
            //        if (!ReadInt(wo, "CachedColorCloth2", out t.cachedColorCloth2))
            //        {
            //            t.cachedColorCloth2 = 0;
            //        }
            //        if (!ReadInt(wo, "CachedColorArmor1", out t.cachedColorArmor1))
            //        {
            //            t.cachedColorArmor1 = 0;
            //        }
            //        if (!ReadInt(wo, "CachedColorArmor2", out t.cachedColorArmor2))
            //        {
            //            t.cachedColorArmor2 = 0;
            //        }
            //        SBProperty lootProp;
            //        if (wo.sbObject.Properties.TryGetValue("Loot", out lootProp))
            //        {
            //            foreach (SBProperty lp in lootProp.Array.Values)
            //            {
            //                t.temporaryLootTableNames.Add(lp.Value.Replace("\0", string.Empty));
            //            }
            //        }
            //        AssetDatabase.CreateAsset(t, @"Assets/Resources/Taxonomies/" + wo.Name + ".asset");
            //        previousTaxes.Add(t);
            //    }
        }
    }
}