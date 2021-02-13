using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Fig.Cli.TeamFoundation.Helpers
{
    public static class AzureWorkItemHelpers
    {
        public static void ChangeState(WorkItemTrackingHttpClient client, int id, string state)
        {
            var patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = state
                }
            };

            client.UpdateWorkItemAsync(patchDocument, id).Wait();
        }

        public static void ChangeField(WorkItemTrackingHttpClient client, int id, string field, string state)
        {
            var patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/" + field,
                    Value = state
                }
            };

            client.UpdateWorkItemAsync(patchDocument, id).Wait();
        }
    }
}
