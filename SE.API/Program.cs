using System.Text;
using AutoMapper;
using dotenv.net;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SE.API.ConfigModel;
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


DotEnv.Load();

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


/*var fcmConfig = new FCMConfigModel
{
    Type = "service_account",
    ProjectId = "test-sep-27039",
    PrivateKeyId = "a1a0e47cd9d6fb18d70542270b48f3787d5a36ff",
    PrivateKey = "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCyImG5RKgCEHwE\nFIFui4PDikxib9FoevRt/6pLHUiVWCSqsFMag94Q/V47rdmx0dcMr7KWkUQOEEYu\n6cHui0JGNiP6y0tA0UDKVy14MtNQf4aHxwjOjZDOpeJ/jaW2+uwOlz/eb+SGedEP\nXD0KvCuS8aQZgN5PtDVcIvRyi8JeSi7lnxYPlDWntMmDIZdUPWJEGdee8SIXXzCk\nmXxcNfC5GHI882+i0Czk2zVvSGS0fMMqjcdFlFyRmAzVnJE04PeWcERck8wcHy0j\n4Gfz0n/VxRsBDHO5OmJg1H+uaLLkX8jhnOZ/3y72yTM83nYrFIvbRHAcAtU2OZsj\nj1X+zPyzAgMBAAECggEABLzTWokP6tYjZjxhYN5HB9lSxL9yk1PJ0m8dSVgjCQ3K\nE9wSqb7eFheW/QFXq9oH3SeDGWwNok4ef7rp1H1RqupftJjZjITEM110MSEw65Ao\ntM2/Vzb+pfBVgMz0nlQ4GP8+zJyvOEBfJghu+y0b/5F7qs35m6cQUD4BwFYlOjq1\ndZDQVzepuzxCmRIEW8iIeohEyuzFXvITBKk8YrOQgTbP6XFKumqI1k1EwL5b3Bve\nThspx2nnDKv0LLmNxp0CE6jXHqvjIe1Zsqkq1kZCbXfyuxPcVGfDD73Lyh3XlkTM\nqj5E1de6kqgEp5si/qX85r2ho3cEeWZPfUpzrXXRIQKBgQDnI6kPw+9IFACrxoQs\nKv+6bkLWZGg8yoMpWCTRXwRpgD6KsLJFghqoNf8t2RRfUqj7XXKASu4GOzekLD5M\nCUzJR48fOKuAnnGACrZtW7GetwB8ylh5ZaUhBXx6/gGPh+IJiIq44iPNy3ggFNYO\nj0j83wF1ic6WEFmhB2xenvKaYwKBgQDFSz9T/KA8UOCkqipal//IlBPo0bs60nLf\nY1SkiSvFFpVL0yoUlI2tr0jayeLbv5KCCkitjwnZaS0GmPVURgw4yPF6/GXA3rDQ\nyW5+3LQ8qYIOIJXPG1EvN6yvqY3PL8j0zbKy9et+MYuhrbfUeSK4cYj7SpSavE4A\nVk6XoEH9cQKBgBGYcF1H4CZPh4GMGjG2kEMj86iYeirui6+RCzR5FD/nyFsMenW6\nIsddXPCjjt52z3BbO8Uybw5AYcr4p0Foj9TewrFwwfWHmkJSDnMiwNHBQqM9UCDl\nsP1jiodeYMYJZRaus0jBxlH7REjE7Uqsc7T0UQsek4Bu/DO6+e/2D6fbAoGBALH0\nem2d+0YMSWQdXNCUM5HPBtpEetXGxvh5lvpGA+Xkxcs778PabqSP6231FZvSgyqq\nbf2mfGLO/F7sDrTx7co2baHaEUnTU7cvSWxCVIw29OkbOSUy5ZpqZGeZzyBnYKJ2\n+01yhfQwalrt31dV4Bxvw/etwLaFTPH+5yra0UrxAoGAXcVx+iB0Fg1tD8zXs8xP\nquy0svRJlLitjnKZ3s8tiGP0E2ds14a6DPc1mQy23BURh/5lR8YBhV5EZ+4fEe4P\n85c5FZeoTXgcu/P/H6BHuYlc574j2lOVuG3p693IK1ik1XSZdZywF3y1grvJ11Bd\nmOUVFOZQWHeGR5+9pMz5XYM=\n-----END PRIVATE KEY-----\n",
    ClientEmail = "firebase-adminsdk-fbsvc@test-sep-27039.iam.gserviceaccount.com",
    ClientId = "111830615134820002851",
    AuthUri = "https://accounts.google.com/o/oauth2/auth",
    TokenUri = "https://oauth2.googleapis.com/token",
    AuthProviderx509CertUrl = "https://www.googleapis.com/oauth2/v1/certs",
    Clientx509CertUrl = "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-fbsvc%40test-sep-27039.iam.gserviceaccount.com",
    UniverseDomain = "googleapis.com"
};*/
/*var fcmClient = new FirestoreClientBuilder
{
    GoogleCredential = fcmCredential
}.Build();
*/
/*
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
    }}");*/
FirebaseApp.Create(new AppOptions()
{
    Credential = fcmCredential,
    ProjectId = fcmConfig.ProjectId
});

//builder.Services.AddSingleton(FirestoreDb.Create(fcmConfig.ProjectId, fcmClient));

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