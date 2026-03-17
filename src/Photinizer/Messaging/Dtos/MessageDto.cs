namespace Photinizer.Messaging.Dtos;

internal class MessageDto
{
    public RequestDto? Request { get; set; }
    public ResponseDto? Response { get; set; }
    public ErrorNotificationDto? Error { get; set; }
}