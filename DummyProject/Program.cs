var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(2); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});
builder.Services.AddAuthentication("MyCookieAuthenticationScheme")
    .AddCookie("MyCookieAuthenticationScheme", options =>
    {
        options.LoginPath = "/Dummy/Login"; // Redirect here when authentication is required
        options.LogoutPath = "/Dummy/Logout"; // Redirect here after logout
        options.ExpireTimeSpan = TimeSpan.FromMinutes(2); // Set expiration time for cookies
        options.SlidingExpiration = true; // Refresh cookie expiration on each request
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dummy}/{action=Login}/{id?}");

app.Run();
