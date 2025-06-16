using System.Diagnostics;

namespace ProjetoFullstackGenerator.Geradores
{
    public static class DotnetEfGenerator
    {
        public static void Generate(string projectName)
        {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), projectName);
            string infraPath = Path.Combine(basePath, $"{projectName}.Infrastructure", "Data");
            Directory.CreateDirectory(infraPath);

            // AppDbContext.cs
            File.WriteAllText(Path.Combine(infraPath, "AppDbContext.cs"), $@"
            using Microsoft.EntityFrameworkCore;
            using {projectName}.Domain.Entities;

            namespace {projectName}.Infrastructure.Data
            {{
                public class AppDbContext : DbContext
                {{
                    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {{ }}

                    public DbSet<Pessoa> Pessoas {{ get; set; }} = null!;

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {{
                        modelBuilder.Entity<Pessoa>().HasKey(p => p.Id);
                        base.OnModelCreating(modelBuilder);
                    }}
                }}
            }}
            ");

            // PessoaRepository.cs
            File.WriteAllText(Path.Combine(infraPath, "PessoaRepository.cs"), $@"
            using {projectName}.Domain.Entities;
            using {projectName}.Application.Interfaces;
            using Microsoft.EntityFrameworkCore;

            namespace {projectName}.Infrastructure.Data
            {{
                public class PessoaRepository : IPessoaService
                {{
                    private readonly AppDbContext _context;

                    public PessoaRepository(AppDbContext context)
                    {{
                        _context = context;
                    }}

                    public async Task<IEnumerable<Pessoa>> GetAllAsync()
                    {{
                        return await _context.Pessoas.ToListAsync();
                    }}

                    public async Task<Pessoa?> GetByIdAsync(int id)
                    {{
                        return await _context.Pessoas.FindAsync(id);
                    }}

                    public async Task<Pessoa> CreateAsync(Pessoa pessoa)
                    {{
                        _context.Pessoas.Add(pessoa);
                        await _context.SaveChangesAsync();
                        return pessoa;
                    }}

                    public async Task<bool> UpdateAsync(Pessoa pessoa)
                    {{
                        var existing = await _context.Pessoas.FindAsync(pessoa.Id);
                        if (existing == null) return false;

                        existing.Nome = pessoa.Nome;
                        existing.Email = pessoa.Email;
                        existing.Senha = pessoa.Senha;

                        await _context.SaveChangesAsync();
                        return true;
                    }}

                    public async Task<bool> DeleteAsync(int id)
                    {{
                        var pessoa = await _context.Pessoas.FindAsync(id);
                        if (pessoa == null) return false;

                        _context.Pessoas.Remove(pessoa);
                        await _context.SaveChangesAsync();
                        return true;
                    }}
                }}
            }}
            ");

            // Criação do appsettings.json
            string appSettingsPath = Path.Combine(basePath, $"{projectName}.WebApi", "appsettings.json");
            File.WriteAllText(appSettingsPath, $@"{{
            ""ConnectionStrings"": {{
                ""DefaultConnection"": ""Server=62.171.151.220;Database=db_{projectName};User Id=sa;Password=9{{7q[e<=;TrustServerCertificate=True;""
            }},
            ""Logging"": {{
                ""LogLevel"": {{
                ""Default"": ""Information"",
                ""Microsoft.AspNetCore"": ""Warning""
                }}
            }},
            ""AllowedHosts"": ""*""
            }}");

            // Caminho onde os comandos EF serão executados
            string efWorkingDir = Path.Combine(basePath, $"{projectName}.Infrastructure");

            // Roda migração e update
            Run("dotnet ef migrations add InitialCreate -s ../" + $"{projectName}.WebApi", efWorkingDir);
            Run("dotnet ef database update -s ../" + $"{projectName}.WebApi", efWorkingDir);

            void Run(string command, string workingDirectory)
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

            Console.WriteLine("\nDbContext e PessoaRepository gerados com sucesso!");
        }
    }
}
