using System;
using Common;
using Database.Static;
using UnityEditor;
using UnityEngine;
using Utility;
using World;
using Object = UnityEngine.Object;
using Common.UnrealTypes;

namespace PackageExtractor.Adapter
{
    public class DestinationAndPortalExtractor : ExtractorAdapter
    {
        Zone targetZone;

        public override string Name
        {
            get { return "Destination/Portal Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract the TravelDestinations and Portals and place them in the provided zone."; }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("Target zone");
            targetZone = EditorGUILayout.ObjectField(targetZone, typeof (Zone), true) as Zone;
        }

        public override bool IsCompatible(PackageWrapper p)
        {
            //use as callback
            foreach (var z in Object.FindObjectsOfType<Zone>())
            {
                if (z.name.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
                {
                    targetZone = z;
                    break;
                }
            }
            return true;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (targetZone == null)
            {
                Log("Select a zone first", Color.yellow);
                return;
            }

            #region Destinations

            var destHolder = targetZone.transform.Find("Destinations");
            if (destHolder == null)
            {
                Log("DestinationsHolder not found", Color.yellow);
                return;
            }
            for (var i = destHolder.childCount; i-- > 0;)
            {
                Object.DestroyImmediate(destHolder.GetChild(i).gameObject);
            }
            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("PlayerStart", StringComparison.Ordinal))
                {
                    Vector3 loc;
                    Rotator unrealRot;
                    Quaternion rot = new Quaternion(); 
                    var navTag = "";
                    if (ReadVector3(wpo, "Location", out loc))
                    {
                        loc = UnitConversion.ToUnity(loc);
                    }
                    if (ReadRotator(wpo, "Rotation", out unrealRot))
                    {
                        rot = UnitConversion.ToUnity(unrealRot);                           
                    }
                    ReadString(wpo, "NavigationTag", out navTag);
                    if (navTag.Length <= 1)
                    {
                        navTag = "[UNKNOWN NAVIGATION TAG]";
                    }
                    var go = new GameObject(wpo.Name);
                    var dest = go.AddComponent<PlayerStart>();
                    dest.NavigationTag = navTag;
                    go.transform.parent = destHolder;
                    go.transform.position = loc;
                    go.transform.rotation = rot;
                    dest.IsRespawn = navTag.Contains("Respawn") | navTag.Contains("respawn");
                }
            }

            #endregion

            #region Portals

            var portalHolder = targetZone.transform.Find("Portals");
            if (portalHolder == null)
            {
                Log("Portalholder not found", Color.yellow);
                return;
            }
            for (var i = portalHolder.childCount; i-- > 0;)
            {
                Object.DestroyImmediate(portalHolder.GetChild(i).gameObject);
            }
            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (!wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("SBWorldPortal", StringComparison.Ordinal)) continue;
                Vector3 loc;
                var navTag = "";
                if (ReadVector3(wpo, "Location", out loc))
                {
                    loc = UnitConversion.ToUnity(loc);
                }
                ReadString(wpo, "NavigationTag", out navTag);
                if (navTag.Length <= 1)
                {
                    navTag = "[UNKNOWN NAVIGATION TAG]";
                }
                var go = new GameObject(wpo.Name);
                var port = go.AddComponent<SBWorldPortal>();
                go.transform.parent = portalHolder;
                go.transform.position = loc;
                port.owningZone = targetZone;
                if (ReadFloat(wpo, "CollisionRadius", out port.collisionRadius))
                {
                    port.collisionRadius = UnitConversion.UnrUnitsToMeters*port.collisionRadius;
                }
                if (ReadFloat(wpo, "CollisionHeight", out port.collisionHeight))
                {
                    port.collisionHeight = UnitConversion.UnrUnitsToMeters*port.collisionHeight;
                }
            }

            #endregion
        }
    }
}