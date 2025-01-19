using minimal_api;
using minimal_api.Domain;
using minimal_api.Helpers;
using minimal_api.Infrastructure;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace minimal_api_tests.IntegrationTests
{
    public class EndPointsIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;

        public EndPointsIntegrationTests(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateClient();
        }

        #region collisionsforoperator

        [Fact]
        public async Task GetCollisionsForOperatorWithValidIdShouldReturnData()
        {
            //Arrange
            //All data is being arranged at startup of app already on the helper service
            
            //Act
            var response = await _httpClient.GetAsync("/collisions/alerts/001");

            //Assert
            var responseData = await response.Content.ReadFromJsonAsync<List<CollisionStatusDto>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response);
            Assert.True(responseData?.Count > 0);
        }

        [Fact]
        public async Task GetCollisionsForOperatorWithInvalidIdShouldNotReturnData()
        {
            //Arrange
            //All data is being arranged at startup of app already on the helper service

            //Act
            var response = await _httpClient.GetAsync("/collisions/alerts/123");

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion

        #region collisionalerts

        [Fact]
        public async Task GetCollisionsAlertsForOperatorWithValidIdShouldReturnData()
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 21;
            var operatorId = "001";

            //expected newest date collision with high probability to be reported
            var collision = new Collision()
            {
                MessageId = "M" + satelliteId + number,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = (DateTime.Now.AddDays(30).ToString("yyyyMMdd") + "T" + number + "000100Z")
                    .ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                await db.Collisions.AddAsync(collision);
                await db.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync("/collisions/alerts/001");

            //Assert
            var responseData = await response.Content.ReadFromJsonAsync<List<CollisionStatusDto>>();

            var expected = new CollisionStatusDto()
            {
                satellite_id = collision.SatelliteId,
                highest_probability_of_collision = collision.ProbabilityOfCollision,
                earliest_collision_date = collision.CollisionDate.FromUniversalDateTimeOffset(),
                chaser_object_id = collision.ChaserObjectId
            };

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response);
            Assert.NotNull(responseData);
            Assert.Contains(responseData,
                item =>
                    item.satellite_id == expected.satellite_id &&
                    item.earliest_collision_date == expected.earliest_collision_date &&
                    item.highest_probability_of_collision == expected.highest_probability_of_collision &&
                    item.chaser_object_id == expected.chaser_object_id
            );
            Assert.True(responseData.Count > 0);
        }

        [Fact]
        public async Task GetCollisionsAlertsForOperatorWithValidIdShouldOnlyReturnHisData()
        {
            //Arrange
            var satelliteId = "42-010";
            var number = 21;
            var operatorId = "002";

            //expected newest date collision with high probability to be reported
            var collision = new Collision()
            {
                MessageId = "M" + satelliteId + number,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = (DateTime.Now.AddDays(30).ToString("yyyyMMdd") + "T" + number + "000100Z")
                    .ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                await db.Collisions.AddAsync(collision);
                await db.SaveChangesAsync();
            }

            //Act
            var response = await _httpClient.GetAsync("/collisions/alerts/001");

            //Assert
            var responseData = await response.Content.ReadFromJsonAsync<List<CollisionStatusDto>>();

            var expected = new CollisionStatusDto()
            {
                satellite_id = collision.SatelliteId,
                highest_probability_of_collision = collision.ProbabilityOfCollision,
                earliest_collision_date = collision.CollisionDate.FromUniversalDateTimeOffset(),
                chaser_object_id = collision.ChaserObjectId
            };

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response);
            Assert.NotNull(responseData);
            Assert.DoesNotContain(responseData,
                item =>
                    item.satellite_id == expected.satellite_id &&
                    item.earliest_collision_date == expected.earliest_collision_date &&
                    item.highest_probability_of_collision == expected.highest_probability_of_collision &&
                    item.chaser_object_id == expected.chaser_object_id
            );
            Assert.True(responseData.Count > 0);
        }

        #endregion

        #region collision

        [Fact]
        public async Task PostCollisionForOperatorWithValidIdShouldInsertCorrectly()
        {
            //Arrange
            var satelliteId = "42-006";
            var number = 21;
            var operatorId = "YYY";

            //expected newest date collision with high probability to be reported
            var collision = new CollisionDto()
            {
                message_id = "M" + satelliteId + number,
                collision_event_id = number.ToString(),
                satellite_id = satelliteId,
                operator_id = operatorId,
                probability_of_collision = 1,
                collision_date = "20251211T" + number + "000100Z",
                chaser_object_id = "2016-" + number
            };

            //Act
            var response = await _httpClient.PostAsJsonAsync("/collision/" + operatorId, collision);

            //Assert

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionsForOperatorId = await db.Collisions.Where(c => c.OperatorId == operatorId).ToListAsync();

                Assert.True(collisionsForOperatorId.Count > 0);
                Assert.Contains(collisionsForOperatorId,
                    item =>
                        item.SatelliteId == collision.satellite_id &&
                        item.CollisionDate == collision.collision_date.ToUniversalDateTimeOffset() &&
                        Math.Abs(item.ProbabilityOfCollision - collision.probability_of_collision) < 9 &&
                        item.ChaserObjectId == collision.chaser_object_id &&
                        item.UpdatedDate.HasValue == false &&
                        item.IsCanceled == false
                );
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }


        [Fact]
        public async Task PostCollisionForOperatorWithValidIdNotMatchingPayloadOperatorIdShouldNotInsert()
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 21;
            var operatorId = "YY1";

            //expected newest date collision with high probability to be reported
            var collision = new CollisionDto()
            {
                message_id = "M" + satelliteId + number,
                collision_event_id = number.ToString(),
                satellite_id = satelliteId,
                operator_id = "ZZZ",
                probability_of_collision = 1,
                collision_date = "20251211T" + number + "000100Z",
                chaser_object_id = "2016-" + number
            };

            //Act
            var response = await _httpClient.PostAsJsonAsync("/collision/001", collision);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionsForOperatorId = db.Collisions.Where(c => c.OperatorId == operatorId).ToList();

                Assert.True(collisionsForOperatorId.Count == 0);
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-1.232522)]
        [InlineData(-0.2)]
        [InlineData(1.999999)]
        [InlineData(1.5)]
        [InlineData(1.2)]
        [InlineData(2.2)]
        public async Task PostCollisionNotValidRangeOfProbabilityShouldNotInsert(double probability)
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 21;
            var operatorId = "XYZ";

            //expected newest date collision with high probability to be reported
            var collision = new CollisionDto()
            {
                message_id = "M" + satelliteId + number,
                collision_event_id = number.ToString(),
                satellite_id = satelliteId,
                operator_id = operatorId,
                probability_of_collision = probability,
                collision_date = "20251211T" + number + "000100Z",
                chaser_object_id = "2016-" + number
            };

            //Act
            var response = await _httpClient.PostAsJsonAsync("/collision/" + operatorId, collision);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionsForOperatorId = db.Collisions.Where(c => c.OperatorId == operatorId).ToList();

                Assert.True(collisionsForOperatorId.Count == 0);
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task PostCollisionForOperatorWithValidIdWithAlreadyExistingMessageIdOnDbShouldNotInsert()
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 21;
            var operatorId = "TTT";
            var messageId = "M" + satelliteId + number;

            var collision = new Collision()
            {
                MessageId = messageId,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20251211T" + number + "000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow
            };

            var collisionDtoSameIdDifferentData = new CollisionDto()
            {
                message_id = messageId,
                collision_event_id = number.ToString() + "-XXX",
                satellite_id = satelliteId,
                operator_id = operatorId,
                probability_of_collision = 1,
                collision_date = "20251211T" + number + "000100Z",
                chaser_object_id = "2016-" + number + "-XXX"
            };

            //insert one forced in DB with specific messageId
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                await db.Collisions.AddAsync(collision);
                await db.SaveChangesAsync();
            }

            //Act
            //try and post same messageId
            var response =
                await _httpClient.PostAsJsonAsync("/collision/" + operatorId, collisionDtoSameIdDifferentData);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionsForOperatorId = db.Collisions.Where(c => c.OperatorId == operatorId).ToList();

                Assert.True(collisionsForOperatorId.Count == 1);
                Assert.DoesNotContain(collisionsForOperatorId,
                    item =>
                        item.SatelliteId == collisionDtoSameIdDifferentData.satellite_id &&
                        item.CollisionDate ==
                        collisionDtoSameIdDifferentData.collision_date.ToUniversalDateTimeOffset() &&
                        item.ProbabilityOfCollision == collisionDtoSameIdDifferentData.probability_of_collision &&
                        item.ChaserObjectId == collisionDtoSameIdDifferentData.chaser_object_id
                );
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Theory]
        [InlineData("20241210T20000100Z")]
        [InlineData("20241210T21000100Z")]
        public async Task PostCollisionForOperatorWithValidIdWithOlderDateThanCurrentDateShouldNotInsert(
            string collisiondate)
        {
            //Arrange

            var number = 21;
            var operatorId = "SINGLE";
            var satelliteId = "42-001" + operatorId;
            var messageId = "M" + satelliteId + number;

            var collision = new Collision()
            {
                MessageId = messageId,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20251211T21000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow
            };

            var collision2 = new Collision()
            {
                MessageId = messageId + 2,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20251211T22000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow
            };

            var collision3 = new Collision()
            {
                MessageId = messageId + 3,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20221211T22000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow
            };

            var collisionDtoWithOldercollisionDate = new CollisionDto()
            {
                message_id = messageId + collisiondate,
                collision_event_id = number.ToString() + "-XXX",
                satellite_id = satelliteId,
                operator_id = operatorId,
                probability_of_collision = 1,
                collision_date = collisiondate, //less or equal than already last db present one
                chaser_object_id = "2016-" + number + "-XXX"
            };

            //insert one forced in DB with specific messageId only once
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                if (!db.Collisions.Any(c => c.SatelliteId == satelliteId))
                {
                    await db.Collisions.AddAsync(collision);
                    await db.Collisions.AddAsync(collision2);
                    await db.Collisions.AddAsync(collision3);
                    await db.SaveChangesAsync();
                }
            }

            //Act
            //try and post same messageId
            var response =
                await _httpClient.PostAsJsonAsync("/collision/" + operatorId, collisionDtoWithOldercollisionDate);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionsForOperatorId = db.Collisions.Where(c => c.OperatorId == operatorId).ToList();

                Assert.True(collisionsForOperatorId.Count == 3);
                Assert.DoesNotContain(collisionsForOperatorId,
                    item =>
                        item.SatelliteId == collisionDtoWithOldercollisionDate.satellite_id &&
                        item.CollisionDate ==
                        collisionDtoWithOldercollisionDate.collision_date.ToUniversalDateTimeOffset() &&
                        item.ProbabilityOfCollision == collisionDtoWithOldercollisionDate.probability_of_collision &&
                        item.ChaserObjectId == collisionDtoWithOldercollisionDate.chaser_object_id
                );
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        #endregion

        #region collisioncancel

        [Fact]
        public async Task PatchCollisionCancelForOperatorWithValidIdAndExistingMessageIdShouldUpdateCorrectly()
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 11;
            var operatorId = "CLS";

            var collision1 = new Collision()
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

            var collision2 = new Collision()
            {
                MessageId = "M" + satelliteId + number + 1,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20251211T" + (number + 1) + "000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow,
                IsCanceled = false
            };

            var collision3 = new Collision()
            {
                MessageId = "M" + satelliteId + number + 2,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20251211T" + (number + 2) + "000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow,
                IsCanceled = false
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                await db.Collisions.AddAsync(collision1);
                await db.Collisions.AddAsync(collision2);
                await db.Collisions.AddAsync(collision3);
                await db.SaveChangesAsync();
            }

            //expected collision to exist to be patched (canceled)
            var collisiondto = new CollisionDto()
            {
                message_id = "M" + satelliteId + number + 2,
                collision_event_id = number.ToString(),
                satellite_id = satelliteId,
                operator_id = operatorId,
                probability_of_collision = 1,
                collision_date = "20251211T11000100Z",
                chaser_object_id = "2016-" + number
            };

            //Act
            var response = await _httpClient.PatchAsJsonAsync("/collision/" + operatorId, collisiondto);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionExpectedPatched =
                    await db.Collisions.FirstOrDefaultAsync(c => c.MessageId == collisiondto.message_id);
                Assert.NotNull(collisionExpectedPatched);
                Assert.True(collisionExpectedPatched.IsCanceled);
                Assert.NotNull(collisionExpectedPatched.UpdatedDate);
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task PatchCollisionCancelForOperatorWithValidIdNoSameMessageIdFoundShouldNotUpdate()
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 11;
            var operatorId = "CLS";

            var collision1 = new Collision()
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

            var collision2 = new Collision()
            {
                MessageId = "M" + satelliteId + number + 1,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20251211T" + (number + 1) + "000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow,
                IsCanceled = false
            };

            var collision3 = new Collision()
            {
                MessageId = "M" + satelliteId + number + 2,
                CollisionEventId = number.ToString(),
                SatelliteId = satelliteId,
                OperatorId = operatorId,
                ProbabilityOfCollision = 1,
                CollisionDate = ("20251211T" + (number + 2) + "000100Z").ToUniversalDateTimeOffset(),
                ChaserObjectId = "2016-" + number,
                CreatedDate = DateTime.UtcNow,
                IsCanceled = false
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                await db.Collisions.AddAsync(collision1);
                await db.Collisions.AddAsync(collision2);
                await db.Collisions.AddAsync(collision3);
                await db.SaveChangesAsync();
            }

            //expected collision to exist to be patched (canceled)
            var collisiondto = new CollisionDto()
            {
                message_id = "M" + satelliteId + number + 99,
                collision_event_id = number.ToString(),
                satellite_id = satelliteId,
                operator_id = operatorId,
                probability_of_collision = 1,
                collision_date = "20251211T11000100Z",
                chaser_object_id = "2016-" + number
            };

            //Act
            var response = await _httpClient.PatchAsJsonAsync("/collision/" + operatorId, collisiondto);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionNotExpectedPatched =
                    await db.Collisions.FirstOrDefaultAsync(c => c.MessageId == collisiondto.message_id);
                Assert.Null(collisionNotExpectedPatched);
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-1.232522)]
        [InlineData(-0.2)]
        [InlineData(1.999999)]
        [InlineData(1.5)]
        [InlineData(1.2)]
        [InlineData(2.2)]
        public async Task PatchCollisionCancelNotValidRangeOfProbabilityShouldNotInsert(double probability)
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 11;
            var operatorId = "CLS";

            //expected collision to exist to be patched (canceled)
            var collisiondto = new CollisionDto()
            {
                message_id = "M" + satelliteId + number + 99,
                collision_event_id = number.ToString(),
                satellite_id = satelliteId,
                operator_id = operatorId,
                probability_of_collision = probability,
                collision_date = "20251211T11000100Z",
                chaser_object_id = "2016-" + number
            };

            //Act
            var response = await _httpClient.PatchAsJsonAsync("/collision/" + operatorId, collisiondto);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionNotExpectedPatched =
                    await db.Collisions.FirstOrDefaultAsync(c => c.MessageId == collisiondto.message_id);
                Assert.Null(collisionNotExpectedPatched);
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task PatchCollisionCancelNotValidOperatorIdMatchShouldNotInsert()
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 11;
            var operatorId = "CLS";

            //expected collision to exist to be patched (canceled)
            var collisiondto = new CollisionDto()
            {
                message_id = "M" + satelliteId + number + 99,
                collision_event_id = number.ToString(),
                satellite_id = satelliteId,
                operator_id = "ZZZ",
                probability_of_collision = 1,
                collision_date = "20251211T11000100Z",
                chaser_object_id = "2016-" + number
            };

            //Act
            var response = await _httpClient.PatchAsJsonAsync("/collision/" + operatorId, collisiondto);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionNotExpectedPatched =
                    await db.Collisions.FirstOrDefaultAsync(c => c.MessageId == collisiondto.message_id);
                Assert.Null(collisionNotExpectedPatched);
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task PatchCollisionCancelOlderDateThanNowShouldNotInsert()
        {
            //Arrange
            var satelliteId = "42-001";
            var number = 11;
            var operatorId = "CLS";

            //expected collision to exist to be patched (canceled)
            var collisiondto = new CollisionDto()
            {
                message_id = "M" + satelliteId + number + 99,
                collision_event_id = number.ToString(),
                satellite_id = satelliteId,
                operator_id = operatorId,
                probability_of_collision = 1,
                collision_date = "20211211T11000100Z",
                chaser_object_id = "2016-" + number
            };

            //Act
            var response = await _httpClient.PatchAsJsonAsync("/collision/" + operatorId, collisiondto);

            //Assert
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CollisionDbContext>();
                var collisionNotExpectedPatched =
                    await db.Collisions.FirstOrDefaultAsync(c => c.MessageId == collisiondto.message_id);
                Assert.Null(collisionNotExpectedPatched);
            }

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        #endregion
    }
}