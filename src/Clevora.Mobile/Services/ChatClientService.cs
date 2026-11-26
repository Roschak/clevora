using Clevora.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Clevora.Mobile.Services;

/// <summary>
/// Manages the SignalR connection to the Clevora chat hub.
/// The server URL is configurable via appsettings (key: ServerUrl).
/// </summary>
public class ChatClientService
{
    private HubConnection? _hub;
    private string? _token;

    public event Action<MessageDto>? OnMessageReceived;
    public event Action<string, int>? OnUserTyping;
    public event Action<string, int>? OnUserStoppedTyping;
    public event Action<string>? OnUserOnline;
    public event Action<string, DateTime>? OnUserOffline;
    public event Action? OnReconnected;

    public HubConnectionState State => _hub?.State ?? HubConnectionState.Disconnected;

    public void Configure(string serverUrl, string token)
    {
        _token = token;
        _hub = new HubConnectionBuilder()
            .WithUrl($"{serverUrl.TrimEnd('/')}/hubs/chat", opts =>
            {
                opts.AccessTokenProvider = () => Task.FromResult<string?>(_token);
            })
            .WithAutomaticReconnect()
            .Build();

        _hub.Reconnected += _ =>
        {
            OnReconnected?.Invoke();
            return Task.CompletedTask;
        };

        _hub.On<MessageDto>("ReceiveMessage", dto =>
        {
            if (dto.ConversationId == _activeConversationId)
                OnMessageReceived?.Invoke(dto);
        });

        _hub.On<string, int>("UserTyping", (userId, convId) => OnUserTyping?.Invoke(userId, convId));
        _hub.On<string, int>("UserStoppedTyping", (userId, convId) => OnUserStoppedTyping?.Invoke(userId, convId));
        _hub.On<string>("UserOnline", userId => OnUserOnline?.Invoke(userId));
        _hub.On<string, DateTime>("UserOffline", (userId, when) => OnUserOffline?.Invoke(userId, when));
    }

    private int _activeConversationId;

    public async Task ConnectAsync()
    {
        if (_hub is null) throw new InvalidOperationException("Call Configure() first.");
        if (_hub.State == HubConnectionState.Disconnected)
        {
            try { await _hub.StartAsync(); }
            catch { /* offline — retry later */ }
        }
    }

    public async Task JoinConversationAsync(int conversationId)
    {
        _activeConversationId = conversationId;
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.SendAsync("JoinConversation", conversationId);
    }

    public async Task LeaveConversationAsync(int conversationId)
    {
        _activeConversationId = 0;
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.SendAsync("LeaveConversation", conversationId);
    }

    public async Task SendMessageAsync(int conversationId, string body, long? replyToId = null)
    {
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.SendAsync("SendMessage", conversationId, body, replyToId);
    }

    public async Task NotifyTypingAsync(int conversationId)
    {
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.SendAsync("StartTyping", conversationId);
    }

    public async Task StopTypingAsync(int conversationId)
    {
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.SendAsync("StopTyping", conversationId);
    }

    public async Task DisconnectAsync()
    {
        if (_hub is not null)
        {
            try { await _hub.DisposeAsync(); }
            catch { }
            _hub = null;
        }
    }
}
