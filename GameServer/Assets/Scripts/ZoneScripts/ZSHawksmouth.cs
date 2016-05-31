using Common;
using Gameplay.Entities;
using Gameplay.Entities.Interactives;
using Network;
using UnityEngine;

namespace ZoneScripts
{
    /// <summary>
    ///     This class currently enables the ILE's for the player client (for debug purposes) TODO find out how to extract them
    ///     properly, and place them in their zones
    /// </summary>
    public class ZSHawksmouth : ZoneScript
    {
        #region Temporary

        public override void OnAfterLoaded()
        {
            SpawnFixedLevelElements();
        }

        void SpawnFixedLevelElements()
        {
            for (var i = 0; i < 79; ++i)
            {
                //Crashing ids (not InteractiveLevelElements but GameActors...)
                if (i == 12 || i == 62 || i == 65)
                {
                    continue;
                }

                //Skip if an ILE with this level object ID already exists
                bool loIDExists = false;
                foreach (var existingILE in AttachedZone.InteractiveElements)
                {
                    if (i == existingILE.LevelObjectID)
                    {
                        loIDExists = true;
                        break;
                    }
                }
                if (loIDExists) continue;

                var go = new GameObject("InteractiveElement_" + i);
                var ie = go.AddComponent<InteractiveLevelElement>();
                ie.LevelObjectID = i;
                ie.Name = "Unknown";
                ie.InitColl = ECollisionType.COL_Blocking;
                ie.InitEnabled = true;
                ie.isDummy = true;
                ie.AssignRelID();
                AttachedZone.AddToZone(ie);
            }
        }

        public override void OnPlayerEntered(PlayerCharacter pc)
        {
            ActivateAllDummyElements(pc);
        }

        void ActivateAllDummyElements(PlayerCharacter pc)
        {
            for (var i = AttachedZone.InteractiveElements.Count; i-- > 0;)
            {
                var ie = AttachedZone.InteractiveElements[i];
                if (ie.LevelObjectID >= 0 && ie.isDummy)
                {
                    var m = PacketCreator.S2C_INTERACTIVELEVELELEMENT_ADD(ie);
                    pc.SendToClient(m);
                }
            }
        }

        #endregion
    }
}