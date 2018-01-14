using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class Extension
    {
        /// <summary>
        /// Unix TimeStamp to DateTime
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(this string text)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            if (string.IsNullOrEmpty(text))
                return dtDateTime;
            dtDateTime = dtDateTime.AddSeconds(Convert.ToInt32(text));
            return dtDateTime;
        }
    }
}
