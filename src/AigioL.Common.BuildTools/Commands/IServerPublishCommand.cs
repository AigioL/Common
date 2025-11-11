using AigioL.Common.BuildTools.Commands.Abstractions;
using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using static AigioL.Common.BuildTools.ProgramHelper;

namespace AigioL.Common.BuildTools.Commands;

/// <summary>
/// 服务端微服务容器发布命令
/// </summary>
public partial interface IServerPublishCommand : ICommand
{
    /// <summary>
    /// 命令名
    /// </summary>
    const string CommandName = "spub";

    /// <inheritdoc cref="ICommand.GetCommand"/>
    static Command ICommand.GetCommand()
    {
        var no_err = new Option<bool>("--no_err")
        {
            Description = "进程退出码是否不为错误值",
        };
        var push_name = new Option<string>("--push_name")
        {
            Description = "推送的名称",
        };
        var input = new Option<string>("--input", "--n", "--i", "--proj")
        {
            Description = "发布的项目组，输入 a、all 发布全部，使用逗号分隔符",
        };
        var push_domain = new Option<string>("--push_domain")
        {
            Description = "推送的域名",
        };
        var push_only = new Option<bool>("--push_only")
        {
            Description = "是否仅推送",
        };
        var tag_ver = new Option<string>("--tag_ver", "--ver")
        {
            Description = "容器镜像的版本号",
        };
        var config = new Option<string>("--config", "--c")
        {
            Description = "Debug 或 Release(r、re、rel)",
        };
        var command = new Command(CommandName, "服务端发布命令")
        {
            no_err, push_name, input, push_domain, push_only, tag_ver, config,
        };
        command.SetAction(parseResult => Handler(
            parseResult.GetValue(no_err),
            parseResult.GetValue(push_name),
            parseResult.GetValue(input),
            parseResult.GetValue(push_domain),
            parseResult.GetValue(push_only),
            parseResult.GetValue(tag_ver),
            parseResult.GetValue(config)
        ));
        return command;
    }

    private static HashSet<string> errors = null!;

    private static readonly char[] separator = [',', '，', '\\', '、', '|'];

