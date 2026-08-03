using Ardalis.Specification;

namespace SharedLibrary.Interfaces;

public class ApiSpecification<T> : Specification<T>, IApiSpecification<T>;