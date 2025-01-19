using minimal_api.Domain.Interfaces;
using minimal_api.Helpers;
using minimal_api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace minimal_api.Domain
{
    public class CollisionService(CollisionDbContext db, ILogger<CollisionService>? logger) : ICollisionService
    {
        public async Task<List<CollisionStatusDto>> GetCollisionsWarningsByOperatorIdAsync(string operatorId,
            CancellationToken ct = default)
        {
            var dbListSortedWithRules = await db.Collisions
                .Where(c => c != null &&
                            c.ProbabilityOfCollision >= 0.75 &&
                            c.CollisionDate > DateTimeOffset.UtcNow &&
                            c.OperatorId == operatorId &&
                            !c.IsCanceled)
                .OrderByDescending(c => c!.ProbabilityOfCollision)
                .ThenByDescending(c => c!.CollisionDate)
                .GroupBy(c => c!.SatelliteId)
                .Select(grp => new CollisionStatusDto()
                {
                    satellite_id = grp.FirstOrDefault()!.SatelliteId,
                    highest_probability_of_collision = grp.FirstOrDefault()!.ProbabilityOfCollision,
                    earliest_collision_date = grp.FirstOrDefault()!.CollisionDate.FromUniversalDateTimeOffset(),
                    chaser_object_id = grp.FirstOrDefault()!.ChaserObjectId
                })
                .AsNoTracking() //for faster retrieval as will not change data
                .ToListAsync(ct);

            return dbListSortedWithRules;
        }

        public async Task<Collision?> GetCollisionByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await db.Collisions.FirstOrDefaultAsync(c => c != null && c.Id == id, cancellationToken: ct);
        }

        public async Task<(bool, string?)> SaveCollisionAsync(string operatorId, CollisionDto dto,
            CancellationToken ct = default)
        {
            if (!ValidateRequestBasicRules(operatorId, dto, out var errorValidation))
                return (false, errorValidation);

            var collisionsForSatellite =
                await db.Collisions
                    .Where(c => c != null &&
                                c.MessageId == dto.message_id &&
                                c.SatelliteId == dto.satellite_id &&
                                !c.IsCanceled)
                    .OrderByDescending(c => c!.CollisionDate)
                    .AsNoTracking()
                    .ToListAsync(ct);

            if (collisionsForSatellite.Any())
            {
                var error = $"The collision message {dto.message_id} is already present, not persisting to db";
                logger?.LogWarning(error);
                return (false, error);
            }

            //all this mapping would probably be in production handled with a mapper
            //or be put on static method to map manually if no need for complexity on microservice
            var collision = new Collision()
            {
                MessageId = dto.message_id,
                CollisionEventId = dto.collision_event_id,
                SatelliteId = dto.satellite_id,
                OperatorId = dto.operator_id,
                ProbabilityOfCollision = dto.probability_of_collision,
                CollisionDate = dto.collision_date.ToUniversalDateTimeOffset(),
                ChaserObjectId = dto.chaser_object_id
            };

            db.Collisions.Add(collision);

            await db.SaveChangesAsync(ct);

            return (true, collision.Id.ToString());
        }

        public async Task<(bool, string?)> CancelCollisionAsync(string operatorId, CollisionDto dto, CancellationToken ct = default)
        {
            if (!ValidateRequestBasicRules(operatorId, dto, out var errorValidation))
                return (false, errorValidation);

            var collisionTopMostRecent =
                await db.Collisions
                    .Where(c => c != null && c.MessageId == dto.message_id && !c.IsCanceled)
                    .OrderByDescending(c => c!.CollisionDate)
                    .FirstOrDefaultAsync(ct);

            //cancelling the most recent message
            if (collisionTopMostRecent == null)
            {
                var error = $"No collision message/s {dto.message_id} present in db, cannot cancel";
                logger?.LogWarning(error);
                return (false, error);
            }

            collisionTopMostRecent.IsCanceled = true;
            collisionTopMostRecent.UpdatedDate = DateTime.UtcNow;
            db.Collisions.Update(collisionTopMostRecent);

            await db.SaveChangesAsync(ct);

            return (true, collisionTopMostRecent.Id.ToString());
        }

        public async Task<List<CollisionDto>> GetCollisionsForOperatorAsync(string operatorId, CancellationToken ct = default)
        {
            var dbList = await db.Collisions.Where(c => c != null && c.OperatorId == operatorId).ToListAsync(ct);

            var curatedList = new List<CollisionDto>();
            dbList.ForEach(c =>
            {
                if (c != null)
                {
                    curatedList.Add(new CollisionDto()
                    {
                        message_id = c.MessageId, collision_event_id = c.CollisionEventId, satellite_id = c.SatelliteId,
                        operator_id = c.OperatorId, probability_of_collision = c.ProbabilityOfCollision,
                        collision_date = c.CollisionDate.FromUniversalDateTimeOffset(),
                        chaser_object_id = c.ChaserObjectId
                    });
                }
            });

            return curatedList;
        }

        private bool ValidateRequestBasicRules(string operatorId, CollisionDto dto, out string? error)
        {
            error = "";
            if (dto.probability_of_collision is < 0 or > 1)
            {
                error = $"The probability_of_collision {dto.probability_of_collision} must be between 0 and 1";
                logger?.LogWarning(error);
                return false;
            }
            
            if (operatorId != dto.operator_id)
            {
                error = $"The operator requesting {operatorId} is not the same as in request {dto.operator_id}";
                logger?.LogWarning(error);
                return false;
            }
            
            if (dto.collision_date.ToUniversalDateTimeOffset() <= DateTime.UtcNow)
            {
                error =
                    $"The collision date on the message {dto.collision_date.ToUniversalDateTimeOffset()} " +
                    $"is older than current date {DateTime.UtcNow}, not persisting to db";
                logger?.LogWarning(error);
                return false;
            }

            return true;
        }
    }
}