using System.Reflection;
using DbUp;
using Serilog;

namespace BoraAli.Api.Extensions;

/// <summary>
/// Inicializador do banco de dados usando DbUp para migrations versionadas.
/// Substitui a execução manual de scripts SQL estáticos.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Executa as migrations automaticamente ao iniciar a aplicação.
    /// Os scripts SQL devem estar como Embedded Resource na pasta Migrations/.
    /// </summary>
    /// <param name="connectionString">String de conexão com o SQLite</param>
    public static void RunMigrations(string connectionString)
    {
        Log.Information("===== DbUp: Iniciando migrations do banco de dados =====");

        // SQLite cria o arquivo .db automaticamente na primeira conexão.
        // Garantimos que o diretório existe para evitar erros de path.
        var dataSource = connectionString.Split('=').LastOrDefault()?.Trim() ?? "BoraAli.db";
        var dbDirectory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        var upgrader = DeployChanges.To
            .SqliteDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                scriptName => scriptName.StartsWith("BoraAli.Api.Migrations.Script"))
            .WithTransactionPerScript()
            .LogToConsole()
            .WithVariablesDisabled()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Log.Fatal(result.Error, "DbUp: Falha crítica ao executar as migrations");
            throw new InvalidOperationException(
                "Falha ao executar as migrations do banco de dados. " +
                "Verifique a integridade dos scripts SQL e a conexão com o banco.",
                result.Error);
        }

        Log.Information("===== DbUp: {Count} scripts executados com sucesso =====", result.Scripts.Count());
        Log.Information("===== DbUp: Banco de dados pronto para uso =====");
    }
}
