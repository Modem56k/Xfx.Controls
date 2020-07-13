var target = Argument("target", "Default");
var local = BuildSystem.IsLocalBuild;
var appName = "Xfx.Controls";
var versionParam = Argument<string>("BuildVersion");
var versionParts = versionParam.Split('.');

var version = string.Format("{0}.{1}.{2}", versionParts[0],versionParts[1],versionParts[2]);
var semVersion = local ? version : (version + string.Concat("-build-", versionParts[3]));
var configuration = Argument("configuration", "Release");
var primaryAuthor = "Chase Florell";

var touchDir = MakeAbsolute(Directory("./build-artifacts/output/touch"));
var droidDir = MakeAbsolute(Directory("./build-artifacts/output/droid"));
var coreDir  = MakeAbsolute(Directory("./build-artifacts/output/core"));
var nugetOutDir  = MakeAbsolute(Directory("./nuget"));
Setup(context =>
{
    var binsToClean = GetDirectories("./src/**/bin/");
	var artifactsToClean = new []{
        touchDir.ToString(), 
        droidDir.ToString(), 
        coreDir.ToString(),
        nugetOutDir.ToString()
	};
	CleanDirectories(binsToClean);
	CleanDirectories(artifactsToClean);

    //Executed BEFORE the first task.
    Information("Building version {0} of {1}.", semVersion, appName);
});

Task("Default")
  .IsDependentOn("Package Library");

Task("Build Droid")
  .IsDependentOn("Patch Assembly Info")
  .Does(() =>
{
  MSBuild("./src/Xfx.Controls.Droid/Xfx.Controls.Droid.csproj", new MSBuildSettings
      {
	      ToolVersion = MSBuildToolVersion.VS2019
	  }
      .WithProperty("OutDir", droidDir.ToString())
      .SetConfiguration(configuration));
});

Task("Build Touch")
  .IsDependentOn("Patch Assembly Info")
  .Does(() =>
{
  MSBuild("./src/Xfx.Controls.iOS/Xfx.Controls.iOS.csproj", new MSBuildSettings
      {
	      ToolVersion = MSBuildToolVersion.VS2019,
		  MSBuildPlatform = (Cake.Common.Tools.MSBuild.MSBuildPlatform)1
	  }
      .WithProperty("OutDir", touchDir.ToString())
      .SetConfiguration(configuration));
});

Task("Build Core")
  .IsDependentOn("Patch Assembly Info")
  .Does(() =>
{
  MSBuild("./src/Xfx.Controls/Xfx.Controls.csproj", new MSBuildSettings
      {
	      ToolVersion = MSBuildToolVersion.VS2019
	  }
      .WithProperty("OutDir", coreDir.ToString())
      .SetConfiguration(configuration));
});

Task("Patch Assembly Info")
    .IsDependentOn("Nuget Restore")
    .Does(() =>
{
    var file = "./src/SolutionInfo.cs";

    CreateAssemblyInfo(file, new AssemblyInfoSettings
    {
        Product = appName,
        Version = version,
        FileVersion = version,
        InformationalVersion = semVersion,
        Copyright = "Copyright (c) 2015 - " + DateTime.Now.Year.ToString() + " " + primaryAuthor
    });
});

Task("Nuget Restore")
    .Does(() => {
    NuGetRestore("./Xfx.Controls.sln", new NuGetRestoreSettings { NoCache = true });
});



Task("Package Library")
  .IsDependentOn("Build Droid")
  .IsDependentOn("Build Touch")
  .IsDependentOn("Build Core")
  .Does(() => {
    var nuGetPackSettings   = new NuGetPackSettings {
                                    Id                      = appName,
                                    Version                 = version,
                                    Title                   = "Xamarin Forms Extended Controls",
                                    Authors                 = new[] {primaryAuthor},
                                    LicenseUrl              = new Uri("https://raw.githubusercontent.com/XamFormsExtended/Xfx.Controls/master/LICENSE.md"),
                                    Description             = "Xamarin Forms Extended Controls. Provides extended controls with a 'Material Design' flare.",
                                    ProjectUrl              = new Uri("https://github.com/XamFormsExtended/Xfx.Controls"),
                                    Files                   = new [] {
                                                                        new NuSpecContent {Source = coreDir.ToString() + "/Xfx.Controls.dll", Target = "lib/netcore45"},
                                                                        new NuSpecContent {Source = coreDir.ToString() + "/Xfx.Controls.dll", Target = "lib/netstandard1.3"},
                                                                        new NuSpecContent {Source = coreDir.ToString() + "/Xfx.Controls.dll", Target = "lib/portable-net45+win8+wpa81+wp8"},

                                                                        new NuSpecContent {Source = droidDir.ToString() + "/Xfx.Controls.Droid.dll", Target = "lib/MonoAndroid"},
                                                                        new NuSpecContent {Source = droidDir.ToString() + "/Xfx.Controls.dll", Target = "lib/MonoAndroid"},

                                                                        new NuSpecContent {Source = touchDir.ToString() + "/Xfx.Controls.iOS.dll", Target = "lib/Xamarin.iOS10"},
                                                                        new NuSpecContent {Source = touchDir.ToString() + "/Xfx.Controls.dll", Target = "lib/Xamarin.iOS10"},
                                                                      },
                                    Dependencies            = new [] { 
                                                                        new NuSpecDependency { Id = "Xamarin.Forms", Version  = "2.0" }
                                                                      },
                                    BasePath                = "./src",
                                    NoPackageAnalysis       = true,
                                    OutputDirectory         = nugetOutDir
                                };

    NuGetPack(nuGetPackSettings);
  });

RunTarget(target);