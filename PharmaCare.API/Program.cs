using Microsoft.EntityFrameworkCore;
using PharmaCare.Data;
using PharmaCare.Data;
using PharmaCare.Data.Repositories.Implementations;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Implementations;
using PharmaCare.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
// Add other repositories as you create them

// Services (Business Logic)
builder.Services.AddScoped<IPatientService, PatientService>();
// Add other services as you create them

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC",
        policy =>
        {
            policy.WithOrigins("https://localhost:7XXX") // Your MVC URL
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowMVC");
app.UseAuthorization();
app.MapControllers();

app.Run();
 