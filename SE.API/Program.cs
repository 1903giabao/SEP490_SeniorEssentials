using System.Text;
using AutoMapper;
using Google.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SE.Common.DTO;
using SE.Common.Mapper;
using SE.Common.Setting;
using SE.Data.UnitOfWork;
using SE.Service.Services;

var builder = WebApplication.CreateBuilder(args);


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



var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new ApplicationMapper());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

string pathToServiceAccountKey = "D:/FPT/Term 9/Do an/Project/testproject-bc2e2-firebase-adminsdk-lqlxd-9709c02fcf.json";
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", pathToServiceAccountKey);

var jwtSettings =  builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(val =>
{
    val.Key = jwtSettings.Key;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMvc();
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
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
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
