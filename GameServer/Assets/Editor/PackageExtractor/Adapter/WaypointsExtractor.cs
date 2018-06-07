using System;
using System.Collections.Generic;
using Common;
using Database.Static;
using UnityEditor;
using UnityEngine;
using Utility;
using World;
using World.Paths;
using Object = UnityEngine.Object;

namespace PackageExtractor.Adapter
{
    public class WaypointsExtractor : ExtractorAdapter
    {
        Zone targetZone;

        TupleList<PatrolPoint, WrappedPackageObject> temporary = new TupleList<PatrolPoint, WrappedPackageObject>();

        public override string Name
        {
            get { return "Waypoints Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract the available Paths for the given Map and set up references if possible"; }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Target zone");
            targetZone = EditorGUILayout.ObjectField(targetZone, typeof (Zone), true) as Zone;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (targetZone == null)
            {
                Log("Select a zone first", Color.yellow);
                return;
            }
            var pathHolder = targetZone.transform.Find("Waypoints");
            if (pathHolder == null)
            {
                pathHolder = new GameObject("Waypoints").transform;
                pathHolder.parent = targetZone.transform;
                pathHolder.localPosition = Vector3.zero;
            }
            else
            {
                for (var i = pathHolder.childCount; i-- > 0;)
                {
                    Object.DestroyImmediate(pathHolder.GetChild(i).gameObject);
                }
            }
            temporary.Clear();
            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("PatrolPoint", StringComparison.OrdinalIgnoreCase))
                {
                    var p = ExtractPatrolPoint(wpo, pathHolder);
                    temporary.Add(p, wpo);
                }
            }
            foreach (var prepared in temporary)
            {
                var prop = prepared.Item2.FindProperty("PatrolPaths");
                if (prop != null)
                {
                    foreach (var pathProp in prop.IterateInnerProperties())
                    {
                        if (pathProp.Value.Replace("\0", string.Empty) != "null")
                        {
                            var foundPathObject = extractorWindowRef.ActiveWrapper.FindObjectWrapper(pathProp.Value.Replace("\0", string.Empty));
                            if (foundPathObject != null)
                            {
                                var path = ExtractAIPath(pathHolder, foundPathObject);
                                prepared.Item1.PatrolPaths.Add(path);
                                //if (path.Path.Count > 0)
                                //{
                                //    prepared.item1.transform.position = path.Path[0];
                                //}
                            }
                        }
                    }
                }
                var connectionsProp = prepared.Item2.FindProperty("Connections");
                if (connectionsProp != null)
                {
                    foreach (var singleConnectionProp in connectionsProp.IterateInnerProperties())
                    {
                        var con = ExtractConnection(pathHolder, singleConnectionProp);
                        prepared.Item1.Connections.Add(con);
                    }
                }
            }
            temporary.Clear();
        }

        PatrolPoint ExtractPatrolPoint(WrappedPackageObject wpo, Transform pathHolder)
        {
            var go = new GameObject(wpo.Name);
            Vector3 loc;
            if (ReadVector3(wpo, "Location", out loc))
            {
                go.transform.position = UnitConversion.ToUnity(loc);
            }
            var p = go.AddComponent<PatrolPoint>();
            p.name = wpo.Name.Replace("\0", string.Empty);
            //public List<PathConnection> Connections; //second iteration
            ReadEnum(wpo, "AnnotationType", out p.AnnotationType);
            ReadBool(wpo, "CreateNode", out p.CreateNode);
            ReadVector3(wpo, "Extent", out p.Extent);
            p.Extent = UnitConversion.ToUnity(p.Extent);
            ReadBool(wpo, "FitNode", out p.FitNode);
            List<byte> annoMask;
            if (ReadByteArray(wpo, "AnnotationMask", out annoMask))
            {
                foreach (var b in annoMask)
                {
                    p.AnnotationMask.Add((EAnnotationType) b);
                }
            }
            ReadString(wpo, "AnnotationScript", out p.AnnotationScript);
            ReadBool(wpo, "TriggerScript", out p.TriggerScript);
            ReadBool(wpo, "WaitForScript", out p.WaitForScript);
            ReadBool(wpo, "PausePatrolScript", out p.PausePatrolScript);
            ReadBool(wpo, "bWalking", out p.bWalking);
            ReadFloat(wpo, "MinimalPathHeight", out p.MinimalPathHeight);
            var groupRefProp = wpo.FindProperty("ActorGroups");
            if (groupRefProp != null)
            {
                foreach (var groupProp in groupRefProp.IterateInnerProperties())
                {
                    var groupObject = extractorWindowRef.ActiveWrapper.FindObjectWrapper(groupProp.GetValue<string>());
                    if (groupObject != null)
                    {
                        string groupDescription;
                        if (ReadString(groupObject, "Description", out groupDescription))
                        {
                            p.groups.Add(groupDescription.Replace("\0", string.Empty));
                        }
                    }
                }
            }
            //public List<AIPath> PatrolPaths; //second iteration
            go.transform.parent = pathHolder;
            return p;
        }

        AIPath ExtractAIPath(Transform pathHolder, WrappedPackageObject wpo)
        {
            var aip = new AIPath();
            var vectorListProp = wpo.FindProperty("Path");
            if (vectorListProp != null)
            {
                foreach (var v in vectorListProp.IterateInnerProperties())
                {
                    Vector3 vec;
                    if (TryConvertToVector3(v.Value.Replace("\0", string.Empty), out vec))
                    {
                        vec = UnitConversion.ToUnity(vec);
                        aip.Path.Add(vec);
                    }
                }
            }
            string controlPointName;
            if (ReadString(wpo, "ControlPoint", out controlPointName))
            {
                var parts = controlPointName.Split('.');
                if (parts.Length > 0)
                {
                    foreach (Transform t in pathHolder)
                    {
                        if (t.name.Equals(parts[parts.Length - 1], StringComparison.OrdinalIgnoreCase))
                        {
                            aip.ControlPoint = t.GetComponent<AnnotationPoint>();
                        }
                    }
                }
            }
            ReadBool(wpo, "Complete", out aip.Complete);
            return aip;
        }

        PathPoint.PathConnection ExtractConnection(Transform pathHolder, SBProperty wpo)
        {
            var con = new PathPoint.PathConnection();
            SBProperty acProp;
            if (wpo.Array.TryGetValue("ConnectedActor", out acProp))
            {
                var connectionActorName = acProp.Value.Replace("\0", string.Empty);
                if (connectionActorName != "null")
                {
                    var parts = connectionActorName.Split('.');
                    if (parts.Length > 0)
                    {
                        foreach (Transform t in pathHolder)
                        {
                            if (t.name.Equals(parts[parts.Length - 1], StringComparison.OrdinalIgnoreCase))
                            {
                                con.ConnectedActor = t.GetComponent<PathPoint>();
                                break;
                            }
                        }
                    }
                }
            }
            if (wpo.Array.TryGetValue("MoveSpeed", out acProp))
            {
                con.MoveSpeed = acProp.GetValue<float>();
            }
            if (wpo.Array.TryGetValue("Walking", out acProp))
            {
                con.Walking = acProp.GetValue<bool>();
            }
            return con;
        }
    }
}