namespace SharedLibrary.Services.Exceptions;

public class ServiceException(string message) : ApplicationException(message);