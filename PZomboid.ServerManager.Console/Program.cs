using PZomboid.ServerManager.Core;
using System.Drawing;

const string GREETING = "Welcome to Project Zomboid Server Management Console";
const string GREETING_LINE = "====================================================";

var linuxCommandService = new LinuxCommandService(
                                "/opt/pzserver/", 
                                "start-server.sh", 
                                new[] { "-servername", "PZServer", "-config", "/home/pzuser/Zomboid/Server/PZServer.init" }, 
                                "/home/pzuser/Zomboid/");

var exit = false;
Console.WriteLine(GREETING);
Console.WriteLine(GREETING_LINE, Color.Green);
Console.WriteLine();

while (!exit)
{
    ShowOptions();
    var command = Console.ReadLine() ?? string.Empty;
    ExecuteCommand(command);
}

void ShowOptions()
{
    Console.WriteLine("Please, select an option:");
    Console.WriteLine("=========================", Color.Green);
    Console.WriteLine("1 - Start server");
    Console.WriteLine("2 - See console logs");
    Console.WriteLine("3 - Execute commands");
    Console.WriteLine("4 - Shutdown Server");
    Console.WriteLine("e - Exit");
    Console.WriteLine();
}

void  ExecuteCommand(string command)
{
    switch (command.ToLowerInvariant())
    {
        case "1":
            linuxCommandService.StartServer();
            break;
        case "2":
            Console.WriteLine("Console output");
            Console.WriteLine("==============");
            Console.WriteLine();
            Console.WriteLine(linuxCommandService.SeeConsoleLogs());
            Console.WriteLine();
            break;
        case "3":
            Console.WriteLine("What command are you going to run?");
            var cmd = Console.ReadLine() ?? string.Empty;
            linuxCommandService.ExecuteServerCommand(cmd);
            break;
        case "4":
            linuxCommandService.ShutdownServerAsync();
            break;
        case "e":
            exit = true;
            break;
        default:
            Console.WriteLine($"{command} option is not valid.");
            break;
    }
}