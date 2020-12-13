﻿using System;
using System.Diagnostics;
using Moonrise.Logging;

namespace Moonrise
{
    class Program
    {
        static void Main(string[] args)
        {
            Samples.Run();
            Logger.Flush();
            Console.ReadKey();
        }
    }
}

//// This is a standard .Net JSON settings file. You COULD process it using the standard .Net core way of 
//// reading it - can't remember what that is as it's ages since I used that. I do remember that it relies
//// on injecting a settings handler. The problem I have with that is that you have to keep injecting the appropriate
//// reference and accepting it etc. Same thing with logging, in fact for logging it's much worse. Why?
//// Well settings you generally only read during some startup phase, whereas logging you want to be able to use EVERYWHERE
//// and I for one do NOT want to keep injecting logger references to every single class that I might want to log from!
//// I explain more in the Samples code. The other advantage of the Moonrise Settings is "hydrating" complete object
//// trees. Again see the example for more details!
//{
//    // If you want to use BOTH Moonrise logging and .Net Core logging - which you can quite easily there are Moonrise providers
//    // to allow Moonrise logging to ultimately go through to the .Net Core logging and to provide a .Net Core logging provider
//    // that will feed into what Moonrise logging providers you're using - then you might want to rename this part of the
//    // settings from Logging to perhaps MoonriseLogging or whatever. You'd just change the corresponding Settings.Read(...)
//    "Logging": {
//        // Everything inside this bit of settings tree now matches the structure of the Moonrise.Samples.Initialise.LoggingConfig class
//        "Logger": {
//            // Everything inside this bit of settings tree now matches the structure of the Moonrise.Logging.Logger.Config class
//            // i.e. showing a sample of reading complete object trees.
//            "LogMethodName": false,
//      // Generally you want to put all of the LogTags that you use in here.
//      // Then to turn them Off or On, simply put or remove an x from in front of them. The LogTags are simply matched 
//      // against a string so if the active ones have an x in front, they won't match, remove the preceeding x and 
//      // they will match!
//      "LogTags": [
//        "NoTags",
//        "AreCurrently",
//        "Defined",
//        "SampleLogging"
//          ],
//      // For the enum if you let them get written out if not existing, they'll be written as their numeric value
//      // You can use the numeric, the enum "name" or the enum "Description" for enum settings being read in.
//      "OutputLevel": "Debug",
//      "StackTracingEnabled": false,
//      "UseConsoleOutput": false,
//      "UseContext": true,
//      "UseThreadId": false

//      // Another thing to note about reading settings as a complete object.
//      // It uses NewtonSoft.Json.JsonConvert to read the JSON into an instantiated object which means it reads the public
//      // settable properties.
//      // You do not need to have all of the properties defined in the JSON and they do not need to be in any particular order
//      // but any properties that are not in the JSON will be set as per the default settings for those properties on creation
//      // i.e. Any values in the structure that are set BEFORE reading the settings will get effectively replaced by a new
//      // instance with EITHER the defaults OR the value in the JSON!
//        },
//    "LogFile": {
//            // Note that this settings subtree now matches the structure of the Moonrise.Logging.BasicFileLogProvider.Config class,
//            "DateTimeFormatterPrefix": "{0:HH:mm:ss} ",
//      "LogCycling": "Weekly",
//      "LogFilePerThread": false,
//      // This will place the logging file back up from the build output into the project directory
//      "LoggingFile": "../../../Logging.log",
//      // The number of log entries that can be written to a log file before cycling to another file that day
//      "MaxEntries": 0,
//      // This determines the naming format if there are multiple files created in a single day as determined from the MaxEntries being exceeded
//      "ByCountFilenameDateTimeFormat": "yyyyMMddhhmmss",
//      // The number of log entries to buffer up before flushing
//      "BufferCount": 1000,
//      // The number of log message bytes to buffer up before flushing - none get lost if it exceeds that value
//      "BufferSize": 102400,
//      // The number of seconds since the last log was written before flushing. NOTE THIS IS ONLY EFFECTIVE WHEN THE NEXT LOG MSG IS PRODUCED
//      "BufferDelay": 30,
//      // The logging level on which to flush
//      "FlushOn": "Error"
//    }
//    },
//  "EncryptedSetting": "HAHA i got ouT!",
//  "NonEncryptedSetting": "HAHA FREEDOM!"
//}
//16522d99c16d8e3c5f7ef1041faf573c