/* Alteration of DestinationExtractor to populate InteractiveElements with door objects
	Can hopefully be eventually refactored as part of a InteractiveElementExtractor adaptor :)
*/

using Common;
using Database.Static;
using Gameplay.Entities;
using UnityEditor;
using UnityEngine;
using Utility;
using World;

namespace PackageExtractor.Adapter
{
    public class DoorExtractor : ExtractorAdapter
    {
        Zone targetZone;

        public override string Name
        {
            get { return "Door Extractor"; }
        }

        public override string Description
        {
            get { return "Tries to extract the Door type InteractiveLevelElements, place them in the provided zone, and link to a zone destination"; }
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
            var destHolder = targetZone.transform.FindChild("Destinations");
            if (destHolder == null)
            {
                Log("DestinationsHolder not found", Color.yellow);
                return;
            }

            var ieHolder = targetZone.transform.FindChild("InteractiveElements");
            if (ieHolder == null)
            {
                Log("InteractiveElementsHolder not found", Color.yellow);
                return;
            }

            for (var i = ieHolder.childCount; i-- > 0;)
            {
                Object.DestroyImmediate(ieHolder.GetChild(i).gameObject);
            }
            foreach (var wpo in extractorWindowRef.ActiveWrapper.IterateObjects())
            {
                if (wpo.sbObject.ClassName.Replace("\0", string.Empty).EndsWith("InteractiveLevelElement"))
                {
                    Vector3 loc;
                    Vector3 rot;
                    var dTag = "";
                    var goid = -1;

                    int.TryParse(wpo.sbObject.Name.Replace("InteractiveLevelElement", string.Empty), out goid);
                    //int.TryParse(wpo.sbObject.Name.Replace("\0", string.Empty), out goid);

                    if (ReadVector3(wpo, "Location", out loc))
                    {
                        loc = UnitConversion.ToUnity(loc);
                    }
                    if (ReadVector3(wpo, "Rotation", out rot))
                    {
                        rot = UnitConversion.ToUnity(rot);
                    }
                    if (ReadString(wpo, "Tag", out dTag))
                    {
                        if (dTag.Contains("entrance")
                            || dTag.Contains("exit")
                            || dTag.Contains("Entrance")
                            || dTag.Contains("Exit"))
                        {
                            var go = new GameObject("door_" + dTag);
                            //PlayerStart dest = go.AddComponent<PlayerStart>();
                            var door = go.AddComponent<InteractiveLevelElement>();
                            door.Name = dTag;
                            door.ActiveZone = targetZone;
                            door.LevelObjectID = goid;
                            door.CollisionType = ECollisionType.COL_Blocking;
                            go.transform.parent = ieHolder;
                            go.transform.position = loc;
                            go.transform.rotation = Quaternion.LookRotation(rot);
                            //dest.isRespawn = navTag.Contains("Respawn") | navTag.Contains("respawn");

                            /*Disabled - destination name tags don't match door tags
							 * - could implement Levenshtein distance algorithm to match close strings?
							//Try to match door's name tag to a destination's name key
							foreach(Transform destIn in destHolder) {
								if (destIn.tag.Contains(dTag)) {

									//and set the destination object to it
									door.setDestination(destIn.transform);
									break;

								}
							}

							*/
                        }
                    }
                }
            }
        }
    }
}