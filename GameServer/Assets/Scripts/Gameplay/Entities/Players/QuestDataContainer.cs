using Database.Dynamic.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities.Players
{
    [Serializable]
    public class QuestDataContainer : ScriptableObject
    {
        public List<PlayerQuestProgress> curQuests;
        public List<int> completedQuestIDs;

        public void CompleteQuest(int questID)
        {
            //Adds completed quest ID to completed list, removes entries with that ID from curQuests
            completedQuestIDs.Add(questID);
            RemoveQuest(questID);
        }

        public void RemoveQuest(int questID)
        {
            foreach (var curQuest in curQuests)
            {
                if (curQuest.questID == questID)
                {
                    curQuests.Remove(curQuest);
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
            pc.QuestData = ScriptableObject.CreateInstance<QuestDataContainer>();

            foreach (var dbQT in quests)
            {
                if (dbQT.isCompleted)
                {
                    pc.QuestData.completedQuestIDs.Add(dbQT.ResourceId);
                }
                else
                {

                    bool idExists = false;
                    int entryIndex = 0;
                    for (int n = 0; n < pc.QuestData.curQuests.Count; n++)
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
                        entryIndex = pc.QuestData.curQuests.Count;
                        pc.QuestData.curQuests.Add(new PlayerQuestProgress(dbQT.ResourceId, new List<int>()));
                    }

                    //While curQuest progress list length < new target index, add empty item to list
                    var listLength = pc.QuestData.curQuests[entryIndex].targetProgress.Count;

                    for (int n = listLength; n < dbQT.targetIndex; n++)
                    {

                        pc.QuestData.curQuests[entryIndex].targetProgress.Add(0);
                    }

                    //Insert the new progress value at its index position    
                    pc.QuestData.curQuests[entryIndex].targetProgress[dbQT.targetIndex] = dbQT.targetProgress;

                }
            }
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
