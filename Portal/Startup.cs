using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;
using Application;
using Portal.Middlewares;
using Core.Entities;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Core.Interfaces;
using Infrastructure.Security;
using Core.Models;
using Portal.Models;
using LinqKit;

namespace Portal
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
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyMethod()
                        .AllowAnyHeader().AllowAnyOrigin();
                });
            });

            services.AddHttpContextAccessor();
            services.AddOptions();

            services.AddControllers()
                .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
            });


            services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).WithExpressionExpanding());

            services.AddIdentity<AppUserEntity, RolesEntity>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            var jwtSettings = Configuration.GetSection("JWT").Get<JwtModel>();
            services.AddSingleton<JwtModel>(jwtSettings);


            var tokenvalidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = false,
            };

            services.AddIdentityCore<AppUserEntity>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
            })
           .AddSignInManager<SignInManager<AppUserEntity>>();

            services.AddSingleton(tokenvalidationParameters);

            services.AddAuthentication((opt) =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = tokenvalidationParameters;
            });
            var mailSettings = Configuration.GetSection(nameof(MailSettings)).Get<MailSettings>();
            services.AddSingleton<MailSettings>(mailSettings);

            services.AddHttpContextAccessor();
            services.AddAuthorization();

            services.AddAutofac();

        }
        public void ConfigureContainer(ContainerBuilder builder)
        {

           // builder.RegisterType<AppDbContext>()
           //.WithParameter("options", new DbContextOptionsBuilder<AppDbContext>()
           //    .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
           //    .Options)
           //.InstancePerLifetimeScope();

            builder.RegisterType<Authorization>()
                .As<IAuthorizationInterface>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(System.Reflection.Assembly.Load(nameof(Application)))
                .Where(t => typeof(IInjectableSelfInstance).IsAssignableFrom(t))
                .AsSelf();

            builder.Register(x => Configuration.GetSection(nameof(MailSettings))
            .Get<MailSettings>()).SingleInstance().AsSelf();

        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAppExceptionHandler();
            app.UseAppAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
