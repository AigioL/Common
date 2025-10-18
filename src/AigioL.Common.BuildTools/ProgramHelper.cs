using AigioL.Common.BuildTools.Commands.Abstractions;
using System.CommandLine;
using System.Reflection;
using System.Text;

namespace AigioL.Common.BuildTools;

public static partial class ProgramHelper
{
    public static async Task<int> M(
        string? projectName,
        string[] args)
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            projectName = typeof(ProgramHelper).Assembly.GetName().Name;
        }
        ArgumentNullException.ThrowIfNull(projectName);
        if (projectName.EndsWith(".BuildTools", StringComparison.OrdinalIgnoreCase))
        {
            projectName = projectName[..^".BuildTools".Length];
        }
        ProjectName = projectName;

        ConsoleWriteInfo(ProjectName);

        if (args == null || args.Length == 0)
        {
            int lastExitCode = unchecked((int)ExitCode.Ok);
            while (true)
            {
                Console.WriteLine("请输入命令参数：");
                var line = Console.ReadLine();
                if (string.Equals("exit", line, StringComparison.OrdinalIgnoreCase))
                {
                    return lastExitCode;
                }
                args = line?.Split(' ') ?? [];
                lastExitCode = await MainCoreAsync(args);
                Console.WriteLine($"---------- {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} ----------");
            }
        }
        else
        {
            var exitCode = await MainCoreAsync(args);
            return exitCode;
        }
    }

    static async Task<int> MainCoreAsync(IReadOnlyList<string> args)
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            string rootCommandDesc = $"{ProjectName} Build Tools";
            var rootCommand = new RootCommand(rootCommandDesc);
            // 根据命令行业务接口反射当前程序集查找所有实现循环添加
            var interfaceType = typeof(ICommand);
            var addMethod = interfaceType.GetMethod(nameof(ICommand.AddCommand),
                BindingFlags.Static | BindingFlags.Public);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
            var commands = interfaceType.Assembly.GetTypes().
                Where(x => x != interfaceType && x.IsInterface && interfaceType.IsAssignableFrom(x)).
                Select(x => addMethod!.MakeGenericMethod(x)).
                ToArray();
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            Array.ForEach(commands, m => m.Invoke(null, [rootCommand,]));
            var exitCode = await rootCommand.Parse(args).InvokeAsync();
            Console.Write("exitCode: ");
            Console.WriteLine(exitCode);
            return exitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("catch: ");
            Console.WriteLine(ex);
            return unchecked((int)ExitCode.Exception);
        }
    }

    /// <summary>
    /// 进程退出状态码
    /// </summary>
    public enum ExitCode
    {
        // https://tldp.org/LDP/abs/html/exitcodes.html

        Ok = 0,

        /// <summary>
        /// 出现 <see cref="System.Exception"/>
        /// </summary>
        Exception = 64,
    }
}
