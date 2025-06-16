using ProjetoFullstackGenerator.Geradores;

namespace ProjetoFullstackGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("=== Gerador de Projeto Fullstack (.NET + Flutter/React) ===\n");

            Console.Write("Informe o nome do projeto: ");
            string? projectName = Console.ReadLine()?.Trim();

            string? dotnetVersion = null;
            while (dotnetVersion == null)
            {
                Console.Write("Informe a versão do .NET (ex: 6, 6.0, 6,0, 7, 8, 9): ");
                string? input = Console.ReadLine()?.Trim();

                dotnetVersion = NormalizarVersaoDotNet(input);

                if (dotnetVersion == null)
                    Console.WriteLine("⚠️ Versão inválida. Tente novamente.");
            }

            DotnetBackendGenerator.Generate(projectName!, dotnetVersion);
            DotnetBackendConfigurator.Configure(projectName, dotnetVersion);
            DotnetCrudPessoaGenerator.Generate(projectName);
            DotnetEfGenerator.Generate(projectName);
            EfMigrationRunner.Run(projectName);

        }

        static string? NormalizarVersaoDotNet(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Replace(',', '.');

            if (System.Text.RegularExpressions.Regex.IsMatch(input, @"^\d+(\.\d+)?$"))
            {
                if (input.StartsWith("6")) return "6";
                if (input.StartsWith("7")) return "7";
                if (input.StartsWith("8")) return "8";
                if (input.StartsWith("9")) return "9";
            }

            return null;
        }
    }
}
