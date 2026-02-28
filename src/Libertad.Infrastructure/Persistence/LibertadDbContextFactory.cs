using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace Libertad.Infrastructure.Persistence;

public class LibertadDbContextFactory : IDesignTimeDbContextFactory<LibertadDbContext>
{
    public LibertadDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Libertad.Api");
        
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);

        var userSecretsId = "9f84a654-1bee-4b09-a658-5a40a9c02b33";
        var userSecretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            userSecretsId);

        if (Directory.Exists(userSecretsPath))
        {
            var secretsFile = Path.Combine(userSecretsPath, "secrets.json");
            configBuilder.AddJsonFile(secretsFile, optional: true, reloadOnChange: false);
        }

        configBuilder.AddEnvironmentVariables();
        
        var configuration = configBuilder.Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in configuration.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<LibertadDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LibertadDbContext(optionsBuilder.Options);
    }
}