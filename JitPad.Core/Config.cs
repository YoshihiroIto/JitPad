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

        #region FilePath

        private string _FilePath = "";

        [JsonIgnore]
        public string FilePath
        {
            get => _FilePath;
            set => SetProperty(ref _FilePath, value);
        }

        #endregion

        public string LoadCodeTemplate()
        {
            try
            {
                if (File.Exists(_codeTemplateFilePath))
                    return File.ReadAllText(_codeTemplateFilePath);
            }
            catch
            {
                // ignored
            }

            return DefaultCodeTemplate;
        }

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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public class Class
{
    public void Method()
    {
    }
}";
        private string _codeTemplateFilePath = "";

        public static Config Load(string? configFilePath = null, string? codeTemplateFilePath = null)
        {
            if (configFilePath == null)
                configFilePath = DefaultFilePath;

            // config
            Config? config = null;
            try
            {
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

            config.FilePath = configFilePath;
            config._codeTemplateFilePath = codeTemplateFilePath ?? DefaultTemplateFilePath;

            // setup code template
            try
            {
                if (File.Exists(config._codeTemplateFilePath) == false)
                    File.WriteAllText(DefaultTemplateFilePath, DefaultCodeTemplate);
            }
            catch
            {
                // ignored
            }

            return config;
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);

            File.WriteAllText(FilePath ?? throw new NullReferenceException(), json);
        }
    }
}