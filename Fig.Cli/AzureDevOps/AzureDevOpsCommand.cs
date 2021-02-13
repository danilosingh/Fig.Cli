using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Net;

namespace Fig.Cli
{
    public abstract class AzureDevOpsCommand<T> : Command<T>
    {
        public AzureDevOpsClientContext AzureContext { get; private set; }

        protected AzureDevOpsCommand(T opts, FigContext context) : base(opts, context)
        {
            EnsureConfiguration();
            InitializeAzureContext();
        }

        protected void InitializeAzureContext()
        {
            AzureContext = CreateContext();
            AddProperty("projectName", Context.Options.ProjectName);
            AddProperty("repository" + Context.Options.RepositoryName, Context.Options.RepositoryId);
        }

        protected virtual VssCredentials CreateCredentials()
        {
            if (!string.IsNullOrEmpty(Context.Options.Pat))
            {
                return new VssBasicCredential(string.Empty, Context.Options.Pat);
            }

            return new VssCredentials(new WindowsCredential(new NetworkCredential(Context.Options.UserName, Context.Options.Password)));
        }

        protected virtual AzureDevOpsClientContext CreateContext()
        {
            var uri = !string.IsNullOrEmpty(Context.Options.ProjectUrl) ? Context.Options.ProjectUrl : "http://localhost:8080/tfs";
            return new AzureDevOpsClientContext(new Uri(uri), CreateCredentials());
        }

        protected bool IsInitialized()
        {
            return Context.IsInitialized();
        }

        private void AddProperty<TValue>(string name, TValue value)
        {
            if (!EqualityComparer<TValue>.Default.Equals(value, default))
            {
                AzureContext.SetValue(name, value);
            }
        }

        protected virtual void EnsureConfiguration()
        {
            if (!IsInitialized())
            {
                throw new ArgumentException("Fig.Cli not configured. Use the option 'config'.");
            }
        }

        protected string ConvertUserName()
        {
            if (!Context.Options.UserName.Contains("@"))
            {
                return Context.Options.UserName;
            }

            var i = Context.Options.UserName.LastIndexOf("@");

            return
                Context.Options.UserName.Substring(i + 1) + @"\" +
                Context.Options.UserName.Substring(0, i);
        }
    }
}
