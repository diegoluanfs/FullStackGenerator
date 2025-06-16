using System.Diagnostics;

namespace ProjetoFullstackGenerator.Geradores
{
    public static class DotnetBackendConfigurator
    {
        public static void Configure(string projectName, string dotnetVersion)
        {
            Console.WriteLine("\nConfigurando Entity Framework Core, Swagger e referências entre projetos...");

            string basePath = Path.Combine(Directory.GetCurrentDirectory(), projectName);
            string webApiPath = Path.Combine(basePath, $"{projectName}.WebApi");
            string infrastructurePath = Path.Combine(basePath, $"{projectName}.Infrastructure");
            string applicationPath = Path.Combine(basePath, $"{projectName}.Application");
            string domainPath = Path.Combine(basePath, $"{projectName}.Domain");

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

            // Instalação dos pacotes necessários no WebApi
            Run("dotnet add package Microsoft.EntityFrameworkCore", webApiPath);
            Run("dotnet add package Microsoft.EntityFrameworkCore.SqlServer", webApiPath);
            Run("dotnet add package Microsoft.EntityFrameworkCore.Tools", webApiPath);
            Run("dotnet add package Swashbuckle.AspNetCore", webApiPath);

            // Instalação dos pacotes no Infrastructure
            Run("dotnet add package Microsoft.EntityFrameworkCore", infrastructurePath);
            Run("dotnet add package Microsoft.EntityFrameworkCore.SqlServer", infrastructurePath);
            Run("dotnet add package Microsoft.EntityFrameworkCore.Design", infrastructurePath);

            // Substituir Program.cs
            string programPath = Path.Combine(webApiPath, "Program.cs");
            File.WriteAllText(programPath, $@"using {projectName}.Application.Interfaces;
            using {projectName}.Application.Services;
            using {projectName}.Infrastructure.Data;
            using Microsoft.EntityFrameworkCore;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString(""DefaultConnection"")));

            // Dependency Injection
            builder.Services.AddScoped<IPessoaService, PessoaService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {{
                app.UseSwagger();
                app.UseSwaggerUI();
            }}

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
            ");

            // Adiciona referências entre projetos usando caminhos absolutos
            string appProj = Path.Combine(applicationPath, $"{projectName}.Application.csproj");
            string infraProj = Path.Combine(infrastructurePath, $"{projectName}.Infrastructure.csproj");
            string domainProj = Path.Combine(domainPath, $"{projectName}.Domain.csproj");
            string webApiProj = Path.Combine(webApiPath, $"{projectName}.WebApi.csproj");

            Run($@"dotnet add ""{appProj}"" reference ""{domainProj}""");
            Run($@"dotnet add ""{infraProj}"" reference ""{domainProj}""");
            Run($@"dotnet add ""{infraProj}"" reference ""{applicationPath}""");
            Run($@"dotnet add ""{webApiProj}"" reference ""{appProj}""");
            Run($@"dotnet add ""{webApiProj}"" reference ""{infraProj}""");
            Run($@"dotnet add ""{webApiProj}"" reference ""{domainProj}""");

            Console.WriteLine("\nConfiguração concluída com sucesso!");
        }
    }
}
