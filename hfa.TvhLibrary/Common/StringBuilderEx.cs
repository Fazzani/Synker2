namespace System.Text
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Web;
    public static class StringBuilderEx
    {
        //in an extension class
        public static void AppendUrlEncoded(this StringBuilder sb, string name, string value)
        {
            if (sb.Length != 0)
                sb.Append("&");
            sb.Append(HttpUtility.UrlEncode(name));
            sb.Append("=");
            sb.Append(HttpUtility.UrlEncode(value));
        }

        public static string ToDataPostEncodedUrl(this string me)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(me);

            var sb = new StringBuilder();
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in dict)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value.ToString()))
                {
                    var sbChild = new StringBuilder();
                    foreach (KeyValuePair<string, string> kvpChild in kvp.Value)
                    {
                        if (!string.IsNullOrEmpty(kvpChild.Key) && !string.IsNullOrEmpty(kvpChild.Value))
                            sbChild.AppendUrlEncoded(kvpChild.Key, kvpChild.Value.ToString());
                    }

                    sb.AppendFormat("{0}={1}", HttpUtility.UrlEncode(kvp.Key), sbChild);
                }

            }

            return sb.ToString();
        }

        public static string ToDataPostEncodedUrl2(this string me)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(me);

            var sb = new StringBuilder();
            foreach (KeyValuePair<string, object> kvp in dict)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value.ToString()))
                    sb.AppendUrlEncoded(kvp.Key, kvp.Value.ToString());

            }

            return sb.ToString();
        }
    }
}
