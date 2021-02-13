using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Fig.Cli
{
    public class AzureDevOpsClientContext
    {
        protected VssCredentials Credentials { get; private set; }
        protected Uri Url { get; private set; }
        protected Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        private VssConnection connection;

        public VssConnection Connection
        {
            get
            {
                if (connection == null)
                {
                    CreateConnection();
                }

                return connection;
            }
            private set
            {
                connection = value;
            }
        }

        private void CreateConnection()
        {
            VssHttpMessageHandler vssHandler = new VssHttpMessageHandler(
                                    Credentials,
                                    VssClientHttpRequestSettings.Default.Clone());

            connection = new VssConnection(
                Url,
                vssHandler,
                Array.Empty<DelegatingHandler>());
        }

        public AzureDevOpsClientContext(Uri url) : this(url, null)
        {
        }

        public AzureDevOpsClientContext(Uri url, VssCredentials credentials)
        {
            Url = url;

            if (credentials == null)
            {
                Credentials = new VssClientCredentials();
            }
            else
            {
                Credentials = credentials;
            }
        }

        public T GetValue<T>(string name)
        {
            return (T)Properties[name];
        }

        public bool TryGetValue<T>(string name, out T result)
        {
            return Properties.TryGetValue<T>(name, out result);
        }

        public void SetValue<T>(string name, T value)
        {
            Properties[name] = value;
        }

        public void RemoveValue(string name)
        {
            Properties.Remove(name);
        }

        public void Log(string message)
        {
            Log(message, null);
        }

        public void Log(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }
}
