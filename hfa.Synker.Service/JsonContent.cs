using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace hfa.Synker.Services
{
    public class JsonContent : StringContent
    {
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        { }
    }
}
