//
//
//
//  RRRRR   EEEEE      A       DDDD          TTTTT  H    H  IIIII   SSSS    !
//  R   R   E         A A      D   D           T    H    H    I    S        !
//  R RR    EEEEE    AAAAA     D   D           T    HHHHHH    I     SSS     !
//  R  R    E       A     A    D   D           T    H    H    I        S
//  R   R   EEEEE  A       A   DDDD            T    H    H  IIIII  sSSS     !
//
//
//
//
// (Yes I did do that by hand and what a pain in the arse it was!)
//
// NOTE: Moonrise.Samples is code to show examples of using the Moonrise nuget packages. You used to be able to bundle content files into nuget packages
// and have them added to the consuming project. Those days are over for so-called "SDK Projects", i.e. .Net Core as opposed to .Net Framework.
// HOWEVER, you CAN still include a readme.txt that the nuget package manager will show you when you first install a package.
//
// There are two files contained within this readme.txt
// 1. The sample code. The various classes are all in one "file" for ease of distribution like this.
// 2. A sample application settings file in JSON format. The sample assumes it's called "Moonrise.Samples.appSettings.json"
//    and "informs" the Moonrise Settings class of the name. The default is to assume it's called "appSettings.json"
// 
// PLEASE read the comments in the sample "program" and if you look in your local nuget cache - usually found under [User folder]\.nuget\packages -
// you will find in the Moonrise.Samples folder that there is a Moonrise.chm help file which is a good reference.
//
// FINALLY, if there is an update to this Moonrise.Samples package, then please upgrade by uninstalling before re-installing the new version
// as this way you guarantee that this readme.txt will be redisplayed with, of course, the updated sample code!
// 
// P.S. I'm using .tt/T4 templates to include the code and settings from the actual code & settings file. It's great, saves me having to copy & paste
// manually into a readme!
//
// Okay, off we go. Cut the content between the +++ & --- comments into Moonrise.Samples.cs and call Moonrise.Samples.Run(); from your console app main
//+++ ----------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using Moonrise.Logging;
using Moonrise.Logging.LoggingProviders;
using Moonrise.Utils.Standard.Config;
using Moonrise.Utils.Standard.DatesTimes;
using Moonrise.Utils.Standard.Exceptions;
using Moonrise.Utils.Test.ObjectCreation;
using Moonrise.Utils.Standard.Extensions;
using Moonrise.Utils.Standard.Exceptions;

