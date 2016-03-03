using Database.Static;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor.Adapter
{
    public class ObjectFinder : ExtractorAdapter
    {
        string searchFieldValue = "";
        bool useFullName;

        public override string Name
        {
            get { return "Object Finder"; }
        }

        public override string Description
        {
            get { return "Check if an object exists in the package"; }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(40));
            searchFieldValue = EditorGUILayout.TextField(searchFieldValue);
            GUILayout.EndHorizontal();
            useFullName = GUILayout.Toggle(useFullName, "Name includes package");
            if (GUILayout.Button("Search"))
            {
                var objWrapper = extractorWindowRef.FindObjectWrapper(searchFieldValue, useFullName);
                if (objWrapper != null)
                {
                    Log(string.Format("Object found! {0}", objWrapper.sbObject.Name), Color.green);
                }
                else
                {
                    Log("Object not found", Color.yellow);
                }
            }
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
        }
    }
}