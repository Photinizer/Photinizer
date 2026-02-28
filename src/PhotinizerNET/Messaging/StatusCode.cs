namespace PhotinizerNET.Messaging;

// Thanks to Ivan (GitHub: https://github.com/ivanvoyager )
public readonly record struct StatusCode(int Code)
{
    public static readonly StatusCode NO_ANSWER = new(0);
    public static readonly StatusCode OK = new(200);

    public static implicit operator StatusCode(int statusCode) => new(statusCode);
    public static explicit operator int(StatusCode type) => type.Code;
    public static bool operator ==(StatusCode left, int right) => left.Code == right;
    public static bool operator !=(StatusCode left, int right) => left.Code != right;
    public static bool operator ==(int left, StatusCode right) => left == right.Code;
    public static bool operator !=(int left, StatusCode right) => left != right.Code;
}