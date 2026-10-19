using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Networking;

namespace PixiEditor.TokenBeam;

public class TokenBeamSession
{
    public bool Connected => _connection is { IsAlive: true };
    public string BeamToken { get; set; }

    public event Action OnConnected;
    public event Action OnConnectionLost;

    private WebSocketConnection? _connection;

    public TokenBeamSession(string token, string url)
    {
        BeamToken = token;
        var request = new WebSocketRequest
        {
            Url = url,
        };

        var api = PixiEditorExtension.Api;

        api.Logger.Log("Connecting to tokenbeam.");

        api.NetworkProvider.ConnectWebSocket(request).Completed += connection =>
        {
            api.Logger.Log("Established connection to tokenbeam.");
            _connection = connection;
            _connection.OnConnectionClosed += () =>
            {
                OnConnectionLost?.Invoke();
            };

            if (connection == null)
            {
                api.Logger.Log("Failed to connect to Token Beam.");
                return;
            }

            api.Logger.Log($"Connected to tokenbeam: {BeamToken}");

            OnConnected?.Invoke();

            connection.OnMessageReceived += (message) =>
            {
                if (message.MessageType != Extensions.CommonApi.Network.WebSocketMessageType.Text)
                    return;

                var json = Encoding.UTF8.GetString(message.Body);

                using var document = JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty("type", out var type))
                    return;

                if (type.GetString() != "sync")
                    return;

                if (!document.RootElement.TryGetProperty("payload", out var payload))
                    return;

                var tokenPayload = JsonSerializer.Deserialize(
                    payload.GetRawText(),
                   TokenBeamJsonContext.Default.TokenBeamPayload);

                if (tokenPayload == null)
                    return;

                foreach (var collection in tokenPayload.Collections)
                {
                    List<PaletteColor> colors = new List<PaletteColor>();
                    foreach (var mode in collection.Modes)
                    {
                        foreach (var token in mode.Tokens)
                        {
                            colors.Add(PaletteColor.Parse(token.Value));
                        }
                    }

                    if (api.Documents.ActiveDocument != null)
                    {
                        api.Documents.ActiveDocument.Palette = colors.ToArray();
                    }
                }
            };

            var pair = new TokenBeamPairRequest
            {
                Type = "pair",
                ClientType = "sender",
                SessionToken = token
            };

            var json = JsonSerializer.Serialize(
                pair,
                TokenBeamJsonContext.Default.TokenBeamPairRequest);

            connection.SendMessage(new WebSocketMessage
            {
                Body = Encoding.UTF8.GetBytes(json),
                MessageType = Extensions.CommonApi.Network.WebSocketMessageType.Text
            });
        };
    }

    public void Disconnect()
    {
        _connection?.Close();
        _connection = null;
        OnConnectionLost?.Invoke();
    }
}

public sealed class TokenBeamPairRequest
{
    [JsonPropertyName("type")] public string Type { get; set; } = "pair";
    [JsonPropertyName("clientType")] public string ClientType { get; set; } = "sender";
    [JsonPropertyName("sessionToken")] public string SessionToken { get; set; } = "";
}

public sealed class TokenBeamPayload
{
    public List<TokenBeamCollection> Collections { get; set; } = [];
}

public sealed class TokenBeamCollection
{
    public string Name { get; set; } = "";
    public List<TokenBeamMode> Modes { get; set; } = new List<TokenBeamMode>();
}

public sealed class TokenBeamMode
{
    public string Name { get; set; } = "";
    public List<TokenBeamToken> Tokens { get; set; } = [];
}

public sealed class TokenBeamToken
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(TokenBeamPairRequest))]
[JsonSerializable(typeof(TokenBeamPayload))]
public partial class TokenBeamJsonContext : JsonSerializerContext
{
}