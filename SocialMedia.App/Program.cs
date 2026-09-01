using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SocialMedia.App.Middlewares;
using SocialMedia.Data.Database;
using SocialMedia.Data.Repository;
using SocialMedia.Domain.Entities;
using SocialMedia.Logic.Logics;
using SocialMedia.Logic.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<SocialMediaDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration["ConnectionStrings:SocialMediaDb"]);
});

// Repositories
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostLogic, PostLogic>();
builder.Services.AddScoped<ICommentLogic, CommentLogic>();
builder.Services.AddScoped<IAuthLogic, AuthLogic>();
builder.Services.AddScoped<IUserLogic, UserLogic>();

// Services
builder.Services.AddSingleton<IFileLogger, FileLogger>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddTransient<ITokenGenerator, TokenGenerator>();
builder.Services.AddTransient<IImageValidator, ImageValidator>();

// Identity configuration
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireNonAlphanumeric = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequiredLength = 6;
    opt.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SocialMediaDbContext>()
    .AddDefaultTokenProviders();

// Authentication
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = "socialmedia.com",
        ValidIssuer = "socialmedia.com",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] 
                                                                           ?? throw new Exception("jwt:key not found in appsettings")))
    };
});

// OpenAPI config (authorization lock icon)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Copy JWT token here (without 'Bearer' prefix)."
        };
        
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                []
            }
        });

        return Task.CompletedTask;
    });
});

// Etc.
builder.Services.AddControllers();
builder.Services.AddRouting();

var app = builder.Build();

// For uploading static images
app.UseStaticFiles();

// OpenAPI, SwaggerUI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Social Media API");
    });
}

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Middlewares
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();