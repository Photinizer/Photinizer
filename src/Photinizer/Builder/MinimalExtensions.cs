using System.Text.Json;
using Photinizer.Messaging;

namespace Photinizer.Builder;

/// <summary>
/// Minimal-API style extensions for Photinizer.Application.
/// Keeps the public surface intuitive for ASP.NET Core developers:
///     app.MapQuery("Hello", _ => "World");
/// </summary>
public static class MinimalExtensions
{
    extension(Application app)
    {
        // ----- Query (request -> response) -----

        public Application MapQuery(string endpoint, Func<JsonElement, object> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnQuery(endpoint, handler));
            app.Messenger.OnQuery(endpoint, handler);
            return app;
        }

        public Application MapQuery<T>(string endpoint, Func<T?, object> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnQuery(endpoint, handler));
            app.Messenger.OnQuery(endpoint, handler);
            return app;
        }

        public Application MapQueryAsync(string endpoint, Func<JsonElement, Task<object>> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnQueryAsync(endpoint, handler));
            app.Messenger.OnQueryAsync(endpoint, handler);
            return app;
        }

        public Application MapQueryAsync<T>(string endpoint, Func<T?, Task<object>> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnQueryAsync(endpoint, handler));
            app.Messenger.OnQueryAsync(endpoint, handler);
            return app;
        }

        // ----- Task (fire-and-wait ack; NeedResponse=true, no data) -----

        public Application MapTask(string endpoint, Action<JsonElement> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnTask(endpoint, handler));
            app.Messenger.OnTask(endpoint, handler);
            return app;
        }

        public Application MapTask<T>( string endpoint, Action<T?> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnTask(endpoint, handler));
            app.Messenger.OnTask(endpoint, handler);
            return app;
        }

        public Application MapTaskAsync(string endpoint, Func<JsonElement, Task> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnTaskAsync(endpoint, handler));
            app.Messenger.OnTaskAsync(endpoint, handler);
            return app;
        }

        public Application MapTaskAsync<T>(string endpoint, Func<T?, Task> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnTaskAsync(endpoint, handler));
            app.Messenger.OnTaskAsync(endpoint, handler);
            return app;
        }

        // ----- Message (fire-and-forget; NeedResponse=false) -----

        public Application MapMessage(string endpoint, Action<JsonElement> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnMessage(endpoint, handler));
            app.Messenger.OnMessage(endpoint, handler);
            return app;
        }

        public Application MapMessage<T>(string endpoint, Action<T?> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnMessage(endpoint, handler));
            app.Messenger.OnMessage(endpoint, handler);
            return app;
        }

        public Application MapMessageAsync(string endpoint, Func<JsonElement, Task> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnMessageAsync(endpoint, handler));
            app.Messenger.OnMessageAsync(endpoint, handler);
            return app;
        }

        public Application MapMessageAsync<T>(string endpoint, Func<T?, Task> handler)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(handler);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.OnMessageAsync(endpoint, handler));
            app.Messenger.OnMessageAsync(endpoint, handler);
            return app;
        }

        // ----- Register -----

        public Application Register(INeedMessenger service)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(service);
            if (!app.IsRunning) return app.AfterStart(a => a.Messenger.Register(service));
            app.Messenger.Register(service);
            return app;
        }
    }
}