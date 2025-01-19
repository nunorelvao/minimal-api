namespace minimal_api.Domain.Interfaces
{
    public interface ICollisionService
    {
        /// <summary>
        /// Gets the colisions warnings by operator identifier asynchronous.
        /// </summary>
        /// <param name="operatorIdInvoker">The operator identifier invoker.</param>
        /// <returns></returns>
        Task<List<CollisionStatusDto>> GetColisionsWarningsByOperatorIdAsync(string operatorIdInvoker);
        /// <summary>
        /// Saves the collision asynchronous.
        /// </summary>
        /// <param name="operatorIdInvoker">The operator identifier invoker.</param>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        Task<Guid> SaveCollisionAsync(string operatorIdInvoker, CollisionDto dto);

        /// <summary>
        /// Cancels the collision asynchronous.
        /// </summary>
        /// <param name="operatorIdInvoker">The operator identifier invoker.</param>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        Task<Guid> CancelCollisionAsync(string operatorIdInvoker, CollisionDto dto);

        /// <summary>
        /// Gets the collisions for operator asynchronous.
        /// </summary>
        /// <param name="operatorIdInvoker">The operator identifier invoker.</param>
        /// <returns></returns>
        Task<List<CollisionDto>> GetCollisionsForOperatorAsync(string operatorIdInvoker);
    }
}
