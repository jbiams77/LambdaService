using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService.MessageSchema
{
    enum DisplayState
    {
        intro,
        login,
        outro,
        flashcard
    }

    class DisplayStateRequest
    {
        public readonly string Topic = "DisplayStateRequest";
    }

    class DisplayStateResponse
    {
        public readonly string Topic = "DisplayStateResponse";
        public DisplayState State { get; set; }
    }
}
