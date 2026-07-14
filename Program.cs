using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext (use existing connection string)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=DAMS;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// HttpContext accessor (used by viewcomponents)
builder.Services.AddHttpContextAccessor();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Seed roles if missing
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Apply any pending migrations so the database schema is created/updated
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log to console; in production use proper logging
        Console.WriteLine("Database migration failed: " + ex.Message);
    }

    if (!db.Roles.Any())
    {
        db.Roles.AddRange(new Role { Id = 1, RoleName = "Admin" }, new Role { Id = 2, RoleName = "Doctor" }, new Role { Id = 3, RoleName = "Patient" });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();