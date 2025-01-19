using minimal_api.Domain;
using minimal_api.Helpers;
using minimal_api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace minimal_api_tests.UnitTests
{
    public class CollisionServiceUnitTests
    {
        [Fact]
        public async Task GetCollisionsForOperatorShouldFromDatabase()
        {
            // Arrange

            await using var context = new CollisionDbContext(GetInMemoryOptions());
            var loggerMock = new NullLogger<CollisionService>();
            var colisionService = new CollisionService(context, loggerMock);

            var satelliteId = "42-001";
            var number = 11;
            var operatorId = "TESTBASE";
            var dbExistingId = Guid.NewGuid();

            var collision = new Collision()
            {
                MessageId = "M" + satelliteId + number,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20251211T" + number + "000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow,
                IsCanceled = false
            };

            await context.Collisions.AddAsync(collision);
            await context.SaveChangesAsync();

            // Act
            var collisions = await colisionService.GetCollisionsForOperatorAsync(operatorId);

            //Assert
            Assert.NotNull(collisions);
            Assert.Single(collisions);
        }

       

        private static DbContextOptions<CollisionDbContext> GetInMemoryOptions() =>
             new DbContextOptionsBuilder<CollisionDbContext>()
            .UseInMemoryDatabase($"InMemoryTestDb-{DateTime.Now.ToFileTimeUtc()}")
            .Options;

    }
}