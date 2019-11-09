using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Value> Values { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserActivity>
            (
                e => e.HasKey(ua => new { ua.AppUserId, ua.ActivityId })
            );

            builder.Entity<UserActivity>()
				.HasOne(u => u.AppUser)
				.WithMany(a => a.UserActivities)
				.HasForeignKey(u => u.AppUserId);

            builder.Entity<UserActivity>()
                .HasOne(a => a.Activity)
                .WithMany(u => u.UserActivities)
                .HasForeignKey(a => a.ActivityId);

            // builder.Entity<AppUser>(e => e.ToTable("Users"));
            // builder.Entity<Role>(e => e.ToTable("Roles"));
            // builder.Entity<UserRole>(e => e.ToTable("UserRoles"));

            // builder.Entity<IdentityUserClaim<string>>(e => e.ToTable("UserClaims"));
            // builder.Entity<IdentityUserLogin<string>>(e => e.ToTable("UserLogins"));
            // builder.Entity<IdentityUserToken<string>>(e => e.ToTable("UserTokens"));
            // builder.Entity<IdentityRoleClaim<string>>(e => e.ToTable("RoleClaims"));
        }
    }
}
