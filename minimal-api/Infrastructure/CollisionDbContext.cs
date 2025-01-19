using minimal_api.Domain;
using Microsoft.EntityFrameworkCore;

namespace minimal_api.Infrastructure
{
    public class CollisionDbContext : DbContext
    {
        public DbSet<Collision?> Collisions => Set<Collision>();
        public CollisionDbContext(DbContextOptions<CollisionDbContext> options)
            : base(options) { }
    }       
}
