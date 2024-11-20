using Microsoft.EntityFrameworkCore;

namespace IdempotentConsumerExample.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Tbl_Messages> Tbl_Messages { get; set; }
    }
}
