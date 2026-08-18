using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using otr_backend.Data;
using otr_backend.Enums;
using otr_backend.Models;
using otr_backend.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OtrDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

const string FrontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddDataProtection();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<ManagedUsersService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });

builder.Services.AddAuthorization();

// UseStaticFiles() needs wwwroot to physically exist before the app starts serving.
Directory.CreateDirectory(Path.Combine(builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot"), "profile-pictures"));

var app = builder.Build();

// Ensure the Superadmin role exists with full permissions before serving any requests.
// The first person to register gets assigned to it (see AuthController.Register).
// Deferred to ApplicationStarted (rather than run inline here) because the
// build-time OpenAPI doc generator also executes this file up to app.Build() to
// reflect over routes, without a real database connection available.
app.Lifetime.ApplicationStarted.Register(() => _ = SeedSuperadminRoleAsync(app.Services));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedSuperadminRoleAsync(IServiceProvider services)
{
    using IServiceScope scope = services.CreateScope();
    OtrDbContext context = scope.ServiceProvider.GetRequiredService<OtrDbContext>();

    Role? superadmin = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Superadmin");
    if (superadmin == null)
    {
        superadmin = new Role
        {
            RoleName = "Superadmin",
            RoleDisplayName = "Superadmin",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
        context.Roles.Add(superadmin);
        await context.SaveChangesAsync();
    }

    // Backfill only — never touches a resource Superadmin already has a row for, so a
    // deliberately-lowered grant survives restarts. This just makes sure that whenever
    // PermissionResource grows, Superadmin doesn't fall behind on the new value.
    List<PermissionResource> grantedResources = await context.RolePermissionMappings
        .Where(m => m.RoleId == superadmin.Id)
        .Select(m => m.Resource)
        .ToListAsync();

    foreach (PermissionResource resource in Enum.GetValues<PermissionResource>())
    {
        if (grantedResources.Contains(resource))
        {
            continue;
        }

        context.RolePermissionMappings.Add(new RolePermissionMapping
        {
            RoleId = superadmin.Id,
            Resource = resource,
            Level = PermissionLevel.Admin,
        });
    }

    await context.SaveChangesAsync();
}
