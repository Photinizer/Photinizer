using System.Text.Json;

namespace PhotinizerNET.Lib.Backend.Messaging;

internal record RequestHandler(Func<JsonElement, Task<object>> HandleFunc, bool NeedResponse)
{
    public RequestHandler(Func<JsonElement, Task> HandleFunc, bool NeedResponse)
        : this(async args => { await HandleFunc(args); return StatusCode.OK; }, NeedResponse) {}
}