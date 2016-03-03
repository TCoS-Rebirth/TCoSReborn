//==============================================================================
//  CT_Greeting
//==============================================================================

using Database.Static;

namespace Gameplay.Conversations
{
    public class CT_Greeting : ConversationTopic
    {
        public SBLocalizedString DefaultText;
    }
}

/*
  function string GetText() {
    if (Len(TopicText.Text) != 0) {                                             //0000 : 07 1C 00 9B 7D 36 18 57 4B 11 01 68 6F 7C 14 16 25 16 
      return Super.GetText();                                                   //0012 : 04 1C D0 C3 83 14 16 
    } else {                                                                    //0019 : 06 27 00 
      return DefaultText.Text;                                                  //001C : 04 36 18 57 4B 11 01 A0 ED 8C 16 
    }
    //07 1C 00 9B 7D 36 18 57 4B 11 01 68 6F 7C 14 16 25 16 04 1C D0 C3 83 14 16 06 27 00 04 36 18 57 
    //4B 11 01 A0 ED 8C 16 04 0B 47 
  }


*/