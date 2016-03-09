using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Common.Internal;
using UnityEditor;
using UnityEngine;

namespace PackageExtractor
{
    public delegate WrappedPackageObject ObjectFinderDelegate(string objName, bool includesParentName);

    public class WrappedPackageObject
    {
        public List<WrappedPackageObject> ChildObjects = new List<WrappedPackageObject>();
        public bool ChildrenExpanded;
        public Dictionary<string, WrappedPackageObject> LinkedObjects = new Dictionary<string, WrappedPackageObject>(StringComparer.OrdinalIgnoreCase);

        public SBObject sbObject;

        public WrappedPackageObject(SBObject original)
        {
            sbObject = original;
        }

        public string Name
        {
            get { return sbObject.Name; }
        }

        public SBProperty FindProperty(string propName)
        {
            SBProperty prop;
            if (sbObject.Properties.TryGetValue(propName, out prop))
            {
                return prop;
            }
            return null;
        }

        public void CachePropertReferences(ObjectFinderDelegate findObject, List<PackageWrapper> importedPackages)
        {
            LinkedObjects.Clear();
            foreach (var p in sbObject.IterateProperties())
            {
                ResolvePropReferencesRecursive(p, findObject, importedPackages);
            }
        }

        void ResolvePropReferencesRecursive(SBProperty currentSearchProp, ObjectFinderDelegate findObject, List<PackageWrapper> importedPackages)
        {
            foreach (var objProp in currentSearchProp.IterateInnerProperties())
            {
                if (objProp.Type == PropertyType.ObjectProperty)
                {
                    var refWrapper = findObject(objProp.Value, true);
                    if (refWrapper == null && importedPackages != null)
                    {
                        for (var i = 0; i < importedPackages.Count; i++)
                        {
                            refWrapper = importedPackages[i].FindObjectWrapper(objProp.Value, true);
                            if (refWrapper != null)
                            {
                                Debug.Log("linked property");
                                break;
                            }
                        }
                    }
                    if (refWrapper != null)
                    {
                        LinkedObjects.Add(refWrapper.sbObject.Name.Replace("\0", string.Empty), refWrapper);
                    }
                }
                else if (objProp.Type == PropertyType.ArrayProperty || objProp.Type == PropertyType.FixedArrayProperty || objProp.Type == PropertyType.StructProperty)
                {
                    ResolvePropReferencesRecursive(objProp, findObject, importedPackages);
                }
            }
        }
    }

    public class PackageWrapper
    {
        List<SBObject> allSBObjects = new List<SBObject>();
        PackageReadConfig arrayDefinitions;
        public WrappedPackageObject inspectedObject;

        public Action<string, Color> LogString;
        float objectContentHeight;
        Rect objectScrollViewRect;
        float objectSubCategoryIndent = 30f;
        Vector2 objectViewScrollPos;

        public Action<string, Color> OnLogMessage;
        float prevWidthHeight;

        Vector2 propScrollPos;
        float visualOffset = 5f;

        public List<WrappedPackageObject> wrappedObjects = new List<WrappedPackageObject>();

        Package wrappedPackage;

        public PackageWrapper(string packageFileName, PackageReadConfig arrayDefinitions, Action<string, Color> logCallback)
        {
            wrappedPackage = new Package(packageFileName);
            this.arrayDefinitions = arrayDefinitions;
            wrappedPackage.OnLogMessage = logCallback;
            OnLogMessage = logCallback;
        }

        public string Name
        {
            get { return wrappedPackage.Name; }
        }

        public void Log(string msg, Color c)
        {
            if (OnLogMessage != null)
            {
                OnLogMessage(msg, c);
            }
        }

        public IEnumerable<WrappedPackageObject> IterateObjects()
        {
            foreach (var s in allSBObjects)
            {
                yield return new WrappedPackageObject(s);
            }
        }

        public SBObject FindReferencedObject(string objName, string referencingParent)
        {
            if (referencingParent == "")
            {
                referencingParent = "null";
            }
            foreach (var sbo in allSBObjects)
            {
                if (sbo.Name.Replace("\0", string.Empty).Equals(objName, StringComparison.OrdinalIgnoreCase)
                    && sbo.Package.Replace("\0", string.Empty).EndsWith(referencingParent, StringComparison.OrdinalIgnoreCase))
                {
                    return sbo;
                }
            }
            return null;
        }

