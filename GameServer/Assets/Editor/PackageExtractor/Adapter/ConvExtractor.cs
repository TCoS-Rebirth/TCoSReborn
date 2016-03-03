using Database.Static;
using Gameplay.Conversations;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class ConvExtractor : ExtractorAdapter
    {
        ConvCollection convCol;
        SBLocalizedStrings locStrings = ScriptableObject.CreateInstance<SBLocalizedStrings>();
        SBResources resourcesProp = ScriptableObject.CreateInstance<SBResources>();

        //string gameDataPath = "Assets/GameData/";

        public override string Name
        {
            get { return "Conversation Extractor"; }
        }

        public override string Description
        {
            get { return "Extracts FULL conversation topics into corresponding conversation collection asset, but doesn't add references to NPCs. Run QuestExtractor FIRST!"; }
        }


        public override void DrawGUI(Rect r)
        {
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            //Log("Disabled, unused", Color.red);
            //return;

            //Package name
            var pW = extractorWindowRef.ActiveWrapper;
            //string saveName = pW.Name;           

            //Load localised strings
            locStrings = AssetDatabase.LoadAssetAtPath<SBLocalizedStrings>("Assets/GameData/SBResources/SBLocalizedStrings.asset");

            //Load resources
            resourcesProp = resources;

            var convColPath = "Assets/GameData/Conversations/" + pW.Name + ".asset";

            //Try to load asset from Conversations folder
            convCol = AssetDatabase.LoadAssetAtPath<ConvCollection>(convColPath);

            //If no asset exists
            if (convCol == null)
            {
                //Create asset
                convCol = ScriptableObject.CreateInstance<ConvCollection>();
                AssetDatabase.CreateAsset(convCol, convColPath);
            }


            //Cycle WPOs
            //If WPO is of a conversation topic class
            foreach (var wpo in pW.IterateObjects())
            {
                if (wpo.sbObject.ClassName.Contains("CT_"))
                {
                    //Extract the full topic
                    var newTopic = getConvTopicFull(wpo, resourcesProp, locStrings, pW);
                    convCol.topics.Add(newTopic);
                    AssetDatabase.AddObjectToAsset(newTopic, convCol);
                }
            }
            EditorUtility.SetDirty(convCol);
            AssetDatabase.SaveAssets();
        }
    }
}