using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UsersContacts.AppDbContext;
using UsersContacts.Interfaces;
using UsersContacts.Modules.Contacts.DTOs;
using UsersContacts.Modules.Contacts.Validators;
using UsersContacts.Persistence;

namespace UsersContacts
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, 
            string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddControllersWithViews();
            services.AddScoped<IValidator<ContactDto>, ContactDtoValidator>();

            return services;
        }

        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/Login";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                });

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSignalR();
            return services;
        }
    }
}