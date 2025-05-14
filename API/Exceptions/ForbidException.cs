namespace API.Exceptions;


public class ForbidException : Exception
{
    public ForbidException(string message = "You are not authorized to access this resource.")
        : base(message)
    {
    }

}