namespace Moonrise
{
    /// <summary>
    ///     Samples of how to use the Moonrise library.
    /// </summary>
    /// <remarks>
    ///     NOTE: Every public and protected Moonrise member has documentation comments so you'll get more info on hovering
    ///     over elements.
    /// </remarks>
    public class Samples
    {
        public static void Run()
        {
            try
            {
                SuggestedInitialisation();
                SampleLogging();
                SampleTestObjectCreation();
                TrySomeParsing();
                ReasonedExceptions();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// I REALLY like these. A <see cref="ReasonedException{TReason}"/> is an exception that is only ever created with an enum "Reason".
        /// The enum is given a [Description] which becomes the text message for the exception and will often include {0} {1} placeholders
        /// so that when it comes to throw that exception, you pass the variables appropriately and you've got a well though out reason
        /// for throwing your exception. You also get a list of all the reasons this exception may be thrown, by virtue of the enums.
        /// Take a looksie.
        /// </summary>
        private static void ReasonedExceptions()
        {
            try
            {
                throw new SampleException(SampleException.SampleReason.BecauseINeedToStartSomewhere);
            }
            catch (SampleException e)
            {
                Logger.Error(e);
            }

            try
            {
                // Note how, because the reason's description has been repeated in the XML comment, when
                // you hover the cursor over the enum you'll see the message and any parameters required for that message
                // This keeps your code cleaner and if it's an exception that might get raised in several different places
                // keeps the message CONSISTENT!
                throw new SampleException(SampleException.SampleReason.OKNextPlease, "The offending string");
            }
            catch (SampleException e)
            {
                Logger.Error(e);
            }

            try
            {
                try
                {
                    int first = 34;
                    int second = 28;

                    if (second == 28)
                        throw new SampleException(SampleException.SampleReason.SeeWhatIMean, first, second);
                }
                catch (SampleException e)
                {
                    throw new AnotherSampleException(e, OtherReason.OrWhatever);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            try
            {
                int valve = 279;
                string door = "Main evacuation";

                throw new AnotherSampleException(OtherReason.DoorShouldNotBeOpenWhenOpeningReserveTank, valve, door);
            }
            catch (AnotherSampleException e)
            {
                Logger.Error(e);
            }

            try
            {
                int valve = 279;

                // Just throwing this one in - throw, gettit? I'll shut up - to show you the DateTimeProvider (there's also a DateTimeOffsetProvider)
                // I always use the DT/DTOProviders rather than DT/DTO as I can easily provide a known value through a specific IDateTimeProvider for test purposes.
                // The .Now static property is thread-safe (in that if you've provided a provider each will be a separate instance which means your tests will
                // be fine if run threaded. The default behaviour is to give DateTime.Now if no provider is set up.
                throw new AnotherSampleException(OtherReason.ValveIsStuck, DateTimeProvider.Now, valve);
            }
            catch (AnotherSampleException e)
            {
                Logger.Error(e);

                // Oh, while we're taking a look at DateTime stuff, checkout the .Within extension method!
                DateTime pointInTime = new DateTime(2018, 11, 1);

                if (!pointInTime.Within(5, LastFew.Years))
                    throw new ApplicationException("Go on, I dare you!");
            }

        }

        /// <summary>
        /// An example of a <see cref="ReasonedException{TReason}"/> where the reason is encapsulated within the exception.
        /// </summary>
        /// <remarks>
        /// This style makes for slightly more convoluted naming of the reason when reading/throwing the exception but
        /// is perhaps better contained.
        /// </remarks>
        /// <seealso cref="Moonrise.Utils.Standard.Exceptions.ReasonedException{Moonrise.Samples.SampleException.SampleReason}" />
        public class SampleException : ReasonedException<SampleException.SampleReason>
        {
            public enum SampleReason
            {
                /// <summary>
                /// Thinking of examples is sometimes tough, but I need to start somewhere!
                /// </summary>
                [Description("Thinking of examples is sometimes tough, but I need to start somewhere!")]
                BecauseINeedToStartSomewhere,

                /// <summary>
                /// For this exception we'll take a string that might help to diagnose:{0}
                /// </summary>
                [Description("For this exception we'll take a string that might help to diagnose:{0}")]
                OKNextPlease,

                /// <summary>
                /// You tried to divide {0} by {1} but I don't what to have to do that!
                /// </summary>
                [Description("You tried to divide {0} by {1} but I don't what to have to do that!")]
                SeeWhatIMean
            }

            public SampleException(SampleReason reason, params object[] args) : base(reason, args)
            {
            }

            public SampleException(Exception innerException, SampleReason reason, params object[] args) : base(innerException, reason, args)
            {
            }
        }

        /// <summary>
        /// A bunch of other reasons for an exception being thrown.
        /// </summary>
        /// <remarks>
        /// Note how easy it is to see all of the possible reasons the exception might be thrown. What tends to happen when I use
        /// this pattern is I just add another reason as I find I need to throw another exception and the list just grows.
        /// </remarks>
        public enum OtherReason
        {
            /// <summary>
            /// It wasn't possible to complete the reverse flush at {0} as valve {1} appears to be stuck!
            /// </summary>
            [Description("It wasn't possible to complete the reverse flush at {0} as valve {1} appears to be stuck!")]
            ValveIsStuck,

            /// <summary>
            /// Unable to open reserve tank number {0} as the {1} door has been left open.
            /// </summary>
            [Description("Unable to open reserve tank number {0} as the {1} door has been left open.")]
            DoorShouldNotBeOpenWhenOpeningReserveTank,

            /// <summary>
            /// Bored now
            /// </summary>
            [Description("Bored now")]
            OrWhatever
        }

        /// <summary>
        /// An example of a <see cref="ReasonedException{TReason}"/> where the reason is declared alongside the exception.
        /// </summary>
        /// <remarks>
        /// This style makes for slightly more readable exception throwing.<para>
        /// Remember that as for all of these examples you would ordinarily have these classes in their own files, except that in
        /// this case of the <see cref="OtherReason"/> enum I would keep it in the same file as the exception using it.
        /// </para>
        /// </remarks>
        public class AnotherSampleException : ReasonedException<OtherReason>
        {
            public AnotherSampleException(OtherReason reason, params object[] args) : base(reason, args)
            {
            }

            public AnotherSampleException(Exception innerException, OtherReason reason, params object[] args) : base(innerException, reason, args)
            {
            }
        }

        /// <summary>
        /// There are some neat string extension methods that really help out when you're doing some casual string parsing.
        /// </summary>
        private static void TrySomeParsing()
        {
            string input = "Some text in a string, how shall we process the process?";

            string getThe = input.Extract("process", "process");
            Debug.Assert(getThe.Equals("the"));

            input = "aw:get this, aw:dude you're, aw:hurting my foot!";
            int start = 0;
            string extracted = input.Extract(ref start, "aw:", 2, ",");
            Logger.Debug(extracted);

            input = input.ReplaceBetween("aw:", ",", "dogs on the leash");
            Logger.Debug(input);
        }

        /// <summary>A sample of how to create test data with the <see cref="Moonrise.Utils.Test.ObjectCreation.Creator" />!</summary>
        /// <remarks>
        ///     I recommend commenting out the SampleLogging to better see the reliability of rerunning repeatable random data
        ///     in the log file!
        /// </remarks>
        private static void SampleTestObjectCreation()
        {
            // Let's start off with a basic creator that creates random data (well as random as the standard RNG can manage).
            var creator = new Creator();
            Logger.Title("Data creation samples");
            Logger.Debug("First non-repeating random data");
            Logger.Debug(creator.GetRandomString());
            Logger.Debug(creator.GetRandomString());

            // Try exploring the different .GetRandomXxxx() methods.
            Logger.Debug(creator.GetRandomLong());

            // Note that every time you run the above you'll get different values. But sometimes you might want repeatedly reliable random data,
            // in which case simply seed your creator with the same seed each time
            var reliableCreator = new Creator(42);
            Logger.Debug("Now repeatable random data");
            Logger.Debug(reliableCreator.GetRandomString());
            Logger.Debug(reliableCreator.GetRandomString());
            Logger.Debug(reliableCreator.GetRandomLong());
            Logger.Debug(reliableCreator.GetRandomDateTime().ToString());

            // Okay, let's up the ante somewhat and create a class with filled random data!
            Logger.Debug(creator.CreateFilled<Initialise.LoggingConfig>());
            Logger.Debug(creator.CreateFilled<Initialise.LoggingConfig>());
            Logger.Debug(reliableCreator.CreateFilled<Initialise.LoggingConfig>());
            Logger.Debug(reliableCreator.CreateFilled<Initialise.LoggingConfig>());

            // Cool huh? Buuuut, not very representative of the sort of data you might want to test with, so now let's control that by using
            // an interface that's been annotated to fine tune that data.
            // First we'll associate an interface with one of the classes in the object tree
            creator.MapInterfaceAttributes<BasicFileLogProvider.Config, IFileLoggerTestData>();
            Initialise.LoggingConfig randomConfig =
                creator.CreateFilled<Initialise.LoggingConfig, ILoggingConfigTestData>();
            Logger.Debug(randomConfig);

            // Okay, so since I haven't (YET) made a creation attribute for a filepath, let's assign a random one through the Get... method
            randomConfig.LogFile.LoggingFile = creator.GetRandomFilePath();
            Logger.Debug("And with the filepath updated to a random but recognisable file path....");
            Logger.Debug(randomConfig);
        }

        /// <summary>
        ///     Samples of how logging can be used. You can tweak what makes it out into the log file by changing the logging level
        ///     in appsettings.json
        ///     They are essentially "Debug", "Info", "Warning", "Error", "Fatal"
        /// </summary>
        private static void SampleLogging()
        {
            LoggingContexts();
            LoggingTags();
            LoggingExceptionsAndObjects();

            // Why don't XML Doc comments work here? Anyway.
            // This method shows how logging context works and is scoped. Scope COULD be method names but are probably better used in a wider way.
            // Up to you, but this is how they work.
            void LoggingContexts()
            {
                Logger.Info("This is outside any context");

                using (Logger.Context("Context 1"))
                {
                    Logger.Debug("Inside the first context");

                    using (Logger.Context("Context 2"))
                    {
                        Logger.Debug("Inside the second context");
                        LoggingContextsAlsoWorkInsideMethods();
                    }

                    Logger.Debug("Outside the second context but inside the first context");
                }

                Logger.Error("And now we're back outside any contexts");

                void LoggingContextsAlsoWorkInsideMethods()
                {
                    Logger.Warning("So, inside second context from inside a method");
                    Logger.LogMethodName = true;
                    Logger.Debug("We're now logging the method name");
                    var areUsingContext = Logger.UseContext;
                    Logger.UseContext = false;
                    Logger.Debug("We're now logging the method name, but without the context");
                    Logger.LogMethodName = false;
                    Logger.UseContext = areUsingContext;
                    Logger.Debug("Now we're not logging the method name");
                }
            }

            // Log tags are a way to get finer grained logging that can be switched on or off in addition to the typical logging levels.
            // I've always tended to activate log tags at startup, based on config file settings, but they could be switched dynamically
            // once you have a way to communicate what tags should be active!
            void LoggingTags()
            {
                Logger.LogActiveLogTags();

                // All contained logging will be tagged, including any scoped methods 
                using (Logger.ScopedLogTag(LogTags.Defined))
                {
                    Logger.Warning(
                        "It's worth knowing that only Info & Debug can get tagged & thus switched off. Anything more serious shouldn't be switchable!");
                    Logger.Debug("A debug message with the tag Defined");
                    Logger.Info("An info message with the tag Defined");
                    Logger.Debug("The previous two used the scoped tag but for this one I'll use a different tag",
                        LogTags.AreCurrently);
                    ButWhatAboutLoggingFromSubroutines();
                }

                using (Logger.ScopedLogTag())
                {
                    Logger.Debug("This one will use a tag of SampleLogging, NOT LoggingTags");
                    ButWhatAboutLoggingFromSubroutines();
                }

                var madeUpOnTheFly = new LogTag("MadeUpOnTheFly");
                Logger.ActivateLogTag(madeUpOnTheFly);
                Logger.Info("Adding a new log tag", madeUpOnTheFly);
                Logger.DeactivateLogTag(madeUpOnTheFly);
                Logger.Info("This one won't appear!", madeUpOnTheFly);


                void ButWhatAboutLoggingFromSubroutines()
                {
                    Logger.Info("If they don't specify a tag, they will take on whatever tag scope is active");
                    Logger.Debug("Unless of course they use their own override", LogTags.NoTags);
                }
            }

            // Here I show you how to log an exception and objects in general
            void LoggingExceptionsAndObjects()
            {
                try
                {
                    try
                    {
                        Logger.Info("You'll get a JSON representation of the Logging Configuration");
                        Logger.Info(Initialise.LoggingConfiguration);

                        throw new AmbiguousImplementationException("Now let's log an exception!");
                    }
                    catch (Exception excep)
                    {
                        Logger.Error(excep);

                        try
                        {
                            Logger.Info("It'll even handle nested exceptions");

                            throw new BadImageFormatException("The outer exception message", excep);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Info(
                        "Previously logged exceptions log less verbosely when logged again in a cascading catch");
                    Logger.Error(e);
                }
            }
        }

        /// <summary>
        ///     This is how I suggest you initialise, as early as you can in your entry point.
        /// </summary>
        private static void SuggestedInitialisation()
        {
            Initialise.ConfigurationSettings();
            Initialise.Logging();
        }


        /// <summary>
        /// This interface is used purely as a vehicle to pass <see cref="ObjectCreationAttribute"/>s when creating random data
        /// in a <seealso cref="BasicFileLogProvider.Config"/> object. Nothing implements this interface but it has members whose
        /// name & type match the members in that class and the attributes against this interface then govern how the test data
        /// gets created. And in case it hasn't dawned on you yet, YES you could use lots of different interfaces with different
        /// criteria against the same class you're creating and run different sorts of tests - if you wanted to!
        /// <seealso cref="Creator.MapInterfaceAttributes{T,I}"/>.
        /// </summary>
        public interface IFileLoggerTestData
        {
            // We're not trying to generate proper looking date time formats here, but illustrating that we can limit the random characters that
            // make up the string to a specific allowable list - as defined by the string we pass.
            [ObjectCreation(StringSource = "hmdHMSTu")]
            public string DateTimeFormatterPrefix { get; set; }

            public BasicFileLogProvider.Cycle LogCycling { get; set; }
            // We won't bother giving a boolean a creation attribute so we won't include it in the interface.
            // Remember that this is not an interface that needs to be implemented, it's used purely as a vehicle
            // to convey attributes to the object creation for matching members (same name & type) and if there
            // isn't one, the member will be created as normal without any creation attributes guiding it.
            //public bool LogFilePerThread { get; set; }

            // The Creator class has a set of predefined string sources of the most likely ones you'll want to use.
            // There is in fact a Creator method to create somnething that looks like a filepath but I haven't yet
            // made an ObjectCreationAttribute member to reflect that - I should do!
            [ObjectCreation(StringSource = Creator.StringSources.AlphaNumericCharactersWithSpaces)]
            public string LoggingFile { get; set; }

            // IF the member on the object being instantiated has validation range(etc) attributes, then the creator WILL respect that
            // if an [ObjectCreation(RespectValidation=true)] is used.
            [ObjectCreation(MinInt = 28, MaxInt = 98)]
            public int MaxEntries { get; set; }

            [ObjectCreation(ItemsSource = typeof(StringItemsSourceExample))] public string ByCountFilenameDateTimeFormat { get; set; }
        }

        /// <summary>
        ///     The ObjectCreationAttribute makes use of "static interfaces" - well that's what I call them. Yes, I know, there's
        ///     no such thing because as soon as you make the class static - which it needs to be to be useable by the
        ///     ObjectCreationAttribute - that's illegal as a static class isn't allowed to implement an interface!
        ///     So, the trick is to implement the interface as non-static, then comment out the implementation. That at least let's
        ///     you know what static things you need to implement for this to work!
        /// </summary>
        /// <seealso cref="string" />
        public static class StringItemsSourceExample // : ObjectCreationAttribute.SIItemSource<string>
        {
            /// <summary>
            ///     Gets the list of objects to be used as the item source for
            ///     <see cref="P:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1.ElementName" />. IF the
            ///     ItemsSource attribute
            ///     DOES supply a static
            ///     <see
            ///         cref="M:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1.ItemSource(System.String,System.Reflection.PropertyInfo,System.Reflection.FieldInfo,Moonrise.Utils.Test.ObjectCreation.Creator)" />
            ///     method then
            ///     it will be called in preference to the property, unless your
            ///     <see cref="T:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1" /> implements the static
            ///     property
            ///     <see
            ///         cref="M:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1.PreferPropertyToItemSourceCall(System.String)" />
            ///     , in which case
            ///     it will use that to decide whether to use the property OR the static
            ///     <see
            ///         cref="M:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1.ItemSource(System.String,System.Reflection.PropertyInfo,System.Reflection.FieldInfo,Moonrise.Utils.Test.ObjectCreation.Creator)" />
            ///     method to supply
            ///     the source list.
            /// </summary>
            /// This is how the default interface implementation will get created, you change ElementName to the name of the element/member you are creating through this call.
            //public static IList<string> ElementName { get; }
            public static IList<string> ByCountFilenameDateTimeFormat => new List<string> {"Format1", "Format2", "etc"};

            /// <summary>
            ///     Returns the list of objects to be used as the item source for elementName
            /// </summary>
            /// <param name="elementName">Name of the element.</param>
            /// <param name="propInfo">The property information - Only if it's a property, null otherwise.</param>
            /// <param name="fieldInfo">The field information - Only if it's a field, null otherwise.</param>
            /// <param name="creator">The instance to use to help you create a random instance, if you need to.</param>
            /// <returns>
            ///     List of objects, to be used to randomly choose from to fill the property or field that the attribute has been
            ///     applied
            ///     to
            /// </returns>
            /// <exception cref="System.NotImplementedException"></exception>
            public static IList ItemSource(string elementName, PropertyInfo propInfo, FieldInfo fieldInfo, Creator creator)
            {
                if (elementName.Equals("ByCountFilenameDateTimeFormat"))
                    throw new NotImplementedException("This shouldn't happen as we've said to use the static property in preference to this method that COULD be used to process a whole set of different elements.");

                return null;
            }

            /// <summary>
            ///     Determines if the
            ///     <see cref="P:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1.ElementName" /> or the
            ///     <see
            ///         cref="M:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1.ItemSource(System.String,System.Reflection.PropertyInfo,System.Reflection.FieldInfo,Moonrise.Utils.Test.ObjectCreation.Creator)" />
            ///     is to be used as the source for
            ///     elements for the elementName /&gt;.
            /// </summary>
            /// <param name="elementName">Name of the element.</param>
            /// <returns>
            ///     True means use the static
            ///     <see cref="P:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1.ElementName" /> property
            ///     where it exists, False means use the static
            ///     <see
            ///         cref="M:Moonrise.Utils.Test.ObjectCreation.ObjectCreationAttribute.SIItemSource`1.ItemSource(System.String,System.Reflection.PropertyInfo,System.Reflection.FieldInfo,Moonrise.Utils.Test.ObjectCreation.Creator)" />
            ///     method.
            /// </returns>
            /// <exception cref="System.NotImplementedException"></exception>
            public static bool PreferPropertyToItemSourceCall(string elementName)
            {
                return elementName.Equals("ByCountFilenameDateTimeFormat");
            }
        }

        /// <summary>
        ///     The ObjectCreationAttribute makes use of "static interfaces" - well that's what I call them. Yes, I know, there's
        ///     no such thing because as soon as you make the class static - which it needs to be to be useable by the
        ///     ObjectCreationAttribute - that's illegal as a static class isn't allowed to implement an interface!
        ///     So, the trick is to implement the interface as non-static, then comment out the implementation. That at least let's
        ///     you know what static things you need to implement for this to work!
        /// </summary>
        /// <seealso cref="string" />
        public static class StringListItemsSourceExample // : ObjectCreationAttribute.SIItemSource<string>
        {
            /// <summary>
            ///     Returns the list of objects to be used as the item source for elementName
            /// </summary>
            /// <param name="elementName">Name of the element.</param>
            /// <param name="propInfo">The property information - Only if it's a property, null otherwise.</param>
            /// <param name="fieldInfo">The field information - Only if it's a field, null otherwise.</param>
            /// <param name="creator">The instance to use to help you create a random instance, if you need to.</param>
            /// <returns>
            ///     List of objects, to be used to randomly choose from to fill the property or field that the attribute has been
            ///     applied
            ///     to
            /// </returns>
            /// <exception cref="System.NotImplementedException"></exception>
            public static IList ItemSource(string elementName, PropertyInfo propInfo, FieldInfo fieldInfo, Creator creator)
            {
                if (elementName.Equals("LogTags"))
                {
                    List<object> retVal = new List<object>
                    {
                        new List<string>{"Tag One", "Tag Too", "Another Tag"},
                        new List<string>{"TagHuer", "ShoppingTag", "Run out of Tag Puns", "Stringer"}
                    };

                    retVal.Add(creator.CreateFilled<List<string>>());

                    return retVal;
                }

                return null;
            }

            public static List<object> LogTags
            {
                get
                {
                    return new List<object>
                    {
                        new List<string> {"Tag One", "Tag Too", "Another Tag"},
                        new List<string> {"TagHuer", "ShoppingTag", "Run out of Tag Puns", "Stringer"}
                    };
                }
            }

            public static bool PreferPropertyToItemSourceCall(string elementName) => true;
        }

        public interface ILoggingConfigTestData
        {
            [ObjectCreation(AllowNullElementsInEnumerable = true,
                ItemsSource = typeof(StringListItemsSourceExample))]
            public List<string> LogTags { get; set; }
        }
    }

    /// <summary>
    ///     A static class to encompass initialisation of various cross-project entities.
    /// </summary>
    /// <remarks>
    ///     Ordinarily this would of course be in a separate file but for ease of reference I've pulled it in here!
    /// </remarks>
    public static class Initialise
    {
        /// <summary>The logging configuration as a variable</summary>
        public static LoggingConfig LoggingConfiguration = new LoggingConfig();

        public static LoggingConfig LoggingConfigurationAsProperty { get; set; } = new LoggingConfig();

        /// <summary>
        ///     Initialisation of the configuration settings infrastructure
        /// </summary>
        /// <remarks>
        ///     Please read the NOTE on the <see cref="Logging()" /> method, it applies equally to Settings - though there's less
        ///     of a need
        ///     with configuration settings to avoid passing an ISettings around multiple places the arguments are the same, it's
        ///     simply easier
        ///     "my way" but without any particular risk.
        ///     <para>
        ///         What I do want to say about settings though is very relevant. To initialise an object, whatever that object is,
        ///         I STRONGLY
        ///         do NOT believe that you should be passing it an interface to a settings mechanism and expecting it to fetch
        ///         that data. NO!
        ///         Instead you should be passing it the configuration DATA it needs, either as individual parameters or as I have
        ///         preferred
        ///         in recent years, a locally defined Config class. I use a config class to "signifcant 'er objects" -
        ///         controllERs, managERs,
        ///         etc (what we used to refer to in HOOD/Ada as "active objects", yes I've been doing this a loooong time) - even
        ///         if there's
        ///         a single initialisation parameter required, as it's both easier when you extend that, neater AND most
        ///         importantly
        ///         semantically clearer.
        ///     </para>
        ///     <para>
        ///         Using <see cref="Moonrise.Utils.Standard.Config.Settings" />you can very easily read entire objects or object
        ///         trees.
        ///     </para>
        /// </remarks>
        public static void ConfigurationSettings()
        {
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider(new JsonConfigSettingsProvider.Config
            {
                ApplicationSettingsFilename = "Moonrise.Samples.appSettings.json"
            });
        }

        /// <summary>
        ///     Initialisation of the logging infrastructure. We're using a <see cref="BasicFileLogProvider" /> to log to a file
        ///     specified in the <see cref="LoggingConfig" />.
        /// </summary>
        /// <remarks>
        ///     NOTE: What you're about to read is not always popular amongst the "coderati". Moonrise.Logging makes use of ......
        ///     SINGLETONS(!), sort of. But a nicer singleton, probably more like a global variable (boo hiss!), but nicer still!
        ///     <para>
        ///         You access virtually all of the logging functionality via the static members of
        ///         <see cref="Moonrise.Logging.Logger" />.
        ///         Meaning you have ONE place to go to for your logging, you don't have to pass around a variable everywhere you
        ///         want to do
        ///         some logging, you ALWAYS know where to go!
        ///     </para>
        ///     <para>
        ///         "But isn't that REALLY BAD" I hear you sob? Not if you're smart about it, no. When you call the static method
        ///         <see cref="Debug.)" />
        ///         that will eventually (the calls aren't too deep, it's pretty efficient) result in a call to the static property
        ///         <see cref="Logger.LogProvider" />
        ///         which is an interface - <seealso cref="Moonrise.Logging.ILoggingProvider" />.
        ///     </para>
        ///     <para>
        ///         The fact it is an interface gets around one problem of traditional singletons, tight coupling. You can use
        ///         whatever provider implementation
        ///         you wish - just like the .Net Core logging! More than that, the property is actually a ThreadLocal of an
        ///         <see cref="ILoggingProvider" /> which
        ///         means you get a new one for each thread - An <see cref="Moonrise.Logging.ILoggingProvider" /> must also
        ///         implement <see cref="Utils.Standard.ICloneable" />.which
        ///         allows a new provider to be created per thread as the need arises!
        ///     </para>
        ///     <para>
        ///         Another "problem" with singletons is they violate the SRP principle because they control their own lifetime.
        ///         Whilst that CAN be true for
        ///         <see cref="Logger" /> - if you don't provide a LogProvider instance for it to use via
        ///         <see cref="Logger.LogProvider" /> property it will
        ///         use itself as that provider and by default will output all logging to the console - you will generally provide
        ///         a (or perhaps several)
        ///         LogProvider(s) to use.
        ///     </para>
        ///     <para>
        ///         Finally, for testing purposes you could quite happily leave logging in and just make sure you set up a logger
        ///         in your test setup code, or
        ///         leave it to simply log out to the Console OR use the TestLogAuditProvider in the Moonrise.TestUtils package and
        ///         then you could test
        ///         that when errors or warnings occur that they do actually get logged!
        ///     </para>
        ///     <para>
        ///         So if you have any prejudices, put them aside and think properly about what your concerns might be. I am happy
        ///         to answer any questions
        ///         or have a discussion. My email is will@moonrise.media and my personal number is +44 7446 232739
        ///     </para>
        /// </remarks>
        public static void Logging()
        {
            Settings.Application.Read("Logging", ref LoggingConfiguration, true);

            // You could also read this into a LoggingConfig property - yes it's perhaps a bit of a pain to repeat the property twice but that's just how you need to do it!
            // POINT IS, if you want to use it as a property, then go ahead, you can!
            Settings.Application.Read("Logging", LoggingConfigurationAsProperty, () => LoggingConfigurationAsProperty,
                true);

            if (LoggingConfigurationAsProperty.Level != LoggingConfiguration.Level)
                throw new ApplicationException("Go on, I dare you!");

            Logger.LogProvider = new BasicFileLogProvider(LoggingConfiguration.LogFile);
            Logger.OutputLevel = LoggingConfiguration.Level;
            Logger.UseContext = LoggingConfiguration.UseLoggingContext;
            Logger.ActivateLogTags(LoggingConfiguration.LogTags);
            Logger.Seperate('*');
            Logger.Debug("Just logging the current logging settings");
            Logger.Debug(LoggingConfiguration);
            Logger.Title("Let the Samples Begin!");
        }

        /// <summary>
        ///     The logging configuration class/structure to be read from the appsettings.json or overrides.
        /// </summary>
        public class LoggingConfig
        {
            /// <summary>
            ///     The level of logging to report - messages of this level and higher priority will get logged
            /// </summary>
            public Logger.ReportingLevel Level { get; set; } = Logger.ReportingLevel.Information;

            /// <summary>
            ///     File logger configuration
            /// </summary>
            public BasicFileLogProvider.Config LogFile { get; set; } = new BasicFileLogProvider.Config();

            /// <summary>
            ///     A list of the log tag names that should be considered active to be logged
            /// </summary>
            public List<string> LogTags { get; set; } // = new List<string>{ "NoTags", "AreCurrently", "Defined"};

            /// <summary>
            ///     True if logging context is to be emitted
            /// </summary>
            public bool UseLoggingContext { get; set; } = true;
        }
    }

    /// <summary>
    ///     The set of <see cref="Moonrise.Logging.LogTag" />s  that I happen to be using in this sample code are being defined
    ///     as a set of
    ///     "constants" so that we know what we can use. You COULD create a LogTag on the fly all over the place but then it
    ///     wouldn't be so
    ///     clear just what tags are being used. Better to have a set defined in one or a few places. Most likely you'd declare
    ///     a set per assembly.
    /// </summary>
    /// <remarks>
    ///     Okay so what are LogTags? Well, they're a little like the .Net Core logging concept of "Categories". It's a way you
    ///     can fine tune
    ///     what logging is emitted by filtering (as the .Net Core logging descriptions refer to it) logs according to what
    ///     logtags have been activated.
    ///     <para>
    ///         You can adjust the tags that are active dynamically, IF you can get a way of trigger that change by using
    ///         <see cref="Logger.ActivateLogTag" />(s) and
    ///         <see cref="Logger.DeactivateLogTag" />(s).
    ///     </para>
    /// </remarks>
    public struct LogTags
    {
        /// <summary>I suggest to use this pattern for creating log tags</summary>
        public static LogTag NoTags = new LogTag(nameof(NoTags));

        public static LogTag AreCurrently = new LogTag(nameof(AreCurrently));
        public static LogTag Defined = new LogTag(nameof(Defined));
    }
}
//--- ----------------------------------------------------------------------------------------------------------------

// Cut the remainder of the following content into Moonrise.Samples.appSettings.json and set the "Copy to Output Directory" to "Always"
//+++ ----------------------------------------------------------------------------------------------------------------
// This is a standard .Net JSON settings file. You COULD process it using the standard .Net core way of 
// reading it - can't remember what that is as it's ages since I used that. I do remember that it relies
// on injecting a settings handler. The problem I have with that is that you have to keep injecting the appropriate
// reference and accepting it etc. Same thing with logging, in fact for logging it's much worse. Why?
// Well settings you generally only read during some startup phase, whereas logging you want to be able to use EVERYWHERE
// and I for one do NOT want to keep injecting logger references to every single class that I might want to log from!
// I explain more in the Samples code. The other advantage of the Moonrise Settings is "hydrating" complete object
// trees. Again see the example for more details!
{
  // If you want to use BOTH Moonrise logging and .Net Core logging - which you can quite easily there are Moonrise providers
  // to allow Moonrise logging to ultimately go through to the .Net Core logging and to provide a .Net Core logging provider
  // that will feed into what Moonrise logging providers you're using - then you might want to rename this part of the
  // settings from Logging to perhaps MoonriseLogging or whatever. You'd just change the corresponding Settings.Read(...)
  "Logging": {
    // Everything inside this bit of settings tree now matches the structure of the Moonrise.Samples.Initialise.LoggingConfig class
    "Level": "Debug",
    "LogFile": {
      // Note that this settings subtree now matches the structure of the Moonrise.Logging.BasicFileLogProvider.Config class,
      // i.e. showing a sample of reading complete object trees.
      "DateTimeFormatterPrefix": "{0:HH:mm:ss} ",
      "LogCycling": "Weekly",
      "LogFilePerThread": false,
      "LoggingFile": "../../../Logging.log"
    },
    // Generally you want to put all of the LogTags that you use in here.
    // Then to turn them Off or On, simply put or remove an x from in front of them. The LogTags are simply matched 
    // against a string so if the active ones have an x in front, they won't match, remove the preceeding x and 
    // they will match!
    "LogTags": [
      "NoTags",
      "AreCurrently",
      "Defined",
      "xSampleLogging"
    ],
    "UseLoggingContext": true
  }
}

//--- ----------------------------------------------------------------------------------------------------------------
