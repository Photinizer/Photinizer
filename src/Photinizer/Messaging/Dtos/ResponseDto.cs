using System.Text.Json;

namespace Photinizer.Messaging.Dtos;

internal class ResponseDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RequestId { get; set; } = null!;
    public JsonElement? Result { get; set; } = null!;
    public JsonElement? Error { get; set; } = null!;

    public static ResponseDto OK(RequestDto request) => FromResult(request, StatusCode.OK);

    public static ResponseDto FromResult(RequestDto request, object? result) =>
        new()
        {
            RequestId = request.Id,
            Result = JsonSerializer.SerializeToElement(result)
        };

    //TODO: error != object
    public static ResponseDto FromError(RequestDto request, object error) =>
        new()
        {
            RequestId = request.Id,
            Error = JsonSerializer.SerializeToElement(error)
        };
}