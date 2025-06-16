using System.IO;

namespace ProjetoFullstackGenerator.Geradores
{
    public static class DotnetCrudPessoaGenerator
    {
        public static void Generate(string projectName)
        {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), projectName);

            // DOMAIN - Pessoa.cs
            string domainPath = Path.Combine(basePath, $"{projectName}.Domain", "Entities");
            Directory.CreateDirectory(domainPath);
            File.WriteAllText(Path.Combine(domainPath, "Pessoa.cs"), @$"
            using {projectName}.Domain.Entities;
            
            namespace {projectName}.Domain.Entities
            {{
                public class Pessoa
                {{
                    public int Id {{ get; set; }}
                    public string Nome {{ get; set; }} = string.Empty;
                    public string Email {{ get; set; }} = string.Empty;
                    public string Senha {{ get; set; }} = string.Empty;
                }}
            }}");


            // APPLICATION - Interface IPessoaService
            string appInterfacesPath = Path.Combine(basePath, $"{projectName}.Application", "Interfaces");
            Directory.CreateDirectory(appInterfacesPath);
            File.WriteAllText(Path.Combine(appInterfacesPath, "IPessoaService.cs"), @$"
            using {projectName}.Domain.Entities;

            namespace {projectName}.Application.Interfaces
            {{
                public interface IPessoaService
                {{
                    Task<IEnumerable<Pessoa>> GetAllAsync();
                    Task<Pessoa?> GetByIdAsync(int id);
                    Task<Pessoa> CreateAsync(Pessoa pessoa);
                    Task<bool> UpdateAsync(Pessoa pessoa);
                    Task<bool> DeleteAsync(int id);
                }}
            }}
            ");

            // APPLICATION - PessoaService
            string appServicesPath = Path.Combine(basePath, $"{projectName}.Application", "Services");
            Directory.CreateDirectory(appServicesPath);
            File.WriteAllText(Path.Combine(appServicesPath, "PessoaService.cs"), @$"
            using {projectName}.Domain.Entities;
            using {projectName}.Application.Interfaces;

            namespace {projectName}.Application.Services
            {{
                public class PessoaService : IPessoaService
                {{
                    private readonly List<Pessoa> _pessoas = new(); // Simulação de repositório

                    public Task<IEnumerable<Pessoa>> GetAllAsync() =>
                        Task.FromResult(_pessoas.AsEnumerable());

                    public Task<Pessoa?> GetByIdAsync(int id) =>
                        Task.FromResult(_pessoas.FirstOrDefault(p => p.Id == id));

                    public Task<Pessoa> CreateAsync(Pessoa pessoa)
                    {{
                        pessoa.Id = _pessoas.Count + 1;
                        _pessoas.Add(pessoa);
                        return Task.FromResult(pessoa);
                    }}

                    public Task<bool> UpdateAsync(Pessoa pessoa)
                    {{
                        var existing = _pessoas.FirstOrDefault(p => p.Id == pessoa.Id);
                        if (existing == null) return Task.FromResult(false);

                        existing.Nome = pessoa.Nome;
                        existing.Email = pessoa.Email;
                        existing.Senha = pessoa.Senha;
                        return Task.FromResult(true);
                    }}

                    public Task<bool> DeleteAsync(int id)
                    {{
                        var pessoa = _pessoas.FirstOrDefault(p => p.Id == id);
                        if (pessoa == null) return Task.FromResult(false);

                        _pessoas.Remove(pessoa);
                        return Task.FromResult(true);
                    }}
                }}
            }}
            ");

            // WEBAPI - PessoaController
            string controllerPath = Path.Combine(basePath, $"{projectName}.WebApi", "Controllers");
            Directory.CreateDirectory(controllerPath);
            File.WriteAllText(Path.Combine(controllerPath, "PessoaController.cs"), @$"
            using Microsoft.AspNetCore.Mvc;
            using {projectName}.Application.Interfaces;
            using {projectName}.Domain.Entities;

            namespace {projectName}.WebApi.Controllers
            {{
                [ApiController]
                [Route(""api/[controller]"")]
                public class PessoaController : ControllerBase
                {{
                    private readonly IPessoaService _service;

                    public PessoaController(IPessoaService service)
                    {{
                        _service = service;
                    }}

                    [HttpGet]
                    public async Task<IActionResult> GetAll() =>
                        Ok(await _service.GetAllAsync());

                    [HttpGet(""{{id}}"")]
                    public async Task<IActionResult> GetById(int id)
                    {{
                        var pessoa = await _service.GetByIdAsync(id);
                        return pessoa == null ? NotFound() : Ok(pessoa);
                    }}

                    [HttpPost]
                    public async Task<IActionResult> Create(Pessoa pessoa) =>
                        Ok(await _service.CreateAsync(pessoa));

                    [HttpPut]
                    public async Task<IActionResult> Update(Pessoa pessoa)
                    {{
                        var updated = await _service.UpdateAsync(pessoa);
                        return updated ? Ok(pessoa) : NotFound();
                    }}

                    [HttpDelete(""{{id}}"")]
                    public async Task<IActionResult> Delete(int id)
                    {{
                        var deleted = await _service.DeleteAsync(id);
                        return deleted ? NoContent() : NotFound();
                    }}
                }}
            }}
            ");

            Console.WriteLine("\nCRUD da entidade Pessoa gerado com sucesso!");
        }
    }
}
