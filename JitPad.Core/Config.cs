using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JitPad.Foundation;

namespace JitPad.Core
{
    public class Config : NotificationObject
    {
        #region IsReleaseBuild

        private bool _IsReleaseBuild = true;

        public bool IsReleaseBuild
        {
            get => _IsReleaseBuild;
            set => SetProperty(ref _IsReleaseBuild, value);
        }

        #endregion

        #region IsTieredJit

        private bool _IsTieredJit;

        public bool IsTieredJit
        {
            get => _IsTieredJit;
            set => SetProperty(ref _IsTieredJit, value);
        }

        #endregion

        #region IsFileMonitoring

        private bool _IsFileMonitoring;

        public bool IsFileMonitoring
        {
            get => _IsFileMonitoring;
            set => SetProperty(ref _IsFileMonitoring, value);
        }

        #endregion

        #region MonitoringFilePath

        private string _MonitoringFilePath = "";

        public string MonitoringFilePath
        {
            get => _MonitoringFilePath;
            set => SetProperty(ref _MonitoringFilePath, value);
        }

        #endregion

        #region CodeTemplate

        private string _CodeTemplate = DefaultCodeTemplate;

        [JsonIgnore]
        public string CodeTemplate
        {
            get => _CodeTemplate;
            set => SetProperty(ref _CodeTemplate, value);
        }

        #endregion

        public static readonly string DefaultFilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Jewelry Development/JitPad/config.json");

        public static readonly string DefaultTemplateFilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Jewelry Development/JitPad/template.cs");

        private const string DefaultCodeTemplate =
            @"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Class
{
    public void Method()
    {
    }
}";

        public static Config Load(string? configFilePath = null, string? codeTemplateFilePath = null)
        {
            Config config = null;

            // config
            try
            {
                if (configFilePath == null)
                    configFilePath = DefaultFilePath;

                if (File.Exists(configFilePath))
                {
                    var json = File.ReadAllText(configFilePath);
                    config = JsonSerializer.Deserialize<Config>(json);
                }
            }
            catch
            {
                // ignored
            }

            if (config == null)
                config = new Config();

            // code template
            try
            {
                if (codeTemplateFilePath == null)
                    codeTemplateFilePath = DefaultTemplateFilePath;

                if (File.Exists(codeTemplateFilePath))
                    config.CodeTemplate = File.ReadAllText(DefaultTemplateFilePath);
                else
                    File.WriteAllText(DefaultTemplateFilePath, DefaultCodeTemplate);
            }
            catch
            {
                // ignored
            }

            return config;
        }

        public void Save(string? filePath = null)
        {
            var json = JsonSerializer.Serialize(this);

            if (filePath == null)
                filePath = DefaultFilePath;

            File.WriteAllText(filePath, json);
        }
    }
}