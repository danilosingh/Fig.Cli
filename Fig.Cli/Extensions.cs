using Fig.Cli.Helpers;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Linq;

namespace Fig.Cli
{
    public static class Extensions
    {
        public static T GetField<T>(this WorkItem workItem, string fieldName)
        {
            if (!workItem.Fields.ContainsKey(fieldName))
                return default;

            return TypeHelper.Convert<T>(workItem.Fields[fieldName]);
        }

        public static int GetParentId(this WorkItem workItem)
        {
            var parentRelation = workItem.Relations.FirstOrDefault(c => c.Rel == "System.LinkTypes.Hierarchy-Reverse");

            if (parentRelation == null)
                return 0;

            return Convert.ToInt32(parentRelation.Url.Substring(parentRelation.Url.LastIndexOf('/') + 1));
        }

        public static string[] Split(this string str, string separator, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries, int count = 0)
        {
            return count > 0 ?
                str.Split(new string[] { separator }, count, options) :
                str.Split(new string[] { separator }, options);
        }

        public static string Replace(this string str, string[] strings, string newString)
        {
            foreach (var item in strings)
            {
                str = str.Replace(item, newString);
            }

            return str;
        }
    }
}
