using Microsoft.EntityFrameworkCore;
using UsersContacts;
using UsersContacts.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddAuthenticationServices();
builder.Services.AddSignalRServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapHub<ContactHub>("/contactHub");

app.Run();
