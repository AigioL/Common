using AigioL.Common.BuildTools.Commands.Abstractions;
using System.CommandLine;

namespace AigioL.Common.BuildTools.Commands;

/// <summary>
/// 模板项目重命名命令
/// </summary>
public partial interface IRenameCommand : ICommand
{
    /// <summary>
    /// 命令名
    /// </summary>
    const string CommandName = "rename";

    /// <inheritdoc cref="ICommand.GetCommand"/>
    static Command ICommand.GetCommand()
    {
        var projName = new Option<string>("--projName", "--n")
        {
            Description = "项目名称",
        };
        var projNameLower = new Option<string>("--projNameLower", "--n")
        {
            Description = "项目名称全小写",
        };
        var notTrimServer = new Option<bool>("--notTrimServer")
        {
            Description = "是否【不】裁剪项目名称末尾中的 .Server",
        };
        var webProtStart = new Option<int>("--webProtStart")
        {
            Description = "Web 端口号起始值",
        };
        var command = new Command(CommandName, "模板项目重命名命令")
        {
            projName, projNameLower, notTrimServer, webProtStart,
        };
        command.SetAction(parseResult => Handler(
            parseResult.GetValue(projName),
            parseResult.GetValue(projNameLower),
            parseResult.GetValue(notTrimServer),
            parseResult.GetValue(webProtStart)
        ));
        return command;
    }

    static readonly string[] handlerFileExs =
    [
        ".md",
        ".sln",
        ".slnx",
        ".cs",
        ".csproj",
        ".props",
        ".targets",
        ".json",
        ".yaml",
        "Dockerfile",
        ".pubxml",
        ".shproj",
        ".projitems",
    ];

    static readonly string[] handlerIgnorePaths =
    [
        ".vs",
        ".git",
        "artifacts",
        "ref",
    ];

    static async Task Handler(string? projName, string? projNameLower, bool notTrimServer, int webProtStart)
    {
        Console.WriteLine($"IsPrivilegedProcess: {Environment.IsPrivilegedProcess}");

        if (webProtStart <= 30)
        {
            webProtStart = 31;
        }
        var webProtStartString = webProtStart.ToString();
        if (string.IsNullOrWhiteSpace(projName))
        {
            Console.WriteLine("请输入项目名称！");
            throw new ArgumentNullException(nameof(projName));
        }
        var projNameTrimServer = notTrimServer ?
            projName :
            projName.Replace(".Server", "", StringComparison.InvariantCultureIgnoreCase);
        if (string.IsNullOrWhiteSpace(projNameLower))
        {
            projNameLower = projNameTrimServer.ToLowerInvariant();
        }

        var tempSlnPath = Path.Combine(ROOT_ProjPath, "AigioLTemplate.Server.slnx");
        if (!File.Exists(tempSlnPath))
        {
            throw new FileNotFoundException("未找到模板解决方案文件！", tempSlnPath);
        }

        Enumerate(ROOT_ProjPath);

        void Enumerate(string dirPath)
        {
            if (handlerIgnorePaths.Any(x => dirPath.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var files = Directory.EnumerateFiles(dirPath);
            foreach (var f in files)
            {
                var itFilePath = f;
                if (handlerFileExs.Any(x => itFilePath.EndsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (itFilePath.Contains("AigioLTemplate.Server", StringComparison.InvariantCultureIgnoreCase))
                    {
                        itFilePath = itFilePath.Replace("AigioLTemplate.Server", projNameTrimServer, StringComparison.InvariantCultureIgnoreCase);
                        // 移动文件
                        File.Move(f, itFilePath);
                    }
                    var content = File.ReadAllText(itFilePath);
                    var content2 = content;
                    // 替换文件内容
                    content = content.Replace("AigioLTemplate.Server", projNameTrimServer, StringComparison.InvariantCultureIgnoreCase);
                    content = content.Replace("aigioltemplate", projNameLower, StringComparison.InvariantCultureIgnoreCase);
                    content = content.Replace("localhost:29", $"localhost:{webProtStartString}", StringComparison.InvariantCultureIgnoreCase);
                    if (content != content2)
                    {
                        Console.WriteLine($"处理文件：{itFilePath}");
                        File.WriteAllText(itFilePath, content);
                    }
                }
            }

            var dirs = Directory.EnumerateDirectories(dirPath);
            foreach (var d in dirs)
            {
                var itDirPath = d;
                if (itDirPath.Contains("AigioLTemplate.Server", StringComparison.InvariantCultureIgnoreCase))
                {
                    itDirPath = itDirPath.Replace("AigioLTemplate.Server", projNameTrimServer, StringComparison.InvariantCultureIgnoreCase);
                    // 移动文件夹
                    try
                    {
                        Directory.Move(d, itDirPath);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"移动文件夹出错：{d} => {itDirPath}");
                        throw;
                    }
                }
                Enumerate(itDirPath);
            }
        }
    }
}
