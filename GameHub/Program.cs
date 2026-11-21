using GameHub.Components;
using ImposterGame.Services;
using Radzen;

namespace GameHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped<LocalizationService>();
            builder.Services.AddScoped<AudioHapticService>();
            builder.Services.AddScoped<WordService>();
            builder.Services.AddScoped<GameService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            // Serve Android Digital Asset Links for TWA (supports Azure App Settings override or wwwroot file)
            app.MapGet("/.well-known/assetlinks.json", (IConfiguration config, IWebHostEnvironment env) =>
            {
                var packageName = config["PWA:PackageName"];
                var fingerprintsRaw = config["PWA:Sha256Fingerprints"];

                if (!string.IsNullOrWhiteSpace(packageName) && !string.IsNullOrWhiteSpace(fingerprintsRaw))
                {
                    var fingerprints = fingerprintsRaw
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    var payload = new[]
                    {
                        new
                        {
                            relation = new[] { "delegate_permission/common.handle_all_urls" },
                            target = new
                            {
                                @namespace = "android_app",
                                package_name = packageName,
                                sha256_cert_fingerprints = fingerprints
                            }
                        }
                    };

                    return Results.Json(payload);
                }

                var filePath = Path.Combine(env.WebRootPath, ".well-known", "assetlinks.json");
                if (File.Exists(filePath))
                {
                    return Results.File(filePath, "application/json");
                }

                return Results.NotFound();
            });

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
