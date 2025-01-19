using minimal_api.Infrastructure.Models;

namespace minimal_api.Domain
{
    /// <summary>
    /// Collision object to database
    /// </summary>
    public class Collision : BaseAudit
    {
        /// <summary>
        /// Gets or sets the message identifier.
        /// </summary>
        /// <value>
        /// The message identifier. (should not be null)
        /// </value>
        public required string MessageId { get; set; }
        /// <summary>
        /// Gets or sets the collision event identifier.
        /// </summary>
        /// <value>
        /// The collision event identifier.
        /// </value>
        public string? CollisionEventId { get; set; }
        /// <summary>
        /// Gets or sets the satellite identifier.
        /// </summary>
        /// <value>
        /// The satellite identifier.
        /// </value>
        public string? SatelliteId { get; set; }

        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier. (should not be null)
        /// </value>
        public required string OperatorId { get; set; }

        /// <summary>
        /// Gets or sets the probability of collision.
        /// </summary>
        /// <value>
        /// The probability of collision.
        /// </value>
        public double ProbabilityOfCollision { get; set; }

        /// <summary>
        /// Gets or sets the collision date.
        /// </summary>
        /// <value>
        /// The collision date.
        /// </value>
        public DateTimeOffset CollisionDate { get; set; }
        /// <summary>
        /// Gets or sets the chaser object identifier.
        /// </summary>
        /// <value>
        /// The chaser object identifier.
        /// </value>
        public string? ChaserObjectId { get; set; }
    }
}
