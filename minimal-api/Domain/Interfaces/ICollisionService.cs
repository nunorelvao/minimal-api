namespace minimal_api.Domain.Interfaces
{
    public interface ICollisionService
    {
        /// <summary>
        /// Gets the collisions warnings by operator identifier asynchronous.
        /// </summary>
        /// <param name="operatorId">The operator identifier .</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<CollisionStatusDto>?> GetCollisionsWarningsByOperatorIdAsync(string operatorId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets the collision by id asynchronous.
        /// </summary>
        /// <param name="id">The id identifier.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Collision?> GetCollisionByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Saves the collision asynchronous.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="dto"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(bool, string?)> SaveCollisionAsync(string operatorId, CollisionDto dto, CancellationToken ct = default);

        /// <summary>
        /// Cancels the collision asynchronous.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="dto"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(bool, string?)> CancelCollisionAsync(string operatorId, CollisionDto dto, CancellationToken ct = default);

        /// <summary>
        /// Gets the collisions for operator asynchronous.
        /// </summary>
        /// <param name="operatorId">The operator identifier .</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<CollisionDto>> GetCollisionsForOperatorAsync(string operatorId, CancellationToken ct = default);
    }
}