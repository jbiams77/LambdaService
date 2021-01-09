using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService.MessageSchema
{
    class SessionUpdate
    {
        public readonly string Topic = "DisplayStateResponse";
        public string CurrentWord { get; set; }
        public int CurrentSchedule { get; set; }
        public int WordsRemaining { get; set; }
    }
}
