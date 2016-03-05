using Database.Dynamic.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities.Players
{
    [Serializable]
    public class QuestDataContainer : ScriptableObject
    {
        public List<PlayerQuestProgress> curQuests = new List<PlayerQuestProgress>();
        public List<int> completedQuestIDs = new List<int>();

        public void CompleteQuest(int questID)
        {
            //Adds completed quest ID to completed list, removes entries with that ID from curQuests
            completedQuestIDs.Add(questID);
            RemoveQuest(questID);
        }

        public int getNumTargets(int questID)   
        {
            foreach (var curQuest in curQuests)
            {
                if (curQuest.questID == questID)
                {
                    return curQuest.targetProgress.Count;                    
                }
            }
            return 0; //return 0 if player doesn't have quest active
        }

        public void RemoveQuest(int questID)
        {
            foreach (var curQuest in curQuests)
            {
                if (curQuest.questID == questID)
                {
                    curQuests.Remove(curQuest);
                    return;
                }
            }
        }

        public void NewQuest(int questID, int numTargets)
        {
            var newQuestProgress = new PlayerQuestProgress(questID, new List<int>(3));
            curQuests.Add(newQuestProgress);
        }

        public void UpdateQuest(int questID, int targetIndex, int progress)
        {
            foreach (var curQuest in curQuests)
            {
                if (curQuest.questID == questID)
                {
                    curQuest.targetProgress[targetIndex] = progress;
                    return;
                }
            }
        }

        public void LoadForPlayer(List<DBQuestTarget> quests, PlayerCharacter pc)
        {
            //TODO : Implementation appears to work ok now with simple parameters, but may want revisiting

            curQuests = new List<PlayerQuestProgress>();
            completedQuestIDs = new List<int>();


            foreach (var dbQT in quests)
            {
                if (dbQT.isCompleted)
                {
                    completedQuestIDs.Add(dbQT.ResourceId);
                }
                else
                {
                    bool idExists = false;
                    int entryIndex = 0;

                    for (int n = 0; n < curQuests.Count; n++)
                    {
                        if (dbQT.ResourceId == curQuests[n].questID)
                        {
                            idExists = true;
                            entryIndex = n;
                            break;
                        }
                    }


                    if (!idExists)
                    {
                        entryIndex = curQuests.Count;
                        curQuests.Add(new PlayerQuestProgress(dbQT.ResourceId, new List<int>()));
                    }

                    //While curQuest progress list length < new target index, add empty item to list
                    var listLength = curQuests[entryIndex].targetProgress.Count;

                    for (int n = listLength; n < dbQT.targetIndex; n++)
                    {
                        curQuests[entryIndex].targetProgress.Add(0);
                    }

                    //Insert the new progress value at its index position   
                    if (curQuests[entryIndex].targetProgress.Count <= dbQT.targetIndex) {
                        curQuests[entryIndex].targetProgress.Add(0);
                    }
                    curQuests[entryIndex].targetProgress[dbQT.targetIndex] = dbQT.targetProgress;

                }
            }

            pc.QuestData = this;
        }

        public List<DBQuestTarget> SaveForPlayer()
        {
            List<DBQuestTarget> output = new List<DBQuestTarget>();

            foreach (var q in curQuests)
            {
                for (int t = 0; t < q.targetProgress.Count; t++)
                {
                    var dbQT = new DBQuestTarget(q.questID, false, t);
                    dbQT.targetProgress = q.targetProgress[t];
                    output.Add(dbQT);
                }
            }

            foreach (var q in completedQuestIDs)
            {
                output.Add(new DBQuestTarget(q, true, 0));
            }

            return output;
        }
    }
}
