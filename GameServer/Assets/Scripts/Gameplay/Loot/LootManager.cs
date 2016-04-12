using Common;
using Gameplay.Entities;
using Gameplay.Items;
using Network;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Loot
{
    /// <summary>
    /// Valshaaran - the loot system implementation changes drastically between
    /// builds - this is based on an older version (TCOS1006SDK)
    /// This and all loot classes will need updating if moving to later builds
    /// </summary>
    public class LootManager
    {

        public const float DEFAULT_TIMER = 120.0f;
        private static LootManager _instance;
        [ReadOnly]
        public List<LootTransaction> lootTransactions;
        [ReadOnly]
        public int LastTransID;

        List<int> assignedLootIDs = new List<int>();

        float tickTimer = 0.0f;
        static float tickLength = 1.0f; //Seconds        

        public void CreateTransaction(List<LootTable> lootTables, List<PlayerCharacter> receivers, ELootMode lootMode = 0, float timer = DEFAULT_TIMER)
        {
            #region Prepare new transaction and add to transactions
            var newTransaction = new LootTransaction();

            var droppedLootItems = new List<LootTransaction.DroppedLootItem>();

            foreach(var table in lootTables)
            {
                var dis = table.GenerateLoot();
                foreach(var di in dis)
                {
                    if (di != null)
                        droppedLootItems.Add(generateDLI(di));
                }
            }
            if (droppedLootItems.Count == 0) return;
            else {
                newTransaction.LootItems = droppedLootItems;
            }

            newTransaction.TransactionID = uniqueTransID();
            newTransaction.Receivers = receivers;
            newTransaction.LootMode = lootMode;

            switch (lootMode)
            {
                case ELootMode.LM_FREE_FOR_ALL:
                    
                break;

                case ELootMode.LM_GROUP:
                    newTransaction.TimedTransaction = true;
                    newTransaction.SelectedDrops = new List<LootTransaction.GroupLootSelection>();
                    break;

                case ELootMode.LM_MASTER:

                break;
            }
            newTransaction.CurrentTimer = timer;

            lootTransactions.Add(newTransaction);
            #endregion

            #region Prepare and dispatch packet to receivers
            //Prepare replicated item list
            var replicatedItemList = new List<ReplicatedLootItem>();
            foreach (var dli in newTransaction.LootItems)
            {
                var ri = new ReplicatedLootItem();
                ri.SetupFromDLI(dli);
                replicatedItemList.Add(ri);
            }

            //Prepare eligible members
            //Valshaaran - placeholder - all receivers eligible for now
            //TODO
            var eligibleMemberList = receivers;

            Message mInitLoot = PacketCreator.S2C_GAME_LOOTING_SV2CL_INITLOOTOFFER
            (newTransaction.TransactionID,
            newTransaction.CurrentTimer,
            lootMode,
            replicatedItemList,
            eligibleMemberList);

            foreach(var player in receivers)
            {
                player.SendToClient(mInitLoot);
            }
            #endregion

        }

        public void GiveItems(PlayerCharacter pc, List<int> transIDs, List<int> lootItemIDs)
        {         
             
            for (int t = 0; t < transIDs.Count; t++)
            {

                var transID = transIDs[t];
                var lootItemID = lootItemIDs[t];

                if (!pc.ItemManager.hasFreeSpace(1))    //TODO: Allow if can be stacked with existing stack?
                {
                    //inv full notification
                    Message mRej = PacketCreator.S2C_GAME_LOOTING_SV2CL_LOOTITEMREJECTED(transID, lootItemID, ELootRejectedReason.LIR_INV_FULL);
                    pc.SendToClient(mRej);
                    RemoveAllGiven();
                    return;
                }

                bool foundTrans = false;
                foreach(var lt in lootTransactions)
                {
                    if (transID == lt.TransactionID)
                    {
                        foundTrans = true;

                        bool foundLI = false;
                        foreach(var li in lt.LootItems)
                        {
                            if (lootItemID == li.LootItemID)
                            {
                                foundLI = true;
                                if(li.Given)
                                {
                                    //Already given notification
                                    Message mRej = PacketCreator.S2C_GAME_LOOTING_SV2CL_LOOTITEMREJECTED(transID, lootItemID, ELootRejectedReason.LIR_ALREADY_TAKEN);
                                    pc.SendToClient(mRej);
                                    break;
                                }                                                                       

                                //Give item to player
                                var gameItem = ScriptableObject.CreateInstance<Game_Item>();
                                gameItem.SetupFromLoot(li);
                                pc.ItemManager.AddItem(gameItem);

                                li.Given = true;
                                break;
                            }
                        }
                        if (foundLI) break;

                    }
                    if (foundTrans) break;
                }
            }
            RemoveAllGiven();
        }

        public void EndTransactions(List<int> transIDs)
        {
            foreach (var transID in transIDs)
            {
                bool foundTrans = false;
                for (int n = 0; n < lootTransactions.Count; n++)
                {
                    var lt = lootTransactions[n];
                    if (lt.TransactionID == transID)
                    {
                        foundTrans = true;

                        //Dispatch transaction end packet
                        Message mEnd = PacketCreator.S2C_GAME_LOOTING_SV2CL_ENDTRANSACTION(transID);
                        foreach(var receiver in lt.Receivers)
                        {
                            receiver.SendToClient(mEnd);
                        }

                        lootTransactions.RemoveAt(n);
                        n--;

                    }
                    if (foundTrans) break;
                }
            }
        }

        public void RemoveAllGiven()
        {
            
            for(int n = 0; n < lootTransactions.Count; n++)
            {
                var lt = lootTransactions[n];
                for (int m = 0; m < lt.LootItems.Count; m++)
                {
                    var li = lt.LootItems[m];
                    if (li.Given)
                    {
                        //Notify receivers of item removal
                        Message mRemove = PacketCreator.S2C_GAME_LOOTING_SV2CL_REMOVEITEM(lt.TransactionID, li.LootItemID);
                        foreach (var receiver in lt.Receivers)
                        {
                            receiver.SendToClient(mRemove);
                        }
                        assignedLootIDs.Remove(li.LootItemID);
                        lt.LootItems.RemoveAt(m);
                        m--;
                        

                        if (lt.LootItems.Count == 0)
                        {
                            Message mEndTran = PacketCreator.S2C_GAME_LOOTING_SV2CL_ENDTRANSACTION(lt.TransactionID);
                            foreach(var receiver in lt.Receivers)
                            {
                                receiver.SendToClient(mEndTran);
                            }
                            lootTransactions.RemoveAt(n);
                            n--;                            
                            break;
                        }
                    }
                }
            }            
        }

        public static LootManager Get
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LootManager();
                    _instance.lootTransactions = new List<LootTransaction>();
                }
                return _instance;
            }
        }

        public void FixedUpdate()
        {
            if (lootTransactions.Count == 0) return;

            tickTimer += Time.deltaTime;
            if (tickTimer < tickLength) return;

            //Update loot items
            for(int n = 0; n < lootTransactions.Count; n++)
            {
                var lt = lootTransactions[n];
                if (lt.TimedTransaction) { lt.CurrentTimer -= tickLength; }
                if (lt.CurrentTimer <= 0.0f)
                {
                    //TODO : Remove loot items from clients
                    lootTransactions.Remove(lt);
                    n--;
                    continue;
                }
            }
            //TODO 
            //any Manager updates

            tickTimer = 0.0f;
        }

        public int uniqueTransID()
        {
            int id = -1;
            bool unique;

            do {                
                id++;
                unique = true;
                foreach (var tran in lootTransactions)
                {
                    if (tran.TransactionID == id)
                    {
                        unique = false;
                        break;
                    }
                }
            } while (!unique);

            return id;
        }

        public int uniqueLootID()
        {
            int id = -1;
            bool unique;

            do
            {                
                id++;
                unique = true;
                foreach (var assignedLID in assignedLootIDs)
                {
                    if (assignedLID == id)
                    {
                        unique = false;
                        break;
                    }
                }
            } while (!unique);

            assignedLootIDs.Add(id);
            return id;
        }

        public LootTransaction.DroppedLootItem generateDLI (DroppedItem di)
        {
            var output = new LootTransaction.DroppedLootItem();
            output.Item = di;
            output.LootItemID = uniqueLootID();
            return output;
        }


    }
}
