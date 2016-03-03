using System;
using Common;
using Database.Static;
using UnityEngine;
using World;
using Object = UnityEngine.Object;

namespace PackageExtractor.Adapter
{
    public class CompleteUniverseFileExtractor : ExtractorAdapter
    {
        bool portalsOnly;

        public override string Name
        {
            get { return "CompleteUniverse.sbu Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract the SBWorld information into each zone"; }
        }

        public override void DrawGUI(Rect r)
        {
            GUILayout.Label("This option is somewhat slow");
            portalsOnly = GUILayout.Toggle(portalsOnly, "Portals and Routes");
        }

        bool FindZone(int id, out Zone z)
        {
            var zones = Object.FindObjectsOfType<Zone>();
            foreach (var zone in zones)
            {
                if ((int) zone.ID != id) continue;
                z = zone;
                return true;
            }
            z = null;
            return false;
        }


        bool FindSBWorldAndZone(string internalWorldName, out WrappedPackageObject wpo, out Zone zone)
        {
            foreach (var wp in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (!IsOfClass(wp, "SBWorld")) continue;
                int sbWorldID;
                Zone z;
                if (ReadInt(wp, "worldID", out sbWorldID) && wp.Name.Replace("\0", string.Empty).Equals(internalWorldName, StringComparison.OrdinalIgnoreCase) &&
                    FindZone(sbWorldID, out z))
                {
                    wpo = wp;
                    zone = z;
                    return true;
                }
            }
            wpo = null;
            zone = null;
            return false;
        }

        public override void HandlePackageContent(WrappedPackageObject wrappedObject, SBResources resources, SBLocalizedStrings localizedStrings)
        {
            if (portalsOnly)
            {
                var portals = Object.FindObjectsOfType<SBWorldPortal>();
                foreach (var portal in portals)
                {
                    portal.Destination = null;
                    portal.PortalTag = "";
                    portal.TargetZone = null;
                }
            }
            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (!portalsOnly)
                {
                    if (IsOfClass(wpo, "SBWorld"))
                    {
                        int sbWorldID;
                        Zone z = null;
                        if (ReadInt(wpo, "worldID", out sbWorldID) && FindZone(sbWorldID, out z))
                        {
                            z.InternalName = wpo.Name;
                            ReadString(wpo, "WorldName", out z.ReadableName);
                            ReadString(wpo, "WorldFile", out z.PackageFileName);
                            int wType;
                            if (ReadInt(wpo, "WorldType", out wType))
                            {
                                z.WorldType = (eZoneWorldTypes) wType;
                            }
                            ReadBool(wpo, "InstanceAutoDestroy", out z.InstanceAutoDestroy);
                        }
                        if (z == null)
                        {
                            string worldName;
                            ReadString(wpo, "WorldName", out worldName);
                            if (worldName == "cc") continue;
                            Debug.Log("Zone not found: " + sbWorldID + "(" + wpo.Name + "/" + worldName + ")");
                        }
                    }
                }
                else
                {
                    if (IsOfClass(wpo, "Game_Route"))
                    {
                        var packageName = wpo.sbObject.Package.Replace("\0", string.Empty);
                        packageName =
                            packageName.Substring(packageName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase),
                                packageName.Length - packageName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase)).Replace(".", string.Empty);
                        WrappedPackageObject worldPackage;
                        Zone zone;
                        if (FindSBWorldAndZone(packageName, out worldPackage, out zone))
                        {
                            var sbwp = zone.FindPortal(wpo.Name.Replace("\0", string.Empty));
                            if (!sbwp)
                            {
                                Debug.Log("Portal not found in zone: " + wpo.Name);
                                continue;
                            }
                            string destWorld, destPortal, worldPortalTag;
                            if (ReadString(wpo, "DestinationWorld", out destWorld) &&
                                ReadString(wpo, "DestinationPortal", out destPortal) &&
                                ReadString(wpo, "WorldPortalTag", out worldPortalTag))
                            {
                                packageName =
                                    destWorld.Substring(destWorld.LastIndexOf(".", StringComparison.OrdinalIgnoreCase),
                                        destWorld.Length - destWorld.LastIndexOf(".", StringComparison.OrdinalIgnoreCase)).Replace(".", string.Empty);
                                WrappedPackageObject targetZoneWorldPackage;
                                Zone targetZone;
                                if (FindSBWorldAndZone(packageName, out targetZoneWorldPackage, out targetZone))
                                {
                                    var ps =
                                        targetZone.FindTravelDestination(
                                            destPortal.Substring(destPortal.LastIndexOf(".", StringComparison.OrdinalIgnoreCase),
                                                destPortal.Length - destPortal.LastIndexOf(".", StringComparison.OrdinalIgnoreCase)).Replace(".", string.Empty));
                                    if (ps == null)
                                    {
                                        Debug.Log("DestinationPortal not found (Playerstart): " + destPortal + " in " + targetZone);
                                        continue;
                                    }
                                    sbwp.Destination = ps;
                                    sbwp.PortalTag = worldPortalTag;
                                    sbwp.TargetZone = targetZone;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}