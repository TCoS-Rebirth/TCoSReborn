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
            ClassType = Owner.Type.NPCClassClassification;
            //NPC_Type with level 0 indicates random level generation by spawner
            if (Owner.Type.FameLevel != 0)
            {
                FameLevel = Owner.Type.FameLevel;
            }
            else
            {
                FameLevel = Random.Range(Owner.RespawnInfo.levelMin, Owner.RespawnInfo.levelMax);
            }
            PepRank = Owner.Type.PePRank;
            var hp = 100;
            var levelHP = 100;
            Owner.Type.InitializeStats(FameLevel, PepRank, out hp, out levelHP, out Body, out Mind, out Focus);
            MaxHealth = Mathf.Max(hp, levelHP, FameLevel * 10, 100); //TODO fix
            Health = MaxHealth;
        }
    }
}