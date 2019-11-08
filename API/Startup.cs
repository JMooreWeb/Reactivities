using API.Middleware;
using Application.Activities;
using Application.Interfaces;
using Domain;
using FluentValidation.AspNetCore;
using Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using System.Text;

namespace API
{
	public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
                options.UseLazyLoadingProxies();
            });

			//services.AddCors(options =>
			//{
			//	options.AddPolicy("CorsPolicy", policy =>
			//	{
			//		policy.AllowAnyOrigin().AllowAnyMethod()
   //                     .AllowAnyHeader().WithOrigins("http://localhost:3000");
			//	});
			//});

            services.AddMediatR(typeof(List.Handler).Assembly);
            
            services.AddControllers(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser().Build();
                    
                options.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddFluentValidation(config => 
                    config.RegisterValidatorsFromAssemblyContaining<Create>());

            var builder = services.AddIdentityCore<AppUser>();

            var identityBuilder = new IdentityBuilder(builder.UserType, builder.Services);
            identityBuilder.AddEntityFrameworkStores<DataContext>();
            identityBuilder.AddSignInManager<SignInManager<AppUser>>();

			services.TryAddSingleton<ISystemClock, SystemClock>();

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenKey"]));
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
					.AddJwtBearer(options =>
					{
						options.TokenValidationParameters = new TokenValidationParameters
						{
							ValidateIssuerSigningKey = true,
							IssuerSigningKey = key,
							ValidateAudience = false,
							ValidateIssuer = false
						};
					});

			services.AddScoped<IJwtGenerator, JwtGenerator>();

            services.AddScoped<IUserAccessor, UserAccessor>();

            // IdentityBuilder builder = services.AddIdentityCore<AppUser>(options =>
            // {
            //     options.Password.RequireDigit = false;
            //     options.Password.RequiredLength = 4;
            //     options.Password.RequireNonAlphanumeric = false;
            //     options.Password.RequiredUniqueChars = 0;
            //     options.Password.RequireUppercase = false;
            //     options.Password.RequireLowercase = true;
            // });

            // builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            // builder.AddEntityFrameworkStores<DataContext>();
            // builder.AddRoleValidator<RoleValidator<Role>>();
            // builder.AddRoleManager<RoleManager<Role>>();
            // builder.AddSignInManager<SignInManager<AppUser>>();

            //services.AddIdentity<User, Role>().AddEntityFrameworkStores<DataContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            
            if (env.IsDevelopment())
            {
                // app.UseDeveloperExceptionPage();
            }
            else
            {
                
            }

			//app.UseCors("CorsPolicy");
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            //app.UseHttpsRedirection();

            app.UseRouting();

			app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
