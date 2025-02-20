using System.Text;
using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SE.Common.DTO;
using SE.Common.Mapper;
using SE.Common.Setting;
using SE.Data.UnitOfWork;
using SE.Service.Helper;
using SE.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Email configuration
var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailSettings>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddTransient<EmailService>();

// Dependency injection for services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IProfessorScheduleService, ProfessorScheduleService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IComboService, ComboService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFamilyTieService, FamilyTieService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IHealthIndicatorService, HealthIndicatorService>();
builder.Services.AddScoped<IIotDeviceService, IotDeviceService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IEmergencyContactService, EmergencyContactService>();
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<ISmsService, SmsService>();


//var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "Configurations", "serviceAccountKey.json");
var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "Configurations", "serviceAccountTemp.json");
//var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "Configurations", "tempKey.json");
System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

// Initialize Firebase Admin SDK
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.GetApplicationDefault()
});

// Add Firestore DB Service (use Project ID)
//builder.Services.AddSingleton(FirestoreDb.Create("testproject-bc2e2")); //tempKey.json
//builder.Services.AddSingleton(FirestoreDb.Create("senioressentials-3ebc7")); //serviceAccountKey.json
builder.Services.AddSingleton(FirestoreDb.Create("senioressentials-94d8e")); //serviceAccountTemp.json

// AutoMapper configuration
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new ApplicationMapper());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// Add controllers and endpoints
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// JWT settings
var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(val =>
{
    val.Key = jwtSettings.Key;
});

// Swagger configuration
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Mock API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
    option.EnableAnnotations();
});

// Authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

// Authorization configuration
builder.Services.AddAuthorization();

// CORS configuration
builder.Services.AddCors(option =>
    option.AddPolicy("CORS", builder =>
        builder.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed((host) => true)));

var app = builder.Build();

// Middleware configuration
app.UseSwagger(op => op.SerializeAsV2 = false);
app.UseSwaggerUI();
app.UseCors("CORS");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();