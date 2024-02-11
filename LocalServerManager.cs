using System;
using Godot;
using System.Diagnostics;
using Serilog;
using System.IO.Pipes;
using System.Text;
using System.IO;

namespace Quake.Net;
public partial class LocalServerManager : Node
{
    private Process _serverProcess;

    private string _host;
    private int _port;

    private bool _waitingForServerInit = true;

    public event EventHandler ServerIsReady;

#pragma warning disable CS8618 // Shut the compiler up about _serverProcess not being initialized because _Ready is the real initializer.
    public LocalServerManager(string host, int port)
#pragma warning restore CS8618
    {
        _host = host;
        _port = port;

        // Set up connection to the server.
    }

    public override void _Ready()
    {
        base._Ready();

        // Delete any init file that might already be at /tmp/Tst_Port.txt
        string serverInitPath = ServerManager.GetInitFilePath(_port);
        try
        {
            File.Delete(serverInitPath);
        }
        catch(IOException ex)
        {
            Log.Error("Could not delete '{0}' because there is an open reference to it. Is there already a server on port {1}? {2}. Exiting", serverInitPath, _port, ex);
            GetTree().Quit();
        }
        catch(Exception ex)
        {
            if(File.Exists(serverInitPath))
            {
                Log.Error("Could not delete '{0}': {1}. Will try to conintue anyway", serverInitPath, _port, ex);
            }
        }

#if DEBUG
        string exe = OS.GetExecutablePath();
#else
        string exe = Assembly.GetExecutingAssembly().Location;
#endif
        Log.Information("Launching server process with executable '{0}' listening on {1}:{2}", exe, _host, _port);
        try
        {

            _serverProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = $"res://TestStairs.tscn --server --port {_port} --headless",
                UseShellExecute = false,

                RedirectStandardOutput = false
            })!;
        }
        catch (Exception ex)
        {
            Log.Error("Could not start server process: {0}. Exiting.", ex);
            GetTree().Quit();
        }

        _serverProcess.Exited += ServerHasExited;

        // Might not be necessary?
        if (_serverProcess.HasExited)
        {
            Log.Error("Server was killed prematurely. Exiting.");
            GetTree().Quit();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        try
        {
            // Kill the server.
            // Maybe idk use something else to kill the server? This is not a graceful shutdown.
            Log.Information("Killing the server process id {0}...", _serverProcess.Id);
            _serverProcess.Exited -= ServerHasExited;
            if (!_serverProcess.HasExited)
            {
                _serverProcess.CloseMainWindow();
                _serverProcess.Kill();
            }
            _serverProcess.Close();
            Log.Information("Done killing the server.");
        }
        catch (Exception ex)
        {
            Log.Error("Could not kill the server: {0}", ex);
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if(_waitingForServerInit)
        {
            string serverInitFile = ServerManager.GetInitFilePath(_port);
            if(File.Exists(serverInitFile))
            {
                Log.Information("Found server init file at '{0}'", serverInitFile);
                _waitingForServerInit = false;
                ServerIsReady?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // TODO timeout.
            }
        }
    }

	private void OnDisconnectedToServer(object sender, EventArgs e) 
	{
		Log.Information("Disconnected from the server.");
	}

    private void ServerHasExited(object? sender, EventArgs e)
    {
        Log.Error(
            "Server has exited prematurely. Server information:\n" +
"            Process ID   : {0}\n" +            
"            Exit time    : {1}\n" +
"            Exit code    : {2}\n" +
"            Elapsed time : {3}\n" +
"   Killing the game.", 
            _serverProcess.Id, _serverProcess.ExitTime, _serverProcess.ExitCode, _serverProcess.ExitTime - _serverProcess.StartTime);
        GetTree().Quit();
    }

}

