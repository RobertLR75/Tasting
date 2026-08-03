// namespace SharedLibrary.Services.Interfaces;
//
// public interface IServiceMapper;
// public interface IRequestServiceMapper : IServiceMapper;
//
//
// public interface IResponseServiceMapper : IServiceMapper;
//
// /// <summary>
// /// use this interface to implement a domain entity mapper for your endpoints that only has a request dto and no response dto.
// /// <para>HINT: entity mappers are used as singletons for performance reasons. do not maintain state in the mappers.</para>
// /// </summary>
// /// <typeparam name="TRequest">the type of request dto</typeparam>
// /// <typeparam name="TEntity">the type of domain entity to map to/from</typeparam>
// public interface IRequestServiceMapper<in TRequest, TEntity> : IRequestServiceMapper
// {
//     /// <summary>
//     /// implement this method and place the logic for mapping the request dto to the desired domain entity
//     /// </summary>
//     /// <param name="ev">the request dto</param>
//     TEntity? ToEntity(TRequest ev);
//     
//     /// <summary>
//     /// implement this method and place the logic for mapping the updated request dto to the desired domain entity
//     /// </summary>
//     /// <param name="r">the request dto to update from</param>
//     /// <param name="e">the domain entity to update</param>
//     TEntity? UpdateEntity(TRequest r, TEntity e);
// }
//
//
//
// /// <summary>
// /// use this interface to implement a domain entity mapper for your endpoints that only has a response dto and no request dto.
// /// <para>HINT: entity mappers are used as singletons for performance reasons. do not maintain state in the mappers.</para>
// /// </summary>
// /// <typeparam name="TResponse">the type of response dto</typeparam>
// /// <typeparam name="TEntity">the type of domain entity to map to/from</typeparam>
// public interface IResponseServiceMapper<TResponse, in TEntity> : IResponseServiceMapper
// {
//     /// <summary>
//     /// implement this method and place the logic for mapping a domain entity to a response dto
//     /// </summary>
//     /// <param name="e">the domain entity to map from</param>
//     TResponse? FromEntity(TEntity e);
// }
//
