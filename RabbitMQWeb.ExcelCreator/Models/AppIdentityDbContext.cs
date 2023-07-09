using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RabbitMQWeb.ExcelCreator.Models
{
    public class AppIdentityDbContext:IdentityDbContext
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options):base(options)
        {
        }

        public DbSet<UserFile> UserFiles { get; set; }
    }
}
