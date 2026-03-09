using System.Text.Json;
using Photino.NET;

namespace Photinizer.Messaging;

public interface IMessenger
{
    void RegisterWindow(PhotinoWindow window);
    void UnregisterWindow(PhotinoWindow window);

    IMessenger OnMessage(string endpoint, Action<JsonElement> handler);
    IMessenger OnMessage<T>(string endpoint, Action<T> handler);
    IMessenger OnMessageAsync(string endpoint, Func<JsonElement, Task> handler);
    IMessenger OnMessageAsync<T>(string endpoint, Func<T, Task> handler);

    IMessenger OnTask(string endpoint, Action<JsonElement> handler);
    IMessenger OnTask<T>(string endpoint, Action<T> handler);
    IMessenger OnTaskAsync(string endpoint, Func<JsonElement, Task> handler);
    IMessenger OnTaskAsync<T>(string endpoint, Func<T, Task> handler);

    IMessenger OnQuery(string endpoint, Func<JsonElement, object?> handler);
    IMessenger OnQuery<T>(string endpoint, Func<T, object?> handler);
    IMessenger OnQueryAsync(string endpoint, Func<JsonElement, Task<object?>> handler);
    IMessenger OnQueryAsync<T>(string endpoint, Func<T, Task<object?>> handler);

    void SendMessage(string endpoint, object data);
    Task SendTask(string endpoint, object data);
    Task<JsonElement[]> SendQuery(string endpoint, object data);
}