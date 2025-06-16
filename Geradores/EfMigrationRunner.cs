using System.Diagnostics;

namespace ProjetoFullstackGenerator.Geradores
{
    public static class EfMigrationRunner
    {
        public static void Run(string projectName)
        {
            Console.WriteLine("\n🔧 Executando migrações do Entity Framework...");

            string basePath = Path.Combine(Directory.GetCurrentDirectory(), projectName);
            string infrastructurePath = Path.Combine(basePath, $"{projectName}.Infrastructure");
            string webApiPath = Path.Combine(basePath, $"{projectName}.WebApi");

            void RunCommand(string command, string workingDirectory)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {command}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = workingDirectory
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine(output);
            }

            // Gera a migration no projeto Infrastructure, mas com startup no WebApi
            RunCommand($"dotnet ef migrations add InitialCreate -s \"{webApiPath}\"", infrastructurePath);

            // Aplica a migration no banco
            RunCommand($"dotnet ef database update -s \"{webApiPath}\"", infrastructurePath);

            Console.WriteLine("✅ Migração aplicada com sucesso!");
        }
    }
}
