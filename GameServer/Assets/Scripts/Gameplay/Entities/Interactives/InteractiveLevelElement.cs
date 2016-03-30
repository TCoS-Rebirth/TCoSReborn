using Common;
using Gameplay.Quests.QuestTargets;
using System.Collections.Generic;
using UnityEngine;
using Network;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Entities.Interactives
{
    public class InteractiveLevelElement : Entity
    {

        public ECollisionType collisionType;
        public bool isEnabled;
        public int levelObjectID;

        /// <summary>
        /// Denotes if the ILE is a 'fake' placeholder for the real object with its levelObjectID
        /// On interaction, if this is true, the server will fill in the ID for the real object
        /// , remove this ILE, and save to the game assets
        /// </summary>
        public bool isDummy = false;

        [SerializeField]
        public List<ILEAction> Actions;

        public void AssignRelID()
        {
            RetrieveRelevanceID();
        }

        public int LevelObjectID
        {
            get { return levelObjectID; }
            set { levelObjectID = value; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public ECollisionType CollisionType
        {
            get { return collisionType; }
            set { collisionType = value; }
        }

        public void onUse(PlayerCharacter source, ERadialMenuOptions menuOption)
        {
            if (!isDummy)
            {
                #region Actions
                //Iterate through actions list
                foreach (var action in Actions)
                {
                    if (action.menuOption != menuOption) continue;  //Skip if non-matching menu option
                                                                    //if (action.Actions.Count == 0) continue;        //Skip if no events to execute

                    //Requirements check - unecessary if client-side?
                    /*
                    bool reqsMet = true;
                    foreach (var req in action.Requirements)
                    {
                        if (!req.isMet(source))
                        {
                            reqsMet = false;
                            break;
                        }
                    }
                    if (!reqsMet) continue; //If a req failed, skip to next action
                    */

                    //Execute actions
                    foreach (var ev in action.Actions)
                    {
                        ev.Execute(source);
                    }

                    //Handle quest target interactions

                    if (action is ILEQTAction)
                    {
                        var qtAction = action as ILEQTAction;

                        switch (qtAction.menuOption)
                        {
                            case ERadialMenuOptions.RMO_LOOT:
                                //QT_Take
                                doQTTake(source, qtAction);
                                break;

                            case ERadialMenuOptions.RMO_USE:
                                //TODO
                                break;
                        }
                    }
                }
                #endregion
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
                    return;
                }

                //Assign to real object
                //saves to asset file ONLY if using Unity editor
                closestIE.assignLOIDAndSave(levelObjectID, source.ActiveZone.name);

                //Valshaaran - dirty temp hack
                //Re-assign the dummy's relevance ID to the new IE
                closestIE.RelevanceID = RelevanceID;

                //Call onUse on the real object
                closestIE.onUse(source, menuOption);

                //Remove this ILE and game object
                source.ActiveZone.RemoveFromZone(this);
                Destroy(this.gameObject);
                Destroy(this);

                #endregion
            }

        }

        protected void lootItem(PlayerCharacter source)
        {
            /*var tag = Name;
            
            //Get current quests, look for tag
            source.quest
            */
        }

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
    }
}