    static async Task<int> Handler(bool no_err, string? push_name, string? input, string? push_domain, bool push_only, string? tag_ver, string? config)
    {
        errors = new();
        int exitCode = 0;
        try
        {
            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromHours(2)); // 设置超时时间

            Console.CancelKeyPress += (_, _) =>
            {
                cts.Cancel();
            };

            await HandlerCore(push_name, input, push_domain, push_only, tag_ver, config, cts.Token);
        }
        catch (ExitApplicationException ex)
        {
            exitCode = ex.ExitCode;
        }
        finally
        {
            if (errors.Count != 0)
            {
                if (!no_err)
                {
                    exitCode = unchecked((int)ExitCode.Exception);
                }
            }

            if (exitCode == 0)
            {
                Console.WriteLine("🆗");
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("❌");
                Console.WriteLine("HasError");
                foreach (var err in errors)
                {
                    Console.Error.WriteLine(err);
                }
            }
        }
        return exitCode;
    }

    static string GetConfig(string? config)
    {
        const string defConfig = "Release";
        return config?.ToLowerInvariant() switch
        {
            "d" or "de" or "deb" or "debug" => "Debug",
            "r" or "re" or "rel" or "release" => "Release",
            _ => defConfig,
        };
    }

    private static async Task HandlerCore(string? push_name, string? input, string? push_domain, bool push_only, string? tag_ver, string? config, CancellationToken cancellationToken)
    {
        var thisType = typeof(IServerPublishCommand);
        if (string.IsNullOrWhiteSpace(push_domain))
        {
            push_domain = thisType.GetField("DefaultPushDomain", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)?.ToString();
        }
        if (string.IsNullOrWhiteSpace(push_name))
        {
            push_name = thisType.GetField("DefaultPushName", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)?.ToString();
        }
        string[] ignoreProjects = [];
        if (thisType.GetField("IgnoreProjects", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) is string[] ignoreProjects2)
        {
            ignoreProjects = ignoreProjects2;
        }

        config = GetConfig(config);
        var projPath = ROOT_ProjPath;
        var projects = GetServerPublishProjects()
            .Where(x => !ignoreProjects.Contains(x.ProjectName))
            .ToArray();
        if (projects == null || projects.Length == 0)
        {
            Console.WriteLine("错误：未配置发布项目");
            return;
        }

        if (!string.IsNullOrWhiteSpace(input))
        {
            var array = input?.Split(separator, StringSplitOptions.RemoveEmptyEntries)?.Select(x => x.Trim())?.ToArray();
            if (array != null && array.Length != 0)
            {
                switch (array.FirstOrDefault()?.ToLowerInvariant())
                {
                    case "p":
                    case "push":
                        push_only = true;
                        break;
                }

                if (!array.Any(x => x.ToLowerInvariant() switch
                {
                    "a" or "all" => true,
                    _ => false,
                }))
                {
                    List<ServerPublishProject> list = new();
                    foreach (var item in array)
                    {
                        if (int.TryParse(item, out var index) && index >= 0 && index < projects.Length)
                        {
                            list.Add(projects[index]);
                            continue;
                        }
                        else
                        {
                            var eq_item = projects.FirstOrDefault(x =>
                            {
                                if (string.Equals(item, x.ProjectName, StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                                var dianSplit = x.ProjectName.Split('.', StringSplitOptions.RemoveEmptyEntries).ToArray();
                                if (dianSplit.Any(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
                                {
                                    return true;
                                }
                                if (string.Equals(item, x.DockerfileTag, StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                                return false;
                            });
                            if (eq_item != null)
                            {
                                list.Add(eq_item);
                            }
                        }
                    }
                    if (list.Count != 0)
                    {
                        projects = [.. list];
                    }
                }
            }
        }

        var proj_datas = projects.ToDictionary(static x => x, y => y.GetData(projPath));

        foreach (var item in proj_datas)
        {
            if (!push_only)
            {
                var proj = item.Key;
                (var csprojPath, var publishPath, var dockerfileTag, var dockerfilePath) = item.Value;
                try
                {
                    Directory.Delete(publishPath, true);
                }
                catch (DirectoryNotFoundException)
                {
                }

                bool useAlpineLinux = false;
                //if (proj.UseAlpineLinux.HasValue)
                //{
                //    useAlpineLinux = proj.UseAlpineLinux.Value;
                //}
                //else
                //{
                //    useAlpineLinux = false;
                //}

                #region dotnet publish
                if (false)
                {
                    ProcessStartInfo psi = new()
                    {
                        FileName = "dotnet",
                        WorkingDirectory = projPath,
                    };

                    psi.ArgumentList.Add("publish");

                    psi.ArgumentList.Add("-c");
                    psi.ArgumentList.Add(config);

                    psi.ArgumentList.Add(csprojPath);
                    psi.ArgumentList.Add("--nologo");
                    psi.ArgumentList.Add("-v");
                    psi.ArgumentList.Add("q");
                    psi.ArgumentList.Add("/property:WarningLevel=0");
                    psi.ArgumentList.Add("-p:AnalysisLevel=none");
                    psi.ArgumentList.Add("-p:GeneratePackageOnBuild=false");
                    psi.ArgumentList.Add("-p:DebugType=none");
                    psi.ArgumentList.Add("-p:DebugSymbols=false");
                    psi.ArgumentList.Add("-p:IsPackable=false");
                    psi.ArgumentList.Add("-p:GenerateDocumentationFile=false");

                    psi.ArgumentList.Add("-o");
                    psi.ArgumentList.Add(publishPath);

                    psi.ArgumentList.Add("-f");
                    psi.ArgumentList.Add($"net{Environment.Version.Major}.{Environment.Version.Minor}");

                    //psi.ArgumentList.Add($"-p:SelfContained={config.SelfContained.ToLowerString()}");

                    //if (!config.PublishSingleFile)
                    //{
                    //    psi.ArgumentList.Add("-p:UseAppHost=false");
                    //}

                    //psi.ArgumentList.Add($"-p:PublishSingleFile={config.PublishSingleFile.ToLowerString()}");

                    //psi.ArgumentList.Add($"-p:PublishReadyToRun={config.PublishReadyToRun.ToLowerString()}");

                    var runtimeIdentifier = "linux-x64";
                    if (useAlpineLinux)
                    {
                        switch (runtimeIdentifier)
                        {
                            case "linux-x64":
                                runtimeIdentifier = "linux-musl-x64";
                                break;
                            case "linux-arm64":
                            case "linux-bionic-arm64":
                                runtimeIdentifier = "linux-musl-arm64";
                                break;
                        }
                    }

                    psi.ArgumentList.Add($"-p:RuntimeIdentifier={runtimeIdentifier}");

                    //psi.ArgumentList.Add($"-p:AssemblyName={AssemblyName_ENTRYPOINT}");

#if DEBUG
                    var publish_args = psi.FileName + ' ' + string.Join(' ', psi.ArgumentList);
                    Debug.WriteLine(publish_args);
#endif

                    var process = Process.Start(psi);
                    if (process == null)
                        return;

                    Console.WriteLine($"dotnet publish start({config})：{proj.ProjectName}");

                    bool isKillProcess = false;
                    void KillProcess()
                    {
                        if (isKillProcess)
                            return;

                        isKillProcess = true;

                        try
                        {
                            process.Kill(true); // 避免进程残留未结束
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                    try
                    {
                        cancellationToken.Register(KillProcess);

                        await process.WaitForExitAsync(cancellationToken);

                        int exitCode = -1;
                        try
                        {
                            exitCode = process.ExitCode;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        if (exitCode == 0)
                        {
                            Console.WriteLine($"dotnet publish end({config})：{proj.ProjectName}");
                        }
                        else
                        {
                            var err = $"dotnet publish end({config})：{proj.ProjectName}, exitCode:{exitCode}";
                            Console.WriteLine(err);
                            errors.Add(err);
                            throw new ExitApplicationException(exitCode, err);
                        }
                    }
                    finally
                    {
                        KillProcess();
                    }
                }
                #endregion

                #region docker build
                {
                    ProcessStartInfo psi = new()
                    {
                        FileName = "docker",
                        WorkingDirectory = projPath,
                        Arguments = $"build -t {dockerfileTag} -f {dockerfilePath} .",
                    };

                    var process = Process.Start(psi);
                    if (process == null)
                        return;

                    Console.WriteLine($"docker build start({config})：{proj.ProjectName}");

                    bool isKillProcess = false;
                    void KillProcess()
                    {
                        if (isKillProcess)
                            return;

                        isKillProcess = true;

                        try
                        {
                            process.Kill(true); // 避免进程残留未结束
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                    try
                    {
                        cancellationToken.Register(KillProcess);

                        await process.WaitForExitAsync(cancellationToken);

                        int exitCode = -1;
                        try
                        {
                            exitCode = process.ExitCode;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        if (exitCode == 0)
                        {
                            Console.WriteLine($"docker build end({config})：{proj.ProjectName}");
                        }
                        else
                        {
                            var err = $"docker build end({config})：{proj.ProjectName}, exitCode:{exitCode}";
                            Console.WriteLine(err);
                            errors.Add(err);
                            throw new ExitApplicationException(exitCode, err);
                        }
                    }
                    finally
                    {
                        KillProcess();
                    }
                }

                #endregion
            }
        }

        #region docker tag/push

        // 并行化推送镜像

        if (!string.IsNullOrWhiteSpace(push_name))
        {
            string[]? domains = null;
            if (!string.IsNullOrWhiteSpace(push_domain))
            {
                var push_domain_split = push_domain.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                if (push_domain_split.Length != 0)
                {
                    domains = push_domain_split;
                }
            }

            if (domains != null && domains.Length != 0)
            {
                if (string.IsNullOrWhiteSpace(tag_ver))
                {
                    tag_ver = "latest";
                }

                var tag_with_push_array = domains.SelectMany(domain => proj_datas.ToArray().Select(y => KeyValuePair.Create(y.Key, (y.Value, domain)))).ToArray();

                await ForEachAsync(
                      tag_with_push_array,
                      cancellationToken,
                      async (item, cancellationToken) =>
                      {
                          var proj = item.Key;
                          (_, _, var dockerfileTag, _) = item.Value.Value;
                          var domain = item.Value.domain;
                          var targetTag = $"{domain}/{push_name}/{dockerfileTag}";

                          // 推送指定版本
                          await RunDockerCommandAsync($"tag {dockerfileTag} {targetTag}:{tag_ver}", cancellationToken);
                          await RunDockerCommandAsync($"push {targetTag}:{tag_ver}", cancellationToken);

                          // 如果不是 latest，额外推送 latest
                          if (tag_ver != "latest")
                          {
                              await RunDockerCommandAsync($"tag {dockerfileTag} {targetTag}:latest", cancellationToken);
                              await RunDockerCommandAsync($"push {targetTag}:latest", cancellationToken);
                          }
                      });

                // 提取的公共方法
                async Task RunDockerCommandAsync(string arguments, CancellationToken cancellationToken)
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = arguments,
                    });
                    ArgumentNullException.ThrowIfNull(process, "docker");

                    try
                    {
                        await process.WaitForExitAsync(cancellationToken);
                    }
                    finally
                    {
                        process.Kill(true);
                    }
                }
            }
        }

        #endregion
    }

    static IEnumerable<ServerPublishProject> GetServerPublishProjects()
    {
        var srcPath = Path.Combine(ROOT_ProjPath, "src");
        var directories = Directory.EnumerateDirectories(srcPath);
        foreach (var it in directories)
        {
            var filePathDockerfile = Path.Combine(it, "Dockerfile");
            if (!File.Exists(filePathDockerfile))
            {
                continue;
            }
            var filePathCsproj = Directory.EnumerateFiles(it, "*.csproj").FirstOrDefault();
            if (filePathCsproj == null)
            {
                continue;
            }
            string? dockerfileTag = null;
            var lines = File.ReadLines(filePathCsproj).GetEnumerator();
            while (lines.MoveNext())
            {
                var line = lines.Current.AsSpan().Trim();
                const string dockerfileTagStart = "<DockerfileTag>";
                if (line.Length > 0 && line.StartsWith(dockerfileTagStart, StringComparison.OrdinalIgnoreCase))
                {
                    line = line[dockerfileTagStart.Length..];
                    const string dockerfileTagEnd = "</DockerfileTag>";
                    if (line.EndsWith(dockerfileTagEnd, StringComparison.OrdinalIgnoreCase))
                    {
                        dockerfileTag = line[..^dockerfileTagEnd.Length].Trim().ToString();
                        break;
                    }
                }
            }
            var fileNameWithoutExtCsproj = Path.GetFileNameWithoutExtension(filePathCsproj);
            yield return new ServerPublishProject()
            {
                DirName = new DirectoryInfo(it).Name,
                ProjectName = fileNameWithoutExtCsproj,
                DockerfileTag = dockerfileTag!,
            };
        }
    }

    private static async Task ForEachAsync<TSource>(IEnumerable<TSource> source, CancellationToken cancellationToken, Func<TSource, CancellationToken, ValueTask> body)
    {
        foreach (var item in source)
        {
            await body(item, cancellationToken);
        }
    }
}

public sealed record class ServerPublishProject
{
    public required string ProjectName { get; set; }

    public required string DirName { get; set; }

    public required string DockerfileTag { get; set; }

    internal (string csprojPath, string publishPath, string dockerfileTag, string dockerfilePath) GetData(string projPath)
    {
        var projName = ProjectName;
        var projDirName = DirName ?? projName;
        var csprojPath = Path.Combine(projPath, "src", projDirName, $"{projName}.csproj");
        var publishPath = Path.Combine(projPath, "src", "artifacts", "publish", projName, "output");
        var dockerfileTag = DockerfileTag;
        var dockerfilePath = Path.Combine(projPath, "src", projDirName, "Dockerfile");
        return (csprojPath, publishPath, dockerfileTag, dockerfilePath);
    }
}