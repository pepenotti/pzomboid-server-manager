using CliWrap;
using System.Text;

namespace PZomboid.ServerManager.Core
{
    public class LinuxCommandService
    {
        private readonly string serverBashDirectory;
        private readonly string serverBashFile;
        private readonly string[] serverBashArgs;

        private readonly string serverRunFilePath;

        private const string SCREEN_CMD = "screen";
        private const string SCREEN_SOCKNAME = "PZServer";
        private const int SCREEN_EXEC_ARG_COMMAND_POSITION = 5;
        private readonly string[] screenExecArgs = new[]
        {
            "screen", "-S", SCREEN_SOCKNAME, "-X", "stuff", "'{COMMAND}'", "`echo -ne '\\015'`"  //position 5 should be updated
        };

        private const string SERVER_COMMAND_SHUTDOWN = "quit";

        public LinuxCommandService(
            string serverBashDirectory,
            string serverBashFile,
            string[] serverBashArgs,
            string serverRunFilePath)
        {
            this.serverBashDirectory = serverBashDirectory;
            this.serverBashFile = serverBashFile;
            this.serverBashArgs = serverBashArgs;
            this.serverRunFilePath = serverRunFilePath;
        }

        public void ShutdownServerAsync()
        {
            var args = screenExecArgs.ToArray();
            args[SCREEN_EXEC_ARG_COMMAND_POSITION] = $"{SERVER_COMMAND_SHUTDOWN}";
            Console.WriteLine($"Shutdown: {SCREEN_CMD} | {string.Join(' ', args)}");

            Cli.Wrap(SCREEN_CMD)
                .WithArguments(args)
                .ExecuteAsync();
        }

        public void ExecuteServerCommand(string command)
        {
            var args = screenExecArgs.ToArray();
            args[SCREEN_EXEC_ARG_COMMAND_POSITION] = $"{command}";
            Console.WriteLine($"Exec: {SCREEN_CMD} | {string.Join(' ', args)}");

            Cli.Wrap(SCREEN_CMD)
                .WithArguments(args)
                .ExecuteAsync();
        }

        public string SeeConsoleLogs()
        {
            var filePath = this.serverRunFilePath.Last() == '/' ||
                           this.serverRunFilePath.Last() == '\\' ?
                            this.serverRunFilePath + "server-console.txt" : "/server-console.txt";
            Console.WriteLine($"Console Log: {filePath}");
            return File.ReadAllText(filePath);
        }

        public async Task StartServerAsync()
        {
            var cmd = $"{serverBashDirectory}{serverBashFile} {string.Join(' ', serverBashArgs)}";
            Console.WriteLine("Command: " + cmd);
            var args = screenExecArgs.ToArray();
            args[SCREEN_EXEC_ARG_COMMAND_POSITION] = $"{cmd}";

            await Cli.Wrap(SCREEN_CMD)
                .WithArguments(args)
                .ExecuteAsync();
        }

        public async Task InitAsync()
        {
            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            Console.WriteLine($"Init: {SCREEN_CMD} -dmS {SCREEN_SOCKNAME}");
            await Cli.Wrap(SCREEN_CMD)
                .WithArguments(new[] { "-dmS", SCREEN_SOCKNAME })
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            Console.WriteLine("Errors: " + stdErrBuffer.ToString());
            Console.WriteLine("Output: " + stdOutBuffer.ToString());
        }

    }
}