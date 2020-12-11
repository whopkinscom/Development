INSTALL AutoT4 Visual Studio Extension
--------------------------------------

In order for the AutoVersion.tt file to run on each build, you will probably need to install the AutoT4 Visual Studio extension.
This causes .tt files to be re-run on a build. This keeps things up to date!

The version will be set as;

MajorVersion.Year.MonthDay.HourMinutesTensOfSeconds

You should now only manually set the MajorVersion, which you'll find in CreateVersion.t4, the rest will get set automatically at build time.

Remember, you will also need to remove the AssemblyVersion and AssemblyFileVersion attributes from AssemblyInfo.cs as well!

READ THIS? NOW PLEASE DELETE THIS FILE!