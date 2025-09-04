using System.CommandLine;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions
{
    internal static class CommandOptionsExtensions
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

        public static Option<bool> CreateBoolOption(
            this Command command,
            string longName,
            string shortName,
            string description,
            bool defaultValue = false)
        {
            var option = new Option<bool>(
                [$"--{longName}", $"-{shortName}"],
                description)
            {
                IsRequired = false,
                Arity = ArgumentArity.Zero,
            };

            option.SetDefaultValue(defaultValue);
            command.AddOption(option);
            return option;
        }
    }
}