using System.CommandLine;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions
{
    public static class CommandOptionsExtensions
    {
        public static Option<string> CreateStringOption(
            this Command command,
            string longName,
            string shortName,
            string description,
            bool isRequired = false)
        {
            var option = new Option<string>(
                [$"--{longName}", $"-{shortName}"],
                description)
            {
                IsRequired = isRequired,
                Arity = ArgumentArity.ExactlyOne
            };

            if (isRequired)
            {
                option.AddValidator(result =>
                {
                    var value = result.GetValueOrDefault<string>();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        result.ErrorMessage =
                            $"Required parameter is missing or empty. Use --{longName} / -{shortName}.";
                    }
                });
            }

            command.AddOption(option);
            return option;
        }
    }
}