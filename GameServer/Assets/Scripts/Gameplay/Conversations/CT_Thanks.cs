//==============================================================================
//  CT_Thanks
//==============================================================================

namespace Gameplay.Conversations
{
    public class CT_Thanks : CT_Target
    {
    }
}

/*
  event bool sv_OnFinish(Game_Pawn aSpeaker,Game_Pawn aPartner) {
    local export editinline Quest_Type quest;
    local export editinline QT_Fedex Target;
    Target = QT_Fedex(Outer);                                                   //0000 : 0F 00 B0 54 8B 16 2E 50 EE AA 01 01 38 75 4B 11 
    quest = Quest_Type(Target.Outer);                                           //0010 : 0F 00 08 E1 8B 16 2E 88 92 78 01 19 00 B0 54 8B 16 05 00 04 01 38 75 4B 11 
    UCASSERT(Target != None,"Quest thanks conversation is not part of a target");//0029 : 1C 48 12 49 11 77 00 B0 54 8B 16 2A 16 1F 51 75 65 73 74 20 74 68 61 6E 6B 73 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 61 20 74 61 72 67 65 74 00 16 
    UCASSERT(quest != None,"Quest fedex conversation is not part of a quest");  //006A : 1C 48 12 49 11 77 00 08 E1 8B 16 2A 16 1F 51 75 65 73 74 20 66 65 64 65 78 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 61 20 71 75 65 73 74 00 16 
    if (Game_PlayerPawn(aPartner).questLog.sv_SetTargetAsCompleted(Target,aSpeaker)) {//00A9 : 07 E7 00 19 19 2E 68 3C 77 01 00 50 DF 8B 16 05 00 04 01 80 6B C7 0F 10 00 04 1C 68 AD 7F 14 00 B0 54 8B 16 00 D0 DE 8B 16 16 
      return Super.sv_OnFinish(aSpeaker,aPartner);                              //00D3 : 04 1C F0 10 CA 0F 00 D0 DE 8B 16 00 50 DF 8B 16 16 
    } else {                                                                    //00E4 : 06 E9 00 
      return False;                                                             //00E7 : 04 28 
    }
    //0F 00 B0 54 8B 16 2E 50 EE AA 01 01 38 75 4B 11 0F 00 08 E1 8B 16 2E 88 92 78 01 19 00 B0 54 8B 
    //16 05 00 04 01 38 75 4B 11 1C 48 12 49 11 77 00 B0 54 8B 16 2A 16 1F 51 75 65 73 74 20 74 68 61 
    //6E 6B 73 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 
    //61 20 74 61 72 67 65 74 00 16 1C 48 12 49 11 77 00 08 E1 8B 16 2A 16 1F 51 75 65 73 74 20 66 65 
    //64 65 78 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 
    //61 20 71 75 65 73 74 00 16 07 E7 00 19 19 2E 68 3C 77 01 00 50 DF 8B 16 05 00 04 01 80 6B C7 0F 
    //10 00 04 1C 68 AD 7F 14 00 B0 54 8B 16 00 D0 DE 8B 16 16 04 1C F0 10 CA 0F 00 D0 DE 8B 16 00 50 
    //DF 8B 16 16 06 E9 00 04 28 04 0B 47 
  }

*/