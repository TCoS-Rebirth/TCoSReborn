using Common;
using Gameplay.Quests.QuestTargets;
using System.Collections.Generic;
using UnityEngine;
using Network;
using World;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Entities.Interactives
{
    public class InteractiveLevelElement : Entity
    {

        public int levelObjectID;

        /// <summary>
        /// Denotes if the ILE is a 'fake' placeholder for the real object with its levelObjectID
        /// On interaction, if this is true, the server will fill in the ID for the real object
        /// , remove this ILE, and save to the game assets
        /// </summary>
        public bool isDummy = false;

        [ReadOnly]
        public EILECategory ileType;

        [ReadOnly]
        public List<ILEAction> Actions;

        [ReadOnly]
        public bool IsActivated;

        [SerializeField]
        int currentOptionIndex = -1;

        [SerializeField]
        ERadialMenuOptions currentOption;

        [SerializeField]
        int currentSubAction;

        [SerializeField]
        PlayerCharacter targetPawn;

        [SerializeField]
        bool reverse;

        public void AssignRelID()
        {
            RetrieveRelevanceID();
        }

        public int LevelObjectID
        {
            get { return levelObjectID; }
            set { levelObjectID = value; }
        }

        protected InteractionComponent currentComponent
        {
            get { return Actions[currentOptionIndex].StackedActions[currentSubAction]; }
        }

        protected int curCompStackSize
        {
            get { return Actions[currentOptionIndex].StackedActions.Count; }
        }

        public void SetActivated(bool active)
        {
            IsActivated = active;
            Message m = PacketCreator.S2R_INTERACTIVELEVELELEMENT_SV2CLREL_UPDATENETISACTIVATED(this, IsActivated);
            BroadcastRelevanceMessage(m);
        }

        public void StartClientSubAction()
        {
            Message m = PacketCreator.S2R_INTERACTIVELEVELELEMENT_SV2CLREL_STARTCLIENTSUBACTION(this, currentOptionIndex, currentSubAction, reverse, targetPawn);
            BroadcastRelevanceMessage(m);

            //currentComponent.onStart(this, targetPawn, reverse);
        }

        [ContextMenu("Force CancelClientSubAction")]
        public void CancelClientSubAction()
        {
            Message m = PacketCreator.S2R_INTERACTIVELEVELELEMENT_SV2CLREL_CANCELCLIENTSUBACTION(this, currentOptionIndex, currentSubAction);
            BroadcastRelevanceMessage(m);

            //currentComponent.onCancel(this, targetPawn);
        }

        [ContextMenu("Force EndClientSubAction")]
        public void EndClientSubAction()
        {
            Message m = PacketCreator.S2R_INTERACTIVELEVELELEMENT_SV2CLREL_ENDCLIENTSUBACTION(this, currentOptionIndex, currentSubAction, reverse);
            BroadcastRelevanceMessage(m);

            //currentComponent.onEnd(this, targetPawn, reverse);
        }

        public void StartOptionActions()
        {
            SetActivated(true);

            if (targetPawn)
            {
                reverse = false;
                currentSubAction = 0;
            }

            currentComponent.onStart(targetPawn, reverse);
        }

        [ContextMenu("Force CancelOptionActions")]
        public void CancelOptionActions()
        {
            if (IsActivated)
            {
                currentComponent.onCancel(targetPawn);
            }

            EndOptionActions();
        }

        public void NextSubAction()
        {
            if (IsActivated)
            {
                currentComponent.onEnd(targetPawn, reverse);
            }

            if (!reverse)
            {
                if (currentSubAction < (curCompStackSize - 1))
                {
                    currentSubAction++;
                    currentComponent.onStart(targetPawn, reverse);
                }
                else
                {
                    EndOptionActions();
                }
            }
            else
            {

            }
        }
        
        public void EndOptionActions()
        {
            //Do the reverse actions
            reverse = true;

            var subactions = Actions[currentOptionIndex].StackedActions;

            while (currentSubAction > -1)
            {
                var subaction = subactions[currentSubAction];
                if (subaction.Reverse)
                {
                    subaction.onStart(targetPawn, reverse);
                    subaction.onEnd(targetPawn, reverse);
                }
                currentSubAction--;
            }
            
            currentOptionIndex = -1;
            currentSubAction = -1;
            reverse = false;
            SetActivated(false);
        }
        

        public bool onRadialMenuOption(PlayerCharacter source, ERadialMenuOptions menuOption)
        {
            if (!isDummy)
            {
                if (!Enabled || IsActivated) { return false; }

                currentOptionIndex = -1;

                for (int n = 0; n < Actions.Count; n++)
                {
                    var action = Actions[n];
                    if (action.menuOption == menuOption
                        && action.isEligible(source))
                    {
                        currentOptionIndex = n;
                        break;
                    }
                }

                if (!Actions[currentOptionIndex].isEligible(source))
                {
                    return false;
                }

                targetPawn = source;
                currentOption = menuOption;
                StartOptionActions();
                return true;
            }
        
            else
            {
                #region Assign dummy ID to real ILE
                //Find closest non-dummy ILE in zone
                var zoneIEs = source.ActiveZone.InteractiveElements;
                InteractiveLevelElement closestIE = null;

                foreach(var ie in zoneIEs)
                {
                    if (closestIE == null)
                    {
                        closestIE = ie;
                    }

                    //TODO: Refine this to avoid linking the wrong ILE(e.g. Hawksmouth Academy doors)
                    //compares distances
                    if (    Vector3.Distance(source.transform.position, ie.transform.position) 
                        <=  Vector3.Distance(source.transform.position, closestIE.transform.position))
                        {
                        if (!ie.isDummy) closestIE = ie;
                    }
                }

                if (closestIE == null)
                {
                    Debug.Log("InteractiveLevelElement.onUse : failed to get ILE closest to player");
                    return false;
                }

                //Assign to real object
                //saves to asset file ONLY if using Unity editor
                closestIE.assignLOIDAndSave(levelObjectID, source.ActiveZone.name);

                //Valshaaran - dirty temp hack
                //Re-assign the dummy's relevance ID to the new IE
                closestIE.RelevanceID = RelevanceID;

                //Now act on the real object
                bool success = closestIE.onRadialMenuOption(source, menuOption);

                //Remove this ILE and game object
                source.ActiveZone.RemoveFromZone(this);
                Destroy(gameObject);
                Destroy(this);

                return success;

                #endregion
            }

        }

        /*
        protected void doQTTake(PlayerCharacter source, ILEQTAction qtAction)
        {
            var qtTake = qtAction.Quest.targets[qtAction.TargetIndex] as QT_Take;

            if (source.TryAdvanceTarget(qtAction.Quest, qtTake))
            {
                //Server adds item to inventory
                //source.GiveInventory(qtTake.Cargo);
            }
            else
            {
                Debug.Log("InteractiveLevelElement.onUse : failed to progress a qtTake target");
            }                        
        }
        */
        
        public void assignLOIDAndSave(int newID, string zoneName)
        {            

            levelObjectID = newID;

            #if UNITY_EDITOR

            string assetPath = "Assets/GameData/Interactives/" + zoneName + ".asset";

            
            var iidCol = AssetDatabase.LoadAssetAtPath<ILEIDCollection>(assetPath);
      
            if (iidCol == null)
            {
                //New asset

                iidCol = ScriptableObject.CreateInstance<ILEIDCollection>();
                AssetDatabase.CreateAsset(iidCol, assetPath);
            }

            //Create ileID obj
            var ileID = ScriptableObject.CreateInstance<ILEID>();
            ileID.name = newID + "-" + name;
            ileID.gameObjName = name;
            ileID.levelObjID = newID;


            if (iidCol.TryAdd(ileID))
            {
                AssetDatabase.AddObjectToAsset(ileID, iidCol);
                ileID.name = newID + "-" + name;
                //EditorUtility.SetDirty(iidCol);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(assetPath);
            }

#endif
        }

        //Update interaction components
        public override void UpdateEntity()
        {
            //Update components of current non-null index
            if (currentOptionIndex == -1 || !IsActivated || reverse) return;
            var curAction = Actions[currentOptionIndex];
            foreach (var comp in curAction.StackedActions)
            {
                comp.Update();
            }
        }
    }
}