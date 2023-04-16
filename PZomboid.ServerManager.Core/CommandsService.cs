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
        private const string SCREEN_COMMAND_PLACEHOLDER = "{COMMAND}";
        private readonly string screenExecArgs = $"-S {SCREEN_SOCKNAME} -X stuff '{SCREEN_COMMAND_PLACEHOLDER}'`echo -ne '\\015'`";

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
            var args = screenExecArgs.Replace(SCREEN_COMMAND_PLACEHOLDER, SERVER_COMMAND_SHUTDOWN);
            Console.WriteLine($"Shutdown: {SCREEN_CMD} | {args}");

            Cli.Wrap(SCREEN_CMD)
                .WithArguments(args)
                .ExecuteAsync();
        }

        public void ExecuteServerCommand(string command)
        {
            var args = screenExecArgs.Replace(SCREEN_COMMAND_PLACEHOLDER, command);
            Console.WriteLine($"Exec: {SCREEN_CMD} | Args: {args}");

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
            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var cmd = $"{serverBashDirectory}{serverBashFile} {string.Join(' ', serverBashArgs)}";
            Console.WriteLine("Command: " + cmd);
            var args = screenExecArgs.Replace(SCREEN_COMMAND_PLACEHOLDER, cmd);
            Console.WriteLine("Args: " + args);

            await Cli.Wrap($"{serverBashDirectory}{serverBashFile}")
                .WithArguments(args)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            Console.WriteLine("Errors: " + stdErrBuffer.ToString());
            Console.WriteLine("Output: " + stdOutBuffer.ToString());
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