using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class DbContextClass(DbContextOptions<DbContextClass> options) : DbContext(options)
{
    private static string? _connectionString;

    // Dynamically istanciate to the DbSet all entities retrieved from the GetTypesFromNamespace function
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entitiesTypeList = GetTypeHelper.GetTypesFromNamespace("Domain.Entities");

        foreach (var entityType in entitiesTypeList)
            modelBuilder.Entity(entityType);
    }

    // Setup Sql connection
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Ensure the connection string is retrieved before using it
            LoadConnectionStringFromSecretStorage();

            if (!string.IsNullOrEmpty(_connectionString))
                optionsBuilder.UseSqlServer(_connectionString);
        }
    }

    private static void LoadConnectionStringFromSecretStorage()
    {
        // Using Task.Run to retrieve the secret connection string from the Dapr SecretStorage to ensure security
        _connectionString ??= Task.Run(() => DaprSecretHelper.RetrieveSecretAsync("connectionString")).GetAwaiter().GetResult();
    }
}
