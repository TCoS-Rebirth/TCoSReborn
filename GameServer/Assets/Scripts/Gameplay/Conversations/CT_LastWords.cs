//==============================================================================
//  CT_LastWords
//==============================================================================

namespace Gameplay.Conversations
{
    public class CT_LastWords : CT_Target
    {
    }
}

/*
  event bool sv_OnFinish(Game_Pawn aSpeaker,Game_Pawn aPartner) {
    local export editinline Quest_Type quest;
    local export editinline QT_Defeat Target;
    Target = QT_Defeat(Outer);                                                  //0000 : 0F 00 98 57 8B 16 2E D0 E3 AA 01 01 38 75 4B 11 
    quest = Quest_Type(Target.Outer);                                           //0010 : 0F 00 50 DE 8B 16 2E 88 92 78 01 19 00 98 57 8B 16 05 00 04 01 38 75 4B 11 
    UCASSERT(Target != None,"Quest defeat conversation is not part of a defeat target");//0029 : 1C 48 12 49 11 77 00 98 57 8B 16 2A 16 1F 51 75 65 73 74 20 64 65 66 65 61 74 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 61 20 64 65 66 65 61 74 20 74 61 72 67 65 74 00 16 
    UCASSERT(quest != None,"Quest defeat conversation is not part of a quest"); //0071 : 1C 48 12 49 11 77 00 50 DE 8B 16 2A 16 1F 51 75 65 73 74 20 64 65 66 65 61 74 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 61 20 71 75 65 73 74 00 16 
    if (Game_PlayerPawn(aPartner).questLog.sv_SetTargetAsCompleted(Target,aSpeaker)) {//00B1 : 07 EF 00 19 19 2E 68 3C 77 01 00 D0 DD 8B 16 05 00 04 01 80 6B C7 0F 10 00 04 1C 68 AD 7F 14 00 98 57 8B 16 00 50 DD 8B 16 16 
      return Super.sv_OnFinish(aSpeaker,aPartner);                              //00DB : 04 1C F0 10 CA 0F 00 50 DD 8B 16 00 D0 DD 8B 16 16 
    } else {                                                                    //00EC : 06 F1 00 
      return False;                                                             //00EF : 04 28 
    }
    //0F 00 98 57 8B 16 2E D0 E3 AA 01 01 38 75 4B 11 0F 00 50 DE 8B 16 2E 88 92 78 01 19 00 98 57 8B 
    //16 05 00 04 01 38 75 4B 11 1C 48 12 49 11 77 00 98 57 8B 16 2A 16 1F 51 75 65 73 74 20 64 65 66 
    //65 61 74 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 
    //61 20 64 65 66 65 61 74 20 74 61 72 67 65 74 00 16 1C 48 12 49 11 77 00 50 DE 8B 16 2A 16 1F 51 
    //75 65 73 74 20 64 65 66 65 61 74 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 
    //70 61 72 74 20 6F 66 20 61 20 71 75 65 73 74 00 16 07 EF 00 19 19 2E 68 3C 77 01 00 D0 DD 8B 16 
    //05 00 04 01 80 6B C7 0F 10 00 04 1C 68 AD 7F 14 00 98 57 8B 16 00 50 DD 8B 16 16 04 1C F0 10 CA 
    //0F 00 50 DD 8B 16 00 D0 DD 8B 16 16 06 F1 00 04 28 04 0B 47 
  }


*/