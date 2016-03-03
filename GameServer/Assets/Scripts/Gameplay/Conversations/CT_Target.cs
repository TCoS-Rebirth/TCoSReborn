//==============================================================================
//  CT_Target
//==============================================================================

namespace Gameplay.Conversations
{
    public class CT_Target : ConversationTopic
    {
    }
}

/*
  function Content_Type GetContext() {
    return Quest_Type(Outer.Outer);                                             //0000 : 04 2E 88 92 78 01 19 01 38 75 4B 11 05 00 04 01 38 75 4B 11 
    //04 2E 88 92 78 01 19 01 38 75 4B 11 05 00 04 01 38 75 4B 11 04 0B 47 
  }


  function string GetText() {
    local export editinline Quest_Type quest;
    quest = Quest_Type(GetContext());                                           //0000 : 0F 00 10 A5 8B 16 2E 88 92 78 01 1B 9F 10 00 00 16 
    UCASSERT(quest != None,"Quest target conversation context is not a quest"); //0011 : 1C 48 12 49 11 77 00 10 A5 8B 16 2A 16 1F 51 75 65 73 74 20 74 61 72 67 65 74 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 63 6F 6E 74 65 78 74 20 69 73 20 6E 6F 74 20 61 20 71 75 65 73 74 00 16 
    if (Len(TopicText.Text) != 0) {                                             //0051 : 07 6D 00 9B 7D 36 18 57 4B 11 01 68 6F 7C 14 16 25 16 
      return Super.GetText();                                                   //0063 : 04 1C D0 C3 83 14 16 
    } else {                                                                    //006A : 06 7D 00 
      return quest.GetName();                                                   //006D : 04 19 00 10 A5 8B 16 06 00 00 1B F9 03 00 00 16 
    }
    //0F 00 10 A5 8B 16 2E 88 92 78 01 1B 9F 10 00 00 16 1C 48 12 49 11 77 00 10 A5 8B 16 2A 16 1F 51 
    //75 65 73 74 20 74 61 72 67 65 74 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 63 6F 6E 74 65 78 74 
    //20 69 73 20 6E 6F 74 20 61 20 71 75 65 73 74 00 16 07 6D 00 9B 7D 36 18 57 4B 11 01 68 6F 7C 14 
    //16 25 16 04 1C D0 C3 83 14 16 06 7D 00 04 19 00 10 A5 8B 16 06 00 00 1B F9 03 00 00 16 04 0B 47 
    //
  }

*/