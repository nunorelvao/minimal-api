namespace minimal_api.Domain
{
    /// <summary>
    /// CollisionStatus Dto Object
    /// </summary>
    public class CollisionStatusDto
    {               
        /// <summary>
        /// Gets or sets the satellite identifier.
        /// </summary>
        /// <value>
        /// The satellite identifier.
        /// </value>
        public string? satellite_id { get; set; }

        /// <summary>
        /// Gets or sets the highest probability of collision.
        /// </summary>
        /// <value>
        /// The highest probability of collision.
        /// </value>
        public double highest_probability_of_collision { get; set; }

        /// <summary>
        /// Gets or sets the earliest collision date.
        /// </summary>
        /// <value>
        /// The earliest collision date.
        /// </value>
        public string? earliest_collision_date { get; set; }
        /// <summary>
        /// Gets or sets the chaser object identifier.
        /// </summary>
        /// <value>
        /// The chaser object identifier.
        /// </value>
        public string? chaser_object_id { get; set; }
    }
}

