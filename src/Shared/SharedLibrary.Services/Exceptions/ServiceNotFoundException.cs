namespace SharedLibrary.Services.Exceptions;

public class ServiceNotFoundException(string message) : ServiceException(message);