using System.Diagnostics;

namespace ProjetoFullstackGenerator.Geradores
{
    public static class DotnetBackendGenerator
    {
        public static void Generate(string projectName, string dotnetVersion)
        {
            Console.WriteLine($"\nGerando projeto backend .NET ({dotnetVersion}) com Clean Architecture...");

            string basePath = Path.Combine(Directory.GetCurrentDirectory(), projectName);
            Directory.CreateDirectory(basePath);

            void Run(string command, string? workingDirectory = null)
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
                        WorkingDirectory = workingDirectory ?? basePath
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine(output);
            }

            // Criação da solution
            Run($"dotnet new sln -n {projectName}");

            // Criação dos projetos
            Run($"dotnet new classlib -n {projectName}.Domain");
            Run($"dotnet new classlib -n {projectName}.Application");
            Run($"dotnet new classlib -n {projectName}.Infrastructure");
            Run($"dotnet new webapi -n {projectName}.WebApi");

            // Adiciona os projetos à solution
            Run($"dotnet sln add {projectName}.Domain/{projectName}.Domain.csproj");
            Run($"dotnet sln add {projectName}.Application/{projectName}.Application.csproj");
            Run($"dotnet sln add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj");
            Run($"dotnet sln add {projectName}.WebApi/{projectName}.WebApi.csproj");

            // Adiciona referências entre os projetos
            Run($"dotnet add {projectName}.Application/{projectName}.Application.csproj reference ../{projectName}.Domain/{projectName}.Domain.csproj", Path.Combine(basePath, projectName + ".Application"));
            Run($"dotnet add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj reference ../{projectName}.Application/{projectName}.Application.csproj", Path.Combine(basePath, projectName + ".Infrastructure"));
            Run($"dotnet add {projectName}.Infrastructure/{projectName}.Infrastructure.csproj reference ../{projectName}.Domain/{projectName}.Domain.csproj", Path.Combine(basePath, projectName + ".Infrastructure"));
            Run($"dotnet add {projectName}.WebApi/{projectName}.WebApi.csproj reference ../{projectName}.Application/{projectName}.Application.csproj", Path.Combine(basePath, projectName + ".WebApi"));
            Run($"dotnet add {projectName}.WebApi/{projectName}.WebApi.csproj reference ../{projectName}.Infrastructure/{projectName}.Infrastructure.csproj", Path.Combine(basePath, projectName + ".WebApi"));

            Console.WriteLine("\nProjeto backend gerado com sucesso!");
        }
    }
}
