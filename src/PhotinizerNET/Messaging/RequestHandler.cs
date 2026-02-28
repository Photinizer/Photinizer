using System.Text.Json;

namespace PhotinizerNET.Messaging;


internal readonly record struct RequestHandler(Func<JsonElement, Task<object>> HandleFunc, bool NeedResponse)
{
    public RequestHandler(Func<JsonElement, Task> HandleFunc, bool NeedResponse)
        : this(async args => { await HandleFunc(args); return StatusCode.OK; }, NeedResponse) {}
}