namespace minimal_api.Domain.Models;

/// <summary>
/// Collision Dto Object
/// </summary>
public class CollisionDto
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    /// <value>
    /// The message identifier. (should not be null)
    /// </value>
    public required string message_id { get; set; }
    /// <summary>
    /// Gets or sets the collision event identifier.
    /// </summary>
    /// <value>
    /// The collision event identifier.
    /// </value>
    public string? collision_event_id { get; set; }
    /// <summary>
    /// Gets or sets the satellite identifier.
    /// </summary>
    /// <value>
    /// The satellite identifier.
    /// </value>
    public string? satellite_id { get; set; }

    /// <summary>
    /// Gets or sets the operator identifier.
    /// </summary>
    /// <value>
    /// The operator identifier. (should not be null)
    /// </value>
    public required string operator_id { get; set; }

    /// <summary>
    /// Gets or sets the probability of collision.
    /// </summary>
    /// <value>
    /// The probability of collision.
    /// </value>
    public double probability_of_collision { get; set; }

    /// <summary>
    /// Gets or sets the collision date.
    /// </summary>
    /// <value>
    /// The collision date.
    /// </value>
    public required string collision_date { get; set; }
    /// <summary>
    /// Gets or sets the chaser object identifier.
    /// </summary>
    /// <value>
    /// The chaser object identifier.
    /// </value>
    public string? chaser_object_id { get; set; }
}