using Microsoft.TeamFoundation.Core.WebApi;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fig.Cli
{
    public static class AzureProjectHelper
    {
        public static TeamProjectReference FindDefaultProject(AzureDevOpsClientContext context)
        {
            if (!FindDefaultProject(context, out TeamProjectReference project))
            {
                throw new Exception("No sample projects available. Create a project in this collection and run the sample again.");
            }

            return project;
        }

        public static Guid FindDefaultProjectId(AzureDevOpsClientContext context)
        {
            return FindDefaultProject(context).Id;
        }

        public static bool FindDefaultProject(AzureDevOpsClientContext context, out TeamProjectReference project)
        {
            // Check if we already have a default project loaded
            if (!context.TryGetValue("$defautProject", out project))
            {
                var connection = context.Connection;
                var projectClient = connection.GetClient<ProjectHttpClient>();

                // Check if an ID was already set (this could have been provided by the caller)

                if (!context.TryGetValue("projectId", out Guid projectId))
                {
                    context.TryGetValue("projectName", out string projectName);
                    var projects = projectClient.GetProjects(null, top: 10).Result;
                    project = projects.OrderBy(c => c.Name == projectName ? 0 : 1).FirstOrDefault();
                }
                else
                {
                    // Get the details for this project
                    project = projectClient.GetProject(projectId.ToString()).Result;
                }
            }

            if (project != null)
            {
                context.SetValue("$defautProject", project);
            }
            else
            {
                // create a project here?
                throw new Exception("No projects available for running the sample.");
            }

            return project != null;
        }

        public static WebApiTeamRef FindAnyTeam(AzureDevOpsClientContext context, Guid? projectId)
        {
            if (!FindAnyTeam(context, projectId, out WebApiTeamRef team))
            {
                throw new Exception("No sample teams available. Create a project/team in this collection and run the sample again.");
            }

            return team;
        }

        public static bool FindAnyTeam(AzureDevOpsClientContext context, Guid? projectId, out WebApiTeamRef team)
        {
            if (!projectId.HasValue)
            {
                if (FindDefaultProject(context, out TeamProjectReference project))
                {
                    projectId = project.Id;
                }
            }

            // Check if we already have a team that has been cached for this project
            if (!context.TryGetValue<WebApiTeamRef>("$" + projectId + "Team", out team))
            {
                TeamHttpClient teamClient = context.Connection.GetClient<TeamHttpClient>();
                team = teamClient.GetTeamsAsync(projectId.ToString(), top: 1).Result.FirstOrDefault();
                if (team != null)
                {
                    context.SetValue<WebApiTeamRef>("$" + projectId + "Team", team);
                }
                else
                {
                    // create a team?
                    throw new Exception("No team available for running this sample.");
                }
            }

            return team != null;
        }

        public static Guid GetCurrentUserId(AzureDevOpsClientContext context)
        {
            return context.Connection.AuthorizedIdentity.Id;
        }

        public static string GetCurrentUserDisplayName(AzureDevOpsClientContext context)
        {
            return context.Connection.AuthorizedIdentity.ProviderDisplayName;
        }

        public static string GetSampleTextFile()
        {
            return GetSampleFilePath("ConsoleApp7.WorkItemTracking.SampleFile.txt");
        }

        public static string GetSampleBinaryFile()
        {
            return GetSampleFilePath("ConsoleApp7.WorkItemTracking.SampleFile.png");
        }

        private static string GetSampleFilePath(string fullResourceName)
        {
            Stream inputStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
            FileInfo tempOutputFile = new FileInfo(Path.Combine(Path.GetTempPath(), fullResourceName));
            FileStream tempFileOutputStream = tempOutputFile.OpenWrite();
            inputStream.CopyTo(tempFileOutputStream);

            tempFileOutputStream.Close();
            inputStream.Close();

            return tempOutputFile.FullName;
        }
    }
}

