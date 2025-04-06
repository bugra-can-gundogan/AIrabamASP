using Microsoft.EntityFrameworkCore;

namespace AirabamASP.Models.Entities
{
    public class AirabamDBContext : DbContext
    {
        public AirabamDBContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<CookieUser> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarTest> CarsTest { get; set; }
        public DbSet<FeatureValueOutput> FeatureTests { get; set; }
    }
}