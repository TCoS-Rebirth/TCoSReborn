namespace Gameplay.Events
{
    public class EV_SetHealth : Content_Event
    {
        public enum EHealthMode
        {
            HM_ABSOLUTE,
            HM_RELATIVE,
            HM_ABSOLUTE_PERCENTAGE,
            HM_RELATIVE_PERCENTAGE
        }

        public EHealthMode HealthMode;
        public float HealthValue;
        /*
        function sv_Execute(Game_Pawn aObject,Game_Pawn aSubject) {
        switch (HealthMode) {                                                       //0000 : 05 01 01 98 09 00 14
        case 0 :                                                                  //0007 : 0A 23 00 24 00
        aSubject.SetHealth(HealthValue);                                        //000C : 19 00 10 0A 00 14 0B 00 00 1B AC 0F 00 00 01 88 0A 00 14 16
        break;                                                                  //0020 : 06 AB 00
        case 1 :                                                                  //0023 : 0A 3F 00 24 01
        aSubject.IncreaseHealth(HealthValue);                                   //0028 : 19 00 10 0A 00 14 0B 00 00 1B 31 10 00 00 01 88 0A 00 14 16
        break;                                                                  //003C : 06 AB 00
        case 2 :                                                                  //003F : 0A 7B 00 24 02
        aSubject.SetHealth(HealthValue * aSubject.CharacterStats.mRecord.MaxHealth);//0044 : 19 00 10 0A 00 14 2B 00 00 1B AC 0F 00 00 AB 01 88 0A 00 14 39 3F 36 D8 91 18 11 19 19 00 10 0A 00 14 05 00 04 01 08 43 34 0F 05 00 68 01 78 2D 34 0F 16 16
        break;                                                                  //0078 : 06 AB 00
        case 3 :                                                                  //007B : 0A A8 00 24 03
        aSubject.SetHealth(HealthValue * aSubject.GetHealth());                 //0080 : 19 00 10 0A 00 14 1C 00 00 1B AC 0F 00 00 AB 01 88 0A 00 14 19 00 10 0A 00 14 06 00 04 1B 12 10 00 00 16 16 16
        break;                                                                  //00A5 : 06 AB 00
        default:                                                                  //00A8 : 0A FF FF
        }
        }
        */
    }
}