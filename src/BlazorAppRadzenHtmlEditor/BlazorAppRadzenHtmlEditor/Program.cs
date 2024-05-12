using BlazorAppRadzenHtmlEditor.Components;
using BlazorAppRadzenHtmlEditor.Data;
using BlazorAppRadzenHtmlEditor.Services;
using BlazorAppRadzenHtmlEditor.ViewModels;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        // Add services to the container.
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
           options.UseInMemoryDatabase("ConnectionInMemory")
        );
        builder.Services.AddScoped<SeedData>();
        builder.Services.AddScoped<BlogPostService>();

        // Radzen Services
        builder.Services.AddScoped<DialogService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<TooltipService>();
        builder.Services.AddScoped<ContextMenuService>();

        // Add mapster mapper
        builder.Services.AddMapster();

        builder.Services.CreateDatabase().GetAwaiter().GetResult();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = long.MaxValue;
        });
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }


        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapControllers();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}

public static class ServiceCollectionExtensions
{
    public static async Task CreateDatabase(this IServiceCollection services)
    {
        using (IServiceScope tmp = services.BuildServiceProvider().CreateScope())
        {
            await using var _context = tmp.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var seedData = tmp.ServiceProvider.GetRequiredService<SeedData>();
            await seedData.CreateInitialData();
        }
    }
}

public static class MapsterConfiguration
{
    public static void AddMapster(this IServiceCollection services)
    {
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        Assembly applicationAssembly = typeof(BaseViewModel<,>).Assembly;
        typeAdapterConfig.Scan(applicationAssembly);

        var mapperConfig = new Mapper(typeAdapterConfig);
        services.AddSingleton<IMapper>(mapperConfig);
    }
}
