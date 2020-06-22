using Microsoft.EntityFrameworkCore;

namespace ActivityCenter.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options) { }

            public DbSet<User> Users { get; set; }

            public DbSet<Gathering> Gatherings { get; set; }

            public DbSet<GuestList> GuestLists { get; set; }
    }
}