using System.Text;
using AutoMapper;
using dotenv.net;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SE.API.ConfigModel;
using SE.Common.Mapper;
using SE.Common.Setting;
using SE.Data.UnitOfWork;
using SE.Service.BackgroundWorkers;
using SE.Service.Helper;
using SE.Service.Services;
using Serilog;

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
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IIotDeviceService, IotDeviceService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IEmergencyContactService, EmergencyContactService>();
builder.Services.AddScoped<IUserLinkService, UserLinkService>();
builder.Services.AddScoped<IHealthIndicatorService, HealthIndicatorService>();
builder.Services.AddScoped<IVideoCallService, VideoCallService>();
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<ISmsService, SmsService>();

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() // Log to console
            .WriteTo.File(@"E:\SEP490\worker_logs.txt", rollingInterval: RollingInterval.Day) // Log to file
            .CreateLogger();

builder.Host.UseSerilog(); 

builder.Services.AddHostedService<Worker>();



DotEnv.Load();


builder.Services.AddSingleton(provider =>
{
    var gglvConfig = new GGCloudVisionApiConfigModel();

    // Create GoogleCredential from JSON string using the config data
    var jsonCredential = $@"
    {{
        ""type"": ""{gglvConfig.Type}"",
        ""project_id"": ""{gglvConfig.ProjectId}"",
        ""private_key_id"": ""{gglvConfig.PrivateKeyId}"",
        ""private_key"": ""{gglvConfig.PrivateKey}"",
        ""client_email"": ""{gglvConfig.ClientEmail}"",
        ""client_id"": ""{gglvConfig.ClientId}"",
        ""auth_uri"": ""{gglvConfig.AuthUri}"",
        ""token_uri"": ""{gglvConfig.TokenUri}"",
        ""auth_provider_x509_cert_url"": ""{gglvConfig.AuthProviderx509CertUrl}"",
        ""client_x509_cert_url"": ""{gglvConfig.Clientx509CertUrl}"",
        ""universe_domain"": ""{gglvConfig.UniverseDomain}""
    }}";

    var credential = GoogleCredential.FromJson(jsonCredential);
    return credential.CreateScoped(ImageAnnotatorClient.DefaultScopes);
});

var fcmConfig = new FCMConfigModel();

var fcmCredential = GoogleCredential.FromJson($@"
    {{
        ""type"": ""{fcmConfig.Type}"",
        ""project_id"": ""{fcmConfig.ProjectId}"",
        ""private_key_id"": ""{fcmConfig.PrivateKeyId}"",
        ""private_key"": ""{fcmConfig.PrivateKey}"",
        ""client_email"": ""{fcmConfig.ClientEmail}"",
        ""client_id"": ""{fcmConfig.ClientId}"",
        ""auth_uri"": ""{fcmConfig.AuthUri}"",
        ""token_uri"": ""{fcmConfig.TokenUri}"",
        ""auth_provider_x509_cert_url"": ""{fcmConfig.AuthProviderx509CertUrl}"",
        ""client_x509_cert_url"": ""{fcmConfig.Clientx509CertUrl}"",
        ""universe_domain"": ""{fcmConfig.UniverseDomain}""
    }}");

var ggCloud = new GGCloudVisionApiConfigModel();



FirebaseApp.Create(new AppOptions()
{
    Credential = fcmCredential,
    ProjectId = fcmConfig.ProjectId
});


var firebase = new FirebaseConfigModel();

var tmp2 = GoogleCredential.FromJson($@"
                    {{
                        ""type"": ""service_account"",
                        ""project_id"": ""{firebase.ProjectId}"",
                        ""private_key_id"": ""{firebase.PrivateKeyId}"",
                        ""private_key"": ""{firebase.PrivateKey}"",
                        ""client_email"": ""{firebase.ClientEmail}"",
                        ""client_id"": ""{firebase.ClientId}"",
                        ""auth_uri"": ""{firebase.AuthUri}"",
                        ""token_uri"": ""{firebase.TokenUri}"",
                        ""auth_provider_x509_cert_url"": ""{firebase.AuthProviderx509CertUrl}"",
                        ""client_x509_cert_url"": ""{firebase.Clientx509CertUrl}"",
                        ""universe_domain"": ""{firebase.UniverseDomain}""
                    }}");
var channel = tmp2.ToChannelCredentials();
var client = new FirestoreClientBuilder{
    GoogleCredential= tmp2
    }.Build();

builder.Services.AddSingleton(FirestoreDb.Create(firebase.ProjectId, client));

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new ApplicationMapper());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.Configure<JwtSettings>(val =>
{
    val.Key = Environment.GetEnvironmentVariable("JwtSettings");
});

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JwtSettings"))),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(option =>
    option.AddPolicy("CORS", builder =>
        builder.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed((host) => true)));

var app = builder.Build();

app.UseSwagger(op => op.SerializeAsV2 = false);
app.UseSwaggerUI();
app.UseCors("CORS");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();