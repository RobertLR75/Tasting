using Ardalis.Specification;

namespace SharedLibrary.Interfaces;

public interface IPersistenceSpecification<T> : ISpecification<T>;
public interface IPersistenceSpecification<T, TResult> : ISpecification<T, TResult>;