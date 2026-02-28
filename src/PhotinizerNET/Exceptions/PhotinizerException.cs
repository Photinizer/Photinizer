namespace PhotinizerNET.Exceptions;

internal class PhotinizerException : Exception
{
    public PhotinizerException() : base()
    {
    }

    public PhotinizerException(string message) : base(message)
    {
    }

    public PhotinizerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}