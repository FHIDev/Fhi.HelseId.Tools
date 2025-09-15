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
                name: $"--{longName}"
            )
            {
                Description = description,
                Required = isRequired
            };

            option.Aliases.Add($"-{shortName}");

            if (isRequired)
            {
                option.Validators.Add(result =>
                {
                    var value = result.GetValueOrDefault<string>();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        result.AddError($"Required parameter is missing or empty. Use --{longName} / -{shortName}.");
                    }
                });
            }

            command.Options.Add(option);
            return option;
        }

        public static Option<bool> CreateBoolOption(
            this Command command,
            string longName,
            string shortName,
            string description
            /*bool defaultValue = false*/)
        {
            var option = new Option<bool>(
                name: $"--{longName}"
            )
            {
                Description = description,
                Required = false
            };

            option.Aliases.Add($"-{shortName}");
            // TODO: how to do this inversion beta5 ?
            //option.SetDefaultValue(defaultValue);

            command.Options.Add(option);
            return option;
        }
    }
}