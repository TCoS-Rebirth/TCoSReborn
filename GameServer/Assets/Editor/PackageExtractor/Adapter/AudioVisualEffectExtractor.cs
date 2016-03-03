using Database.Static;
using Gameplay.Skills;
using Gameplay.Skills.Effects;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class AudioVisualEffectExtractor : ExtractorAdapter
    {
        SkillEffectCollection fxCollection;

        public override string Name
        {
            get { return "AV Effect Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract the AudioVisual Effects as simple resources (ID) into the provided collection, the collection needs to be newly created!"; }
        }

        public override bool IsCompatible(PackageWrapper p)
        {
            return base.IsCompatible(p) && p.Name.Contains("Effects") && p.Name.Contains("AV");
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Effect collection asset:");
            fxCollection = EditorGUILayout.ObjectField(fxCollection, typeof (SkillEffectCollection), false) as SkillEffectCollection;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (fxCollection == null)
            {
                Log("No collection assigned!", Color.yellow);
                return;
            }
            var p = extractorWindowRef.ActiveWrapper;
            foreach (var wpo in p.IterateObjects())
            {
                var sbo = wpo.sbObject;
                if (!sbo.ClassName.Replace("\0", string.Empty).Contains("FSkill_EffectClass_AudioVisual"))
                {
                    continue;
                }
                var searchString = string.Format("{0}.{1}.{2}", p.Name, sbo.Package, sbo.Name);
                var res = resources.GetResource(searchString);
                if (res == null)
                {
                    Debug.Log("ID not found for: " + sbo.Name);
                }
                else
                {
                    var se = ScriptableObject.CreateInstance<AudioVisualSkillEffect>();
                    se.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    se.referenceName = searchString;
                    ReadRotator(wpo, "Rotation", out se.rotation);
                    ReadVector3(wpo, "Location", out se.location);
                    ReadFloat(wpo, "IntroDuration", out se.introDuration);
                    ReadFloat(wpo, "OutroDuration", out se.outroDuration);
                    ReadFloat(wpo, "PulseDuration", out se.pulseDuration);
                    ReadFloat(wpo, "Duration", out se.duration);
                    ReadFloat(wpo, "RunningDuration", out se.runningDuration);
                    ReadBool(wpo, "ScaleWithBase", out se.scaleWithBase);
                    ReadByte(wpo, "Category", out se.category);
                    se.name = se.referenceName;
                    se.resourceID = res.ID;
                    AssetDatabase.AddObjectToAsset(se, fxCollection);
                    fxCollection.effects.Add(se);
                }
            }
            EditorUtility.SetDirty(fxCollection);
        }
    }
}