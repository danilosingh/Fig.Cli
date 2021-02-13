using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Fig.Cli
{
    public class FigContext
    {
        private string rootDirectory;
        private string figDirectory;

        public string FigDirectory
        {
            get { return figDirectory; }
        }
        public string RootDirectory
        {
            get { return rootDirectory; }
        }
        public string ConfFilePath
        {
            get { return Path.Combine(FigDirectory, ".conf"); }
        }

        public FigOptions Options { get; private set; }

        public FigContext()
        {
            LoadRootDirectory();
            LoadOptions();
        }

        private void LoadRootDirectory()
        {
            rootDirectory = GitHelper.FindRootDirectory();

            if (string.IsNullOrEmpty(rootDirectory))
            {
                throw new InvalidOperationException("not a git repository.");
            }

            figDirectory = Path.Combine(rootDirectory, ".fig");
        }

        public void LoadOptions()
        {
            Options = new FigOptions();

            if (!File.Exists(ConfFilePath))
            {
                return;
            }

            Options = JsonConvert.DeserializeObject<FigOptions>(File.ReadAllText(ConfFilePath));
        }

        public bool IsInitialized()
        {
            return File.Exists(ConfFilePath);
        }

        public void SetOptions(FigOptions options)
        {
            Options = options;
        }

        public void SaveOptions()
        {
            if (!Directory.Exists(FigDirectory))
            {
                var directoryInfo = Directory.CreateDirectory(FigDirectory);
                directoryInfo.Attributes |= FileAttributes.Hidden;
            }

            File.WriteAllText(ConfFilePath, JsonConvert.SerializeObject(Options), Encoding.UTF8);
        }

        public static FigContext Instance = new FigContext();
    }
}