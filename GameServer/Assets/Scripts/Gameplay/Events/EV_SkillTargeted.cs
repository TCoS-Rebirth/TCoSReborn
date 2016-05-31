using System;
using Gameplay.Entities;
using Gameplay.Skills;

namespace Gameplay.Events
{
    public class EV_SkillTargeted : Content_Event
    {
        public FSkill Skill;
        public int skillID;
        public string temporarySkillName;

        public override bool CanExecute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            throw new NotImplementedException();
        }

        /*function sv_Execute(Game_Pawn aObject,Game_Pawn aSubject) {
        local Game_Pawn executor;
        local Game_Pawn Target;
        local bool sheathe;
        if (aObject != None) {
        executor = aObject;
        } else {
        executor = aSubject;
        }
        if (aSubject != None) {
        Target = aSubject;
        } else {
        Target = aObject;
        }
        if (!executor.combatState.CheckWeaponType(Skill.RequiredWeapon)) {
        sheathe = executor.combatState.CombatReady();
        executor.combatState.sv_SwitchToWeaponType(Skill.RequiredWeapon);
        }
        if (Target != None) {
        executor.Skills.ExecuteL(Skill,Target.Location,executor.Level.TimeSeconds);
        } else {
        executor.Skills.ExecuteL(Skill,executor.Location,executor.Level.TimeSeconds);
        }
        */
    }
}