using Moonrise.Logging;
using Moonrise.Logging.LoggingProviders;
using Moonrise.Utils.Standard.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public static class Initialise
    {
        private class LoggingConfig
        {
            public Logger.ReportingLevel Level { get; } = Logger.ReportingLevel.Debug;
            public BasicFileLogProvider.Config LogFile { get; } = new BasicFileLogProvider.Config();
        }

        public static void Logging()
        {
            LoggingConfig loggingConfig = new LoggingConfig();
            Settings.Application.Read("Logging", ref loggingConfig, true);
            Logger.LogProvider = new BasicFileLogProvider(loggingConfig.LogFile);
            Logger.OutputLevel = loggingConfig.Level;
            Logger.Seperate('*');
        }

        public static void ConfigSettings()
        {
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider();


            //Settings.Application.SettingsProvider = new JsonConfigSettingsProvider(
            //    new JsonConfigSettingsProvider.Config
            //    {
            //        ApplicationSettingsFilename = "C:\\Source\\Dowty.PropellerInformationSystem\\WebApplication1\\bin\\Debug\\netcoreapp2.1\\appSettings.json"
            //    });
        }
    }
}
