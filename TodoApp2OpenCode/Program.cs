using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using TodoApp2OpenCode.Components;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with factory for concurrent access
builder.Services.AddDbContextFactory<FlowBoardDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<TodoService>();
builder.Services.AddSingleton<BottomNavService>();
builder.Services.AddScoped<TestConnectionService>();

// Auth Service - choose implementation:
// Production (database): IAuthService → AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Testing (localStorage): IAuthService → LocalStorageAuthService
// builder.Services.AddScoped<IAuthService, LocalStorageAuthService>();

// Board Service - choose implementation:
// Production (database): IBoardService → BoardService
builder.Services.AddScoped<IBoardService, BoardService>();

// Testing (localStorage): IBoardService → LocalStorageBoardService
// builder.Services.AddScoped<IBoardService, LocalStorageBoardService>();

// Log Service - choose implementation:
// Production (database): ILogService → LogService
builder.Services.AddScoped<ILogService, LogService>();

// Testing (localStorage): ILogService → LocalStorageLogService
// builder.Services.AddScoped<ILogService, LocalStorageLogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
