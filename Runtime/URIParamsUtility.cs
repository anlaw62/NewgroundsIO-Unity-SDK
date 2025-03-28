using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Newgrounds
{
    public static class URIParamsUtility
    {
        private static readonly Regex paramRegEx= new Regex(@"[?&](?<key>[^&=]+)=(?<value>[^&]+)", RegexOptions.Compiled);
        public static Dictionary<string,string> GetParams(Uri uri)
        {
           
            Dictionary<string, string> uriParams = new();
            string query = uri.Query;


          
           
            foreach (Match match in paramRegEx.Matches(query))
            {

                string key = match.Groups["key"].Value;

                string value = match.Groups["value"].Value;
       
                uriParams[key] = value;
            }
            return uriParams;
        }
    }
}