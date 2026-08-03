namespace SharedLibrary.Services.Exceptions;

public class ForbiddenException(string message) : ServiceException(message);
