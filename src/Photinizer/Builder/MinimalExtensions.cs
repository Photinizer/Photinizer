using System.Text.Json;
using Photino.NET;

namespace Photinizer.Builder;

/// <summary>
/// Minimal-API style extensions for Photinizer.Application.
/// Keeps the public surface intuitive for ASP.NET Core developers:
///     app.MapQuery("Hello", (_, _) => "World");
/// </summary>
public static class MinimalExtensions
{
    extension(Application app)
    {
        // ----- Query (request -> response) -----

        public Application MapQuery(string endpoint, Func<PhotinoWindow, JsonElement, object> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnQuery(endpoint, handler);
            return app;
        }

        public Application MapQuery<T>(string endpoint, Func<PhotinoWindow, T?, object> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnQuery(endpoint, handler);
            return app;
        }

        public Application MapQueryAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task<object?>> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnQueryAsync(endpoint, handler);
            return app;
        }

        public Application MapQueryAsync<T>(string endpoint, Func<PhotinoWindow, T?, Task<object?>> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnQueryAsync(endpoint, handler);
            return app;
        }

        // ----- Task (fire-and-wait ack; NeedResponse=true, no data) -----

        public Application MapTask(string endpoint, Action<PhotinoWindow, JsonElement> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnTask(endpoint, handler);
            return app;
        }

        public Application MapTask<T>( string endpoint, Action<PhotinoWindow, T?> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnTask(endpoint, handler);
            return app;
        }

        public Application MapTaskAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnTaskAsync(endpoint, handler);
            return app;
        }

        public Application MapTaskAsync<T>(string endpoint, Func<PhotinoWindow, T?, Task> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnTaskAsync(endpoint, handler);
            return app;
        }

        // ----- Message (fire-and-forget; NeedResponse=false) -----

        public Application MapMessage(string endpoint, Action<PhotinoWindow, JsonElement> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnMessage(endpoint, handler);
            return app;
        }

        public Application MapMessage<T>(string endpoint, Action<PhotinoWindow, T?> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnMessage(endpoint, handler);
            return app;
        }

        public Application MapMessageAsync(string endpoint, Func<PhotinoWindow, JsonElement, Task> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnMessageAsync(endpoint, handler);
            return app;
        }

        public Application MapMessageAsync<T>(string endpoint, Func<PhotinoWindow, T?, Task> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            app.Messenger.OnMessageAsync(endpoint, handler);
            return app;
        }
    }
}