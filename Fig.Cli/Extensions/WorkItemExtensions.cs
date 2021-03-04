using Fig.Cli.Helpers;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Linq;

namespace Fig.Cli.Extensions
{
    public static class WorkItemExtensions
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
    }
}