        public SBObject FindObject(string objName, bool includesParentName)
        {
            var packageName = "";
            if (includesParentName)
            {
                var parts = objName.Split('.');
                if (parts.Length > 1)
                {
                    packageName = parts[0];
                    objName = parts[parts.Length - 1];
                }
                //find package parent
                foreach (var wrapper in wrappedObjects)
                {
                    var parent = FindRecursiveWrapper(wrapper, packageName);
                    if (parent != null)
                    {
                        var foundObject = FindRecursive(parent, objName);
                        if (foundObject != null)
                        {
                            return foundObject;
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < wrappedObjects.Count; i++)
                {
                    var foundObject = FindRecursive(wrappedObjects[i], objName);
                    if (foundObject != null)
                    {
                        return foundObject;
                    }
                }
            }
            return null;
        }

        //Valshaaran : implemented to filter out incorrect conversation topic matches in that extractor
        public SBObject FindObject(string objName, string fullPackageName)
        {  
            
            foreach (var obj in allSBObjects)
            {
                if ((obj.Name == objName) && (obj.Package == fullPackageName))
                        return obj;
            }

            return null;
        }

        public WrappedPackageObject FindObjectWrapper(string objName, bool includesParentName)
        {
            var packageName = "";
            if (includesParentName)
            {
                var parts = objName.Split('.');
                if (parts.Length > 1)
                {
                    packageName = parts[0];
                    objName = parts[parts.Length - 1];
                }
                //find package parent
                foreach (var wrapper in wrappedObjects)
                {
                    var parent = FindRecursiveWrapper(wrapper, packageName);
                    if (parent != null)
                    {
                        var foundObject = FindRecursiveWrapper(parent, objName);
                        if (foundObject != null)
                        {
                            return foundObject;
                        }
                    }
                    else
                    {
                        var foundObject = FindRecursiveWrapper(wrapper, objName);
                        if (foundObject != null)
                        {
                            return foundObject;
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < wrappedObjects.Count; i++)
                {
                    var foundObject = FindRecursiveWrapper(wrappedObjects[i], objName);
                    if (foundObject != null)
                    {
                        return foundObject;
                    }
                }
            }
            return null;
        }

        public WrappedPackageObject FindObjectWrapper(string fullName)
        {
            fullName = fullName.Replace(Name + ".", string.Empty);
            foreach (var obj in allSBObjects)
            {
                var comparer = (obj.Package + "." + obj.Name).Replace("null.", string.Empty);
                if (fullName.Equals(comparer, StringComparison.OrdinalIgnoreCase))
                {
                    return new WrappedPackageObject(obj);
                }
            }
            return null;
        }

        SBObject FindRecursive(WrappedPackageObject current, string objName)
        {
            if (current.sbObject.Name.Equals(objName, StringComparison.OrdinalIgnoreCase))
            {
                return current.sbObject;
            }
            for (var i = 0; i < current.ChildObjects.Count; i++)
            {
                var foundObject = FindRecursive(current.ChildObjects[i], objName);
                if (foundObject != null)
                {
                    return foundObject;
                }
            }
            return null;
        }

        WrappedPackageObject FindRecursiveWrapper(WrappedPackageObject current, string objName)
        {
            if (current.sbObject.Name.Equals(objName, StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }
            for (var i = 0; i < current.ChildObjects.Count; i++)
            {
                var foundObject = FindRecursiveWrapper(current.ChildObjects[i], objName);
                if (foundObject != null)
                {
                    return foundObject;
                }
            }
            return null;
        }

        void BuildTree()
        {
            LogString("Building tree view", Color.white);
            var objectList = wrappedPackage.Objects;
            for (var i = objectList.Count; i-- > 0;)
            {
                var sbO = objectList[i];
                allSBObjects.Add(sbO);
                var parent = sbO.Package.Replace("\0", string.Empty);
                if (parent == "null")
                {
                    var p = new WrappedPackageObject(sbO);
                    wrappedObjects.Add(p);
                    objectList.RemoveAt(i);
                }
            }
            Thread.Sleep(1);
            for (var i = wrappedObjects.Count; i-- > 0;)
            {
                AddNodes(objectList, wrappedObjects[i]);
            }
            if (objectList.Count > 0)
            {
                //LogString("Not all objects could be assigned to their (Sub/)Packages, assigning them to root!", Color.red);
                for (var i = objectList.Count; i-- > 0;)
                {
                    var pow = new WrappedPackageObject(objectList[i]);
                    wrappedObjects.Add(pow);
                }
            }
        }

        void AddNodes(List<SBObject> openList, WrappedPackageObject currentNode)
        {
            for (var i = openList.Count; i-- > 0;)
            {
                if (i >= openList.Count)
                {
                    i = openList.Count - 1;
                }
                var sbO = openList[i];
                if (IsChild(currentNode, sbO))
                {
                    var p = new WrappedPackageObject(sbO);
                    currentNode.ChildObjects.Add(p);
                    openList.Remove(sbO);
                    AddNodes(openList, p);
                }
            }
        }

        bool IsChild(WrappedPackageObject parent, SBObject possibleChild)
        {
            var parentPackage = parent.sbObject.Package.Replace("\0", string.Empty);
            var childPackage = possibleChild.Package.Replace("\0", string.Empty);
            if (string.Format("{0}.{1}", parentPackage, parent.Name).Replace("null.", string.Empty).Equals(childPackage, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public void Load(object sender, DoWorkEventArgs e)
        {
            wrappedPackage.Load(arrayDefinitions);
            wrappedObjects.Clear();
            inspectedObject = null;
            BuildTree();
            LogString("Caching internal references", Color.white);
            wrappedObjects = wrappedObjects.OrderBy(x => x.Name, new AlphanumComparer()).ToList();
            wrappedObjects.Reverse();
            e.Result = e.Argument;
            //foreach (WrappedPackageObject pw in wrappedObjects)
            //{
            //    pw.CachePropertReferences(FindObjectWrapper, null);
            //    Thread.Sleep(1);
            //}
        }

        public void ResolveReferences(List<PackageWrapper> linkedPackages)
        {
            foreach (var pw in wrappedObjects)
            {
                pw.CachePropertReferences(FindObjectWrapper, linkedPackages);
            }
        }

        public void OnDestroy()
        {
            OnLogMessage = null;
            arrayDefinitions = null;
            LogString = null;
            inspectedObject = null;
            wrappedObjects.Clear();
            wrappedPackage = null;
        }

        public void Draw(Rect r)
        {
            r = new Rect(0, 1, r.width - 1, r.height - 2);
            objectScrollViewRect.height = objectContentHeight;
            objectViewScrollPos = GUI.BeginScrollView(r, objectViewScrollPos, objectScrollViewRect);
            var elementRect = new Rect(visualOffset, visualOffset, r.width, 20);
            for (var i = wrappedObjects.Count; i-- > 0;)
            {
                DrawRecursive(ref elementRect, wrappedObjects[i]);
            }
            GUI.EndScrollView();
            objectContentHeight = elementRect.y;
        }

        void DrawRecursive(ref Rect elementRect, WrappedPackageObject current)
        {
            DrawElement(ref elementRect, current);
            if (current.ChildrenExpanded)
            {
                elementRect.x += objectSubCategoryIndent;
                for (var g = current.ChildObjects.Count; g-- > 0;)
                {
                    var child = current.ChildObjects[g];
                    DrawRecursive(ref elementRect, child);
                }
                elementRect.x -= objectSubCategoryIndent;
            }
        }

        public void DrawProperties(Rect r, GUIStyle style)
        {
            if (inspectedObject == null)
            {
                return;
            }
            r.x = 0;
            r.y = 0;
            GUILayout.BeginArea(r);
            propScrollPos = GUILayout.BeginScrollView(propScrollPos);
            var content = inspectedObject.sbObject.ToString();
            var widthHeight = style.CalcSize(new GUIContent(content));
            var curWidthHeight = widthHeight.x + widthHeight.y;
            if (prevWidthHeight != curWidthHeight)
            {
                GUIUtility.keyboardControl = 0;
                prevWidthHeight = curWidthHeight;
            }
            EditorGUILayout.SelectableLabel(content, style, GUILayout.Width(widthHeight.x), GUILayout.Height(widthHeight.y));
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawElement(ref Rect r, WrappedPackageObject p)
        {
            GUI.color = inspectedObject == p ? new Color(0.75f, 0.75f, 0.75f) : Color.white;
            if (p.ChildObjects.Count > 0)
            {
                var elementRect = new Rect(r.x, r.y, 10, r.height);
                p.ChildrenExpanded = EditorGUI.Foldout(elementRect, p.ChildrenExpanded, "");
                elementRect.x += 15;
                elementRect.width = r.width - 15;
                if (GUI.Button(elementRect, p.sbObject.Name, "Tag MenuItem"))
                {
                    inspectedObject = p;
                }
            }
            else
            {
                if (GUI.Button(r, p.sbObject.Name, "Tag MenuItem"))
                {
                    inspectedObject = p;
                }
            }
            GUI.color = Color.white;
            r.y += r.height;
        }
    }
}