# Moonrise.StandardUtils.NetStd
A collection of really useful little classes and extension methods that I've accumulated over the years just to make my life (and yours should you choose it)' easier.
## Config.Settings
Settings will work with either JSON (by default) or XML configuration files. It is far less faff than using the "built-in" way, provides the ability to write out settings that haven't yet been created, works with both application and user settings **and** provides fully transparent machine or user level encryption. It uses an ISettingsProvider **if** you need to roll your own or test using mocked settings. It works using a *Thread Global Singleton* which means any (unlikely) changes to the provider are maintained on the thread that made the change. i.e. It is fully thread-safe. You don't need any of the Microsoft infrastructure to use this - though you can still use it side-by-side as it simply reads the config file - so it's great for all application types.
### Usage
```C#
    internal class MyConfigClass
    {
        public int AnInt { get; set; } = 42;
        public LoggingLevel LoggingLevel { get; set; } = LoggingLevel.Debug;
        public string AString { get; set; } = "Initial Value";
        public AnotherConfigClass AdditionalConfig { get; set; } = new AdditionalConfig();
    }
    ...
    MyConfigClass Config;
    ...
    // Read the config into the instance of the config class. The (optional) true param says, if that setting is not there, then write it in.
    Settings.Application.Read("MyConfigLocation", ref Config, true);
```
So this shows how nested configuration can be read in one hit **and** if it's not in there, it'll get written in for you - so you don't have to hand roll the config. The written file will be with your .exe so down in the bin folder. So open that up, copy out the created settings, paste the, back into your source version of the settings file and tweak as required.
### Initialisation
By default Settings will use a JsonConfigSettingsProvider, which by default uses an appSettings.json file, so unless you need to override anything, just using Settings.Application or Settings.User "out of the box" will **just work**.
### Opinion
I **do not** like the concept of injecting a settings object into different classes as Microsoft seem to encourage. Why? I believe that breaks the SRP (Single Responsibility Principal). I believe that a class should define its "data requirements" and be passed that data. It shouldn't be left for that class to pull its initialisation data out of a particular store. What if you want to store your config in a database? What if you want it to be dynamic? Well in both those cases you'll inevitably use either a bunch of parameters or my general way of choice, a config class.

What I do for my startup code is create a collated configuration class that uses nested configuration for all of the configuration anything needs to get from settings, read it all in one shot and where that config is required for any DI constructed classes register the config appropriately. Now when you come to test, you don't need to faff about with different settings files or mocjed settings providers, just give the class under test the data it wants. Simples!
### User Settings
Instead of Settings.Application just use Settings.User. The settings file will be located in the user's app data folder.
### Encryption
Settings can make use an encryption provider to decrypt an encrypt settings, either in a group or individually. The supplied DpApiSettingsEncryptor makes use of the Data Protection API to encrypt or decrypt using the keys in either the User or Machine store. Since Settings can be written there is an option to write them as encrypted, these will then be passed through the encryption provider to encrypt. There is a command-line utility EncryptAppSettings.exe that is bundled in the Moonrise.Samples Nuget package - you'll need to dig into the package folder in your cache. Or you can simply roll your own. Encrypted data is written as Base64 between a start marker of **"[{ENC]{"** and an end marker of **"]{ENC[{"** - chosen for the **extreme** unlikelihood of those sets of characters occurring naturally in a settings file. If they might you are free to choose your own set, but then you'd need to rebuild EncryptAppSettings.exe!

Using the DpApiSettingsEncryptor (as used by the afore-mentioned .exe) means you either encrypt using User or Machine level stores. User level means the encrypted settings can only be read by processes running under that user account but can be done so on any machine. Machine level means the encrypted settings can only be read on the machine that did the encryption.
## DateTime Providers
An interfaced DateTime/Offset provider that supplies a .Now that you can control, particularly for testing.
## Reasoned Exceptions
I **REALLY** like these. A ReasonedException<TEnumReason> is an exception that is only ever created with an enum "Reason". The enum is given a [Description("description text")] which becomes the text message for the exception and will often include {0} {1} {etc} placeholders so that when it comes to throw that exception, you pass the variables appropriately and you've got a well though out reason for throwing your exception. You also get a list of all the reasons this exception may be thrown, by virtue of the enums. If you then additionally use XML comments for the enum members that completely matches your [Description("description text")] you will get Intellisense at the point where the exception is thrown giving you the text. This pattern/approach means you have very tight control over the exceptions that you throw. You have defined reasons and controlled text so that you are not making up potentially different messages for the same reason, except of course for any specifics of the referenced variables in play.
## Extensions
A bunch of useful extension methods
### ClassExtensions
.DisplayName - Gets the text of any DisplayAttribute applied to the property of typically a view model class

.MethodName/FQMethodName - Gets the name of the current method, qualified with the class or FQ class name. Simple but sometimes handy.
### DateTime/OffsetExtensions
.Within - Is the DateTime/Offset within the last few X of now - Seconds, Minutes, Years etc.

.TrimOff - Truncates a DateTime/Offset to the required unit.
### EnumExtensions
.Description - gives the description of an enum as defined by its, in this order, changed description (see later), [Description] or string value (which for PascalCased values will be "Pascal Cased").

Enumerable<TEnum> - does what it says on the tin.

FromString<TEnum> - converts an enum from its string representation - the opposite of .Description.

In - determines if an enum is in a range of specified values.

.ToInt - dies what is says on the tin.

.ModifyDescription - allows you to effectively modify the description attribute. I've used this in internationalisation, reading values from a database and updating the enum description.

.OriginalDescription - Like .Description but jumps past any modified description.
### StringExtensions
.CSL - gives a Comma (actually any string you want) Separated List from an IEnumerable - OK, not **strictly** a string extension!

.Description - gets the [Description] of any object that had that attribute applied, or its name otherwise - OK, not **strictly** a string extension!

.Extract - extracts a string from a string between two specified strings at a particular starting point. Great for ad-hoc parsing.

.FindEnd/.FindStart - does what it says on the tin. Used by .Extract.

.IndexOf - finds the location of a string in a string but with options to ignore case and whitespace

.LastIndexOf - as above.

.Left/Mid/Right - gets substrings to the left middle or right of a string.

.Pluralise - Pretty good pluraliser that can include counts and use a custom plural string.

.ReplaceBetween - Replaces one string with another between two sets of strings.

.ToIntList - does what you'd expect

.ToStringList - does what you'd expect

.Trim/Start/End - does what you'd expect
## Files
### FileUtils
ApplicationPath/ApplicationName/GetParentDirectory/RoamingUserApplicationPath - does what it says on the tin

ReadFile/WriteFile - wrappers around File.Read/WriteText that simplify it a little more.
### StreamUtils
StringStream - turns a string into a memory stream.
## Threading
### ScopedNestableThreadGlobalSingleton<T>
Provides scoped, nestable, thread global values.

*Scoped* because any call to get the value (via a static) that occurs somewhere INSIDE the using scope will get that value.

*Nestable* because if you open another scope (through an interior/nested using) then THAT becomes the value anything inside of THAT scope will receive whereas once outside of THAT using scope the value for the PREVIOUS scope is the static value.

*Thread* because a ThreadLocal<T> is used as the backing store and so each scopes within different threads are just for that thread.

*Global* because it's sort of acting like a global variable!

*Singleton* because you access the value via a static property on the class.

Another way of thinking about this class is that it is a smuggler. It can smuggle values (including numbers of budgies) way down into call heirarchies without you needing to retrofit paramters to pass to each call. You know the way you can use class variables for temporary working purposes without them being true properties/attributes of that class (from the design rather than language persepective here)? Well, a ScopedNestableThreadGlobalSingleton<T> is really the same thing, but for a thread. Kinda!

Usage:
```C#
   public class SUT : NestableThreadGlobalSingleton<string>
   {
      public SUT(string value) : base(value) {}
   }
```
Wrap any significant "outer code" with
```C#
   using (new SUT("value"))
   {
      YOUR CODE 
   }
```
Then anywhere, even deep, within YOUR CODE you can get the current nested, threaded global value via
```C#
   ...
   SUT.CurrentValue()
   ...
```
all thread safe and properly scoped!