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

            command.Options.Add(option);
            return option;
        }

        public static Option<bool> CreateBoolOption(
            this Command command,
            string longName,
            string shortName,
            string description)
        {
            var option = new Option<bool>(
                name: $"--{longName}"
            )
            {
                Description = description,
                Required = false
            };

            option.Aliases.Add($"-{shortName}");
            option.DefaultValueFactory = _ => false;

            command.Options.Add(option);
            return option;
        }
    }
}