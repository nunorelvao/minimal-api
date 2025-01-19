using minimal_api.Domain;
using minimal_api.Infrastructure;

namespace minimal_api.Helpers
{
    /// <summary>
    /// Helper as a service to be injected on startup
    /// </summary>
    public class DummyDataHelper
    {
        private readonly ILogger<DummyDataHelper> _logger;
        private readonly IServiceProvider _serviceProvider;
        public DummyDataHelper(
            IServiceProvider serviceProvider,
            ILogger<DummyDataHelper> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Generates the data.
        /// </summary>
        public async Task GenerateData()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();

                //generate data for operatorid = 001
                //adding same data for different satelliteId so it can be sure to have at least 2 in grouping by satelliteId
                await AddRandomDataForOperator(dbContext, "001", "42-001");
                await AddRandomDataForOperator(dbContext, "001", "42-002");

                //generate data for operatorid = 002
                //adding same data for different satelliteId so it can be sure to have at least 2 in grouping by satelliteId
                await AddRandomDataForOperator(dbContext, "002", "42-001");
                await AddRandomDataForOperator(dbContext, "002", "42-002");

                //generate data for operatorid = 003
                //adding same data for different satelliteId so it can be sure to have at least 2 in grouping by satelliteId
                await AddRandomDataForOperator(dbContext, "003", "42-001");
                await AddRandomDataForOperator(dbContext, "003", "42-002");

                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Dummy data populated!");
            }
        }

        private async Task AddRandomDataForOperator(CollisionDbContext dbContext, string operatorId, string satelliteId)
        {
            for (var i = 10; i < 21; i++)
            {
                await dbContext.AddAsync(
                new Collision()
                {
                    MessageId = "M" + satelliteId + i,
                    CollisionEventId = i.ToString(),
                    SatelliteId = satelliteId,
                    OperatorId = operatorId,
                    ProbabilityOfCollision = NextRandomDouble2Digit(new Random()),
                    CollisionDate =  (DateTime.Now.AddDays(30).ToString("yyyyMMdd") +"T" + i + "000100Z").ToUniversalDateTimeOffset(),
                    ChaserObjectId = "2016-" + i,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        //helper to generate random probability of collision float with 2 digit
        static double NextRandomDouble2Digit(Random random)
        {
            return Math.Round(random.NextDouble(), 2);
        }
    }
}
