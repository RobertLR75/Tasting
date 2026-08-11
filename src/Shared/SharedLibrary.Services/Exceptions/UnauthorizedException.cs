namespace SharedLibrary.Services.Exceptions;

public class UnauthorizedException(string message) : ServiceException(message);
