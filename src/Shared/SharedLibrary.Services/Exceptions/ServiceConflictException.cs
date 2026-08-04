namespace SharedLibrary.Services.Exceptions;

public class ServiceConflictException(string message) : ConflictException(message);


public class ServiceGameStateException(string message) : ServiceException(message);