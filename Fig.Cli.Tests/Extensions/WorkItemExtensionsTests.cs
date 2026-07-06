using System.Collections.Generic;
using Fig.Cli.Extensions;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Xunit;

namespace Fig.Cli.Tests.Extensions
{
    public class WorkItemExtensionsTests
    {
        [Fact]
        public void GetParentId_WhenRelationsIsNull_ReturnsZero()
        {
            // Work item sem nenhum link -> o SDK do Azure DevOps deixa Relations == null.
            // Regressão do crash do `fig done` (NRE em WorkItemsFromDifferentParents).
            var workItem = new WorkItem { Relations = null };

            Assert.Equal(0, workItem.GetParentId());
        }

        [Fact]
        public void GetParentId_WhenNoParentRelation_ReturnsZero()
        {
            var workItem = new WorkItem
            {
                Relations = new List<WorkItemRelation>
                {
                    new WorkItemRelation { Rel = "System.LinkTypes.Related", Url = "https://x/_apis/wit/workItems/999" }
                }
            };

            Assert.Equal(0, workItem.GetParentId());
        }

        [Fact]
        public void GetParentId_WhenParentRelationPresent_ReturnsParentId()
        {
            var workItem = new WorkItem
            {
                Relations = new List<WorkItemRelation>
                {
                    new WorkItemRelation { Rel = "System.LinkTypes.Hierarchy-Reverse", Url = "https://x/_apis/wit/workItems/4812" }
                }
            };

            Assert.Equal(4812, workItem.GetParentId());
        }
    }
}
