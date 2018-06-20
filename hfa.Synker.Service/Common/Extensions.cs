using Microsoft.Extensions.Logging;
using Nest;
using PlaylistManager.Entities;

namespace System.Text
{
    public static class EncodingForBase64
    {
        public static string EncodeBase64(this Encoding encoding, string text)
        {
            if (text == null)
            {
                return null;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        public static string DecodeBase64(this Encoding encoding, string encodedText)
        {
            if (encodedText == null)
            {
                return null;
            }

            byte[] textAsBytes = Convert.FromBase64String(encodedText);
            return encoding.GetString(textAsBytes);
        }
    }
   
}