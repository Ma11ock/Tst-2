using Godot;
using Quake.Net;
using System;
using Serilog;
using Common.Log;
using Serilog.Configuration;

namespace Quake;
public partial class Main : Node
{
    // TODO Move to configuration object.
    public string RemoteHost = "";
    public int RemotePort = 0;

    public bool HasSetupClient { get; private set; } = false;

    public void SetNextWorldState(Packet packet)
    {
    }

    private enum NetworkSetting
    {
        None,
        Client,
        Server,
    }

    public bool SetupClient(string remoteHost, int remotePort)
    {
        if (HasSetupClient) return false;

        Log.Information("Creating the client setup...");
        GetTree().Root.AddChildDeffered(new ClientManager(remoteHost, remotePort));
        Log.Information("Done creating the client setup.");
        return true;
    }

    private int GetNextArgInt(string[] args, ref int i)
    {
        string nextArg = GetNextArg(args, ref i);
        if (Int32.TryParse(nextArg, out int result))
        {
            return result;
        }
        Log.Error("Flag '{0}' takes an integer argument, but '{1}' is not a valid integer. Exiting.", args[i - 1], nextArg);
        GetTree().Quit();
        return 0; // Never reached.
    }

    private string GetNextArg(string[] args, ref int i)
    {
        try
        {
            return args[++i];
        }
        catch (IndexOutOfRangeException)
        {
            Log.Error("Flag '{0}' takes an argument but none was found. Exiting.", args[i - 1]);
            GetTree().Quit();
        }
        return ""; // Never reached.
    }

    private void ParseCmdArgs()
    {
        GD.Print("Initializing game...");

        string[] args = OS.GetCmdlineArgs();


        string host = NetworkManager.DEFAULT_IP;
        int port = NetworkManager.DEFAULT_PORT;
        NetworkSetting setting = NetworkSetting.None;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            Log.Information(arg);
            switch (arg)
            {
                case "--server":
                    setting = NetworkSetting.Server;
                    break;
                case "--client":
                    setting = NetworkSetting.Client;
                    break;
                case "--port":
                    port = GetNextArgInt(args, ref i);
                    break;
                case "--host":
                    host = GetNextArg(args, ref i);
                    break;
                case "--headless":
                    // Godot engine arguments that we don't care about.
                    break;
                default:
                    if(arg.EndsWith(".tscn"))
                    {
                        // Starting scene.
                        break;
                    }
                    GD.PrintErr($"Warn: \"{arg}\" is not recognized as an argument (maybe an engine argument?).");
                    break;
            }
        }

        // If headless, we can only be a server.
        if (setting == NetworkSetting.Client && (OS.HasFeature("dedicated_server") || DisplayServer.GetName() == "headless"))
        {
            GD.PrintErr("Bad configuration: game is in headless mode but is configured as a client. Exiting...");
            GetTree().Quit();
        }

        RemotePort = port;
        RemoteHost = host;

        switch (setting)
        {
            case NetworkSetting.None:
                // Launch a server process and wait.
                try
                {
                    Log.Logger = new LoggerConfiguration()
                        .WriteTo.File("logs/log_client-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 1 << 22)
                        .WriteTo.Godot()
                        .CreateLogger();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Could not instantiate logger: \"{ex.Message}\"");
                }

                LocalServerManager localServerManager = new LocalServerManager(host, port);
                localServerManager.ServerIsReady += OnServerIsReady; 
                this.AddChildDeffered(localServerManager);
                break;
            case NetworkSetting.Server:
                // Create logger.
                try
                {
                    Log.Logger = new LoggerConfiguration()
                        .WriteTo.File("logs/log_server-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 1 << 22)
                        .WriteTo.Godot()
                        .CreateLogger();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Could not instantiate logger: \"{ex.Message}\"");
                }

                Log.Information("Launching as server on port {0}...", port);
                ServerManager serverManager = new ServerManager(port);
                GetTree().Root.AddChildDeffered(serverManager);
                break;
            case NetworkSetting.Client:
                try
                {
                    Log.Logger = new LoggerConfiguration()
                        .WriteTo.File("logs/log_client-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 1 << 22)
                        .WriteTo.Godot()
                        .CreateLogger();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Could not instantiate logger: \"{ex.Message}\"");
                }

                SetupClient(host, port);
                break;
        }
    }

    public override void _Ready()
    {
        base._Ready();

        // Set up logging
        try
        {
            ParseCmdArgs();
        }
        catch (Exception ex)
        {
            Log.Error("Error in parsing command line arguments: {0}. Exiting.", ex);
            GetTree().Quit();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    private void OnServerIsReady(object? sender, EventArgs e)
    {
        Log.Information("!!!!!!!!!!!!!");
        // Unsubscribe from the event.
        if(sender is LocalServerManager serverManager)
        {
            serverManager.ServerIsReady -= OnServerIsReady;
        }

        CallDeferred(nameof(SetupClient), RemoteHost, RemotePort);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        Log.CloseAndFlush();
    }

}
