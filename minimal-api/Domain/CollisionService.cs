using minimal_api.Domain.Interfaces;
using minimal_api.Helpers;
using minimal_api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace minimal_api.Domain
{
    public class CollisionService : ICollisionService
    {
        private readonly CollisionDbContext _db;
        private readonly ILogger<CollisionService> _logger;
        public CollisionService(CollisionDbContext db, ILogger<CollisionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<CollisionStatusDto>> GetColisionsWarningsByOperatorIdAsync(string operatorIdInvoker)
        {
            var dbListSortedWithRules = await _db.Collisions
                .Where(c => c.ProbabilityOfCollision >= 0.75 && c.CollisionDate > DateTimeOffset.UtcNow && c.OperatorId == operatorIdInvoker && !c.IsCanceled)
                .OrderByDescending(c => c.ProbabilityOfCollision)
                .ThenByDescending(c => c.CollisionDate)
                .GroupBy(c => c.SatelliteId)
                .Select(grp => new CollisionStatusDto()
                {
                    satellite_id = grp.First().SatelliteId,
                    highest_probability_of_collision = grp.First().ProbabilityOfCollision,
                    earliest_collision_date = grp.First().CollisionDate.FromUniversalDateTimeOffset(),
                    chaser_object_id = grp.First().ChaserObjectId
                })
                .AsNoTracking() //for faster retrieval as will not change data
                .ToListAsync();

            return dbListSortedWithRules;
        }

        public async Task<Guid> SaveCollisionAsync(string operatorIdInvoker, CollisionDto dto)
        {
            if (!ValidateRequestBasicRules(operatorIdInvoker, dto))
                return Guid.Empty;

            var collisionsForSatellite =
                await _db.Collisions
                .Where(c => c.SatelliteId == dto.satellite_id && !c.IsCanceled)
                .OrderByDescending(c => c.CollisionDate)
                .AsNoTracking()
                .ToListAsync();

            if (collisionsForSatellite.Any(c => c.MessageId == dto.message_id))
            {
                _logger.LogWarning("The collision message {message_id} is already present, not persisting to db", dto.message_id);
                return Guid.Empty;
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

            _db.Collisions.Add(collision);

            await _db.SaveChangesAsync();

            return collision.Id;
        }

        public async Task<Guid> CancelCollisionAsync(string operatorIdInvoker, CollisionDto dto)
        {
            if (!ValidateRequestBasicRules(operatorIdInvoker, dto))
                return Guid.Empty;

            var collisionsForSatellite =
                await _db.Collisions
                .Where(c => c.MessageId == dto.message_id && !c.IsCanceled)
                .OrderByDescending(c => c.CollisionDate)
                .ToListAsync();

            if (!collisionsForSatellite.Any())
            {
                _logger.LogWarning("No collision message/s {message_id} present in db, cannot cancel", dto.message_id);
                return Guid.Empty;
            }

            //cancelling the most recent message
            var collisionTopMostRecent = collisionsForSatellite.First();
            collisionTopMostRecent.IsCanceled = true;
            collisionTopMostRecent.UpdatedDate = DateTime.UtcNow;
            _db.Collisions.Update(collisionTopMostRecent);

            await _db.SaveChangesAsync();
            return collisionTopMostRecent.Id;
        }

        public async Task<List<CollisionDto>> GetCollisionsForOperatorAsync(string operatorIdInvoker)
        {
            var dbList = await _db.Collisions.Where(c => c.OperatorId == operatorIdInvoker).ToListAsync();

            var curatedList = new List<CollisionDto>();
            dbList.ForEach(c => curatedList.Add(new CollisionDto()
            {
                message_id = c.MessageId,
                collision_event_id = c.CollisionEventId,
                satellite_id = c.SatelliteId,
                operator_id = c.OperatorId,
                probability_of_collision = c.ProbabilityOfCollision,
                collision_date = c.CollisionDate.FromUniversalDateTimeOffset(),
                chaser_object_id = c.ChaserObjectId
            }));
            return curatedList;
        }

        private bool ValidateRequestBasicRules(string operatorIdInvoker, CollisionDto dto)
        {
            if (dto.probability_of_collision < 0 || dto.probability_of_collision > 1)
            {
                _logger.LogWarning("The probability_of_collision {probability_of_collision} must be between 0 and 1", dto.probability_of_collision);
                return false;
            }

            if (dto.operator_id != operatorIdInvoker)
            {
                _logger.LogWarning("The operator requesting {operatorIdInvoker} is not the same as in request {operatorID}", operatorIdInvoker, dto.operator_id);
                return false;
            }

            if (dto.collision_date.ToUniversalDateTimeOffset() <= DateTime.UtcNow)
            {
                _logger.LogWarning("The collision date on the message {collision_date} is older than current date {current_date}, not persisting to db",
                    dto.collision_date.ToUniversalDateTimeOffset(),
                    DateTime.UtcNow);
                return false;
            }

            return true;
        }
    }
}
