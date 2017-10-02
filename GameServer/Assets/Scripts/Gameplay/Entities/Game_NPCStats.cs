using UnityEngine;

namespace Gameplay.Entities
{
    public class Game_NPCStats : Game_CharacterStats
    {

        NpcCharacter Owner;

        public override void Init(Character character)
        {
            base.Init(character);
            Owner = character as NpcCharacter;
            //TODO fix stat initialization
            var hp = 100;
            var levelHP = 100;   
            Owner.NPCPePRank = Owner.Type.PePRank;
            Owner.Type.InitializeStats(Owner.NPCFameLevel, Owner.NPCPePRank, out hp, out levelHP, out mRecord.Body, out mRecord.Mind, out mRecord.Focus);
            //NPC_Type with level 0 indicates random level generation by spawner
            Owner.NPCFameLevel = Owner.Type.FameLevel != 0 ? Owner.Type.FameLevel : Random.Range(Owner.RespawnInfo.levelMin, Owner.RespawnInfo.levelMax);
            mCharacterClass = Owner.Type.NPCClassClassification;
            mRecord.PePRank = Owner.Type.PePRank;
            mRecord.MaxHealth = Mathf.Max(hp, levelHP, GetFameLevel() * 10, 100); //TODO fix
            mHealth = mRecord.MaxHealth;
            mRecord.CopyHealth = mHealth;
        }

        public override int GetFameLevel()
        {
            return Owner.NPCFameLevel;
        }

        public override int GetPePRank()
        {
            return Owner.NPCPePRank;
        }
    }
}