using Ardalis.Specification;

namespace SharedLibrary.Interfaces;

public class PersistenceSpecification<T> : Specification<T>, IPersistenceSpecification<T>;

public class PersistenceSpecification<T, TResult> : Specification<T, TResult>, IPersistenceSpecification<T, TResult>;