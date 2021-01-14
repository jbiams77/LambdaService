using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService.DisplayTemplates
{
    class CurrentWordTemplate : IBodyTemplate
    {
        public string Type => "CurrentWordTemplate";

        public string Token { get; set; }

        //[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        //public string Title { get; set; }

        [JsonProperty("textContent")]
        public TemplateContent Content { get; set; }
    }
}
