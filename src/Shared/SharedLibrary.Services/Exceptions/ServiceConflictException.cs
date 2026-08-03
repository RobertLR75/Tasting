namespace SharedLibrary.Services.Exceptions;

public class ServiceConflictException(string message) : ServiceException(message);


public class ServiceGameStateException(string message) : ServiceException(message);