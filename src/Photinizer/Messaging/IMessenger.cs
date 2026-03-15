using System.Text.Json;
using Photino.NET;

namespace Photinizer.Messaging;

public interface IMessenger
{
    void RegisterWindow(PhotinoWindow window);
    void UnregisterWindow(PhotinoWindow window);

    IMessenger OnMessage(string endpoint, Action<PhotinoWindow, JsonElement> handler);
    IMessenger OnMessage<T>(string endpoint, Action<PhotinoWindow, T> handler);
    IMessenger OnMessageAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task> handler);
    IMessenger OnMessageAsync<T>(string endpoint, Func<PhotinoWindow, T, Task> handler);

    IMessenger OnTask(string endpoint, Action<PhotinoWindow, JsonElement> handler);
    IMessenger OnTask<T>(string endpoint, Action<PhotinoWindow, T> handler);
    IMessenger OnTaskAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task> handler);
    IMessenger OnTaskAsync<T>(string endpoint, Func<PhotinoWindow, T, Task> handler);

    IMessenger OnQuery(string endpoint, Func<PhotinoWindow, JsonElement, object?> handler);
    IMessenger OnQuery<T>(string endpoint, Func<PhotinoWindow, T, object?> handler);
    IMessenger OnQueryAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task<object?>> handler);
    IMessenger OnQueryAsync<T>(string endpoint, Func<PhotinoWindow, T, Task<object?>> handler);

    void SendMessage(string endpoint, object data);
    Task SendTask(string endpoint, object data, CancellationToken cancellationToken = default);
    Task<JsonElement[]> SendQuery(string endpoint, object data, CancellationToken cancellationToken = default);
}