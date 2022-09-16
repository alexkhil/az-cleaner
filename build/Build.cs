using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Utilities.Collections;
using Serilog;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.SonarScanner.SonarScannerTasks;

[GitHubActions(
    name: "ci",
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = new[] { nameof(Tests) },
    OnPushBranches = new[] { "main" },
    OnPullRequestBranches = new[] { "main" },
    OnWorkflowDispatchOptionalInputs = new[] { "reason" }
)]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Tests);

    [Secret]
    readonly string SonarToken = "3f8bd10f7d983f205dcf1209bb673cb6e9cea5a4";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [GitRepository]
    readonly GitRepository GitRepository;

    [GitVersion]
    readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath TestsDirectory => RootDirectory / "tests";

    AbsolutePath TestResultsDirectory => RootDirectory / "TestResults";

    protected override void OnBuildInitialized()
    {
        Log.Information("Configuration {Configuration}", Configuration);
        Log.Information("GitVersion.SemVer {SemVer}", GitVersion.SemVer);
    }

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("*/bin", "*/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("*/bin", "*/obj").ForEach(DeleteDirectory);

            EnsureCleanDirectory(TestResultsDirectory);
        });

    Target SonarBegin => _ => _
        .OnlyWhenStatic(() => !string.IsNullOrWhiteSpace(SonarToken))
        .After(Clean)
        .DependentFor(Restore)
        .Executes(() =>
        {
            SonarScannerBegin(_ => _
                .SetOrganization("alexkhil")
                .SetProjectKey("alexkhil_az-cleaner")
                .SetLogin(SonarToken)
                .SetFramework("net5.0")
                .SetOpenCoverPaths(TestResultsDirectory / "**/*.xml")
                .SetServer("https://sonarcloud.io"));
        });

    Target SonarEnd => _ => _
        .OnlyWhenDynamic(() => SucceededTargets.Contains(SonarBegin))
        .TriggeredBy(Tests)
        .Executes(() =>
        {
            SonarScannerEnd(_ => _
                .SetLogin(SonarToken)
                .SetFramework("net5.0"));
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoLogo()
                .EnableNoRestore());
        });

    Target Tests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .EnableNoBuild()
                .EnableNoRestore()
                .SetConfiguration(Configuration)
                .SetDataCollector("XPlat Code Coverage")
                .SetResultsDirectory(TestResultsDirectory)
                .SetRunSetting(
                    "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format",
                    nameof(CoverletOutputFormat.opencover))
            );
        });
}
