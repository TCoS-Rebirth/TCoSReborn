using Database.Dynamic.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities.Players
{
    [Serializable]
    public class PersistentVarsContainer : ScriptableObject
    {
        //[ReadOnly]
        public List<PersistentVar> varsList = new List<PersistentVar>();

        public void SetVar(int contextID, int varID, int value)
        {
            foreach(var pv in varsList)
            {
                if (pv.ContextID == contextID && pv.VarID == varID)
                {
                    pv.Value = value;
                    return;
                }
            }
            var newPV = new PersistentVar();
            newPV.ContextID = contextID;
            newPV.VarID = varID;
            newPV.Value = value;
            varsList.Add(newPV);
        }
        public int GetValue(int contextID, int varID)
        {
            foreach(var pv in varsList)
            {
                if (pv.ContextID == contextID && pv.VarID == varID)
                {
                    return pv.Value;
                }
            }
            return 0;
        }
        public void LoadForPlayer(List<DBPersistentVar> dbVars, PlayerCharacter pc)
        {
            varsList = new List<PersistentVar>();

            foreach (var dbVar in dbVars)
            {
                var playerVar = new PersistentVar();
                playerVar.ContextID = dbVar.ContextId;
                playerVar.VarID = dbVar.VarId;
                playerVar.Value = dbVar.Value;
                varsList.Add(playerVar);                                    
            }

            pc.persistentVars = this;
        }
        public List<DBPersistentVar> SaveForPlayer()
        {
            List<DBPersistentVar> output = new List<DBPersistentVar>();

            foreach (var v in varsList)
            {
                var dbVar = new DBPersistentVar(v.ContextID, v.VarID, v.Value);
                    output.Add(dbVar);
            }
            return output;
        }
    }
}
