using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quake.Api;
public class ApiServer : IDisposable
{
    private static JsonSerializerOptions Options = new JsonSerializerOptions()
    {
        Converters =
        {
            new JsonStringEnumConverter(),
        }
    };

    private HttpListener _httpListener;

    public string Host { get; private set; }
    public ushort Port { get; private set; }

    private static void SerializeTo(ApiResponse response, Stream stream) => JsonSerializer.Serialize(stream, response, typeof(ApiResponse), Options);


    private static ApiResponse Error404Response = new ApiResponse()
    {
        Status = ApiStatus.Fail,
    };


    public ApiServer(string host, ushort port)
    {
        Host = host;
        Port = port;
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://{host}:{port}/");
        _httpListener.Start();
    }

    public void StartProcessing() => _httpListener.BeginGetContext(OnContext, null);

    private void OnContext(IAsyncResult result)
    {
        HttpListenerContext ctx = _httpListener.EndGetContext(result);

        _httpListener.BeginGetContext(OnContext, null);

        HttpListenerRequest request = ctx.Request;
        switch (request.HttpMethod)
        {
            case "GET":
                HandleGet(ctx);
                break;
            case "POST":
                HandlePost(ctx);
                break;
            default:
                Log.Error("Got '{0}' requst", request.HttpMethod);
                break;
        }
    }

    private void HandleGet(HttpListenerContext ctx)
    {
        HttpListenerRequest request = ctx.Request;
        HttpListenerResponse response = ctx.Response;

        string path = request?.Url?.AbsolutePath ?? "";

        Log.Debug("Got GET request '{0}'", path);

        response.ContentEncoding = Encoding.UTF8;
        response.ContentType = MediaTypeNames.Application.Json;
        response.StatusCode = (int)HttpStatusCode.OK;
        response.StatusDescription = "OK";

        switch (path)
        {
            case "/":
                // Get general server status.
                SerializeTo(new ApiResponse() { Data = new Dictionary<string, object>() { { "Fps", 69.3 } } }, response.OutputStream);
                break;
            case "/IsReady":
                // Get true if ready, false if not.
                SerializeTo(new ApiResponse() { Data = new Dictionary<string, object>() { { "IsReady", true } } }, response.OutputStream);
                break;
            default:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.StatusDescription = "Not Found";
                SerializeTo(Error404Response, response.OutputStream);
                break;
        }

        response.Close();
    }

    private void HandlePost(HttpListenerContext ctx)
    {
        HttpListenerRequest request = ctx.Request;
        HttpListenerResponse response = ctx.Response;

        Log.Debug("Got POST request '{0}'", request.Url.AbsolutePath);
        if (!request.HasEntityBody)
        {
            return;
        }
    }

    public void Dispose()
    {
        _httpListener?.Close();
        _httpListener = null;
    }
}
