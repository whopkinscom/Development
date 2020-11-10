using System;
using Moonrise.Logging;
using Moonrise.Logging.LoggingProviders;
using Moonrise.Utils.Standard.Config;

namespace ConsoleApp1
{
    class Program
    {
        private class OtherConfig
        {
            public int LintyInty { get; set; } = 68;
            public string CheesyString { get; set; } = "Cheddary";
        }

        private class LoggingConfig
        {
            public BasicFileLogProvider.Config File { get; set; } = new BasicFileLogProvider.Config();
            public Logger.ReportingLevel LogLevel { get; set; } = Logger.ReportingLevel.Information;
        }

        private class Config
        {
            public LoggingConfig Logging { get; set; } = new LoggingConfig();
            public int MintyInty { get; set; } = 18;

            public OtherConfig MoreConfig { get; set; } = new OtherConfig();
        }

        private static Config Configuration = new Config();
        static void Main(string[] args)
        {
            Initialise();
            Console.WriteLine("Hello World!");
            Logger.Debug("Hello World!");
            Logger.Debug(Console.ReadKey());
        }

        private static void Initialise()
        {
            InitialiseConfig();
            InitialiseLogging();
        }

        private static void InitialiseConfig()
        {
            Settings.Application.Read("Configuration", ref Configuration, true);
        }

        private static void InitialiseLogging()
        {
            Logger.LogProvider = new BasicFileLogProvider(Configuration.Logging.File);
            Logger.OutputLevel = Configuration.Logging.LogLevel;
            Logger.Seperate();
            Logger.Info(Configuration);
        }
    }
}