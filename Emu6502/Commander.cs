using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Emu6502;

public class Commander
{
    public Dictionary<string, (Type type, MethodInfo methodInfo, string commandDescription)> commands = new();

    public void RegisterCommandsInType(Type type)
    {
        var methodsInType = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        var commandMethods = methodsInType.Where(x => x.GetCustomAttribute<CommandAttribute>() is not null);
        foreach (var commandMethod in commandMethods)
        {
            var attribute = commandMethod.GetCustomAttribute<CommandAttribute>();
            commands[attribute.CommandName] = (type, commandMethod, attribute.Description);
        }
    }

    public void ExecuteCommand(string input)
    {
        var command = commands.Keys.FirstOrDefault(input.StartsWith);
        if (command is null)
            throw new ArgumentException("Unknown command");
        var argumentAttributes = commands[command].methodInfo.GetCustomAttributes<ArgumentAttribute>();
        var arguments = new List<object>();
        var argumentsFromInput = input.Replace(command, string.Empty).Trim();
        foreach (var argument in argumentAttributes)
        {
            if (string.IsNullOrWhiteSpace(argumentsFromInput))
                throw new CommandNotEnoughArgumentsException();
            switch (argument.Type)
            {
                case var x when x == typeof(ushort):
                    {
                        var currentArgument = argumentsFromInput.Split(' ', 2)[0];
                        if (argumentsFromInput.Split(' ', 1).Length > 1)
                            argumentsFromInput = argumentsFromInput.Split(' ', 2)[1];
                        if (ushort.TryParse(currentArgument, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                            arguments.Add(value);
                        else
                            throw new CommandArgumentException(currentArgument, typeof(ushort));
                        break;
                    }
                case var x when x == typeof(byte):
                    {
                        var currentArgument = argumentsFromInput.Split(' ', 2)[0];
                        if (argumentsFromInput.Split(' ', 1).Length > 1)
                            argumentsFromInput = argumentsFromInput.Split(' ', 2)[1];
                        if (byte.TryParse(currentArgument, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                            arguments.Add(value);
                        else
                            throw new CommandArgumentException(currentArgument, typeof(byte));
                        break;
                    }
                case var x when x == typeof(string):
                    {
                        var currentArgument = argumentsFromInput.Split(' ', 2)[0];
                        if (argumentsFromInput.Split(' ', 1).Length > 1)
                            argumentsFromInput = argumentsFromInput.Split(' ', 2)[1];
                        arguments.Add(currentArgument);
                        break;
                    }
                case var x when x == typeof(byte[]):
                    {
                        var argumentArray = argumentsFromInput.Split(' ');
                        var values = new byte[argumentArray.Length];
                        for (var i = 0; i < argumentArray.Length; i++)
                        {
                            if (byte.TryParse(argumentArray[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                                values[i] = value;
                            else
                                throw new CommandArgumentException(argumentsFromInput, typeof(byte[]));
                        }
                        arguments.Add(values);
                        argumentsFromInput = string.Empty;
                        break;
                    }
                default:
                    throw new CommandTypeNotSupportedException(argument.Type);
            }
        }
        commands[command].type.InvokeMember(commands[command].methodInfo.Name, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, arguments.ToArray());
    }

    public string GenerateHelpLines()
    {
        var result = new StringBuilder();
        foreach (var command in commands)
        {
            var argumentAttributes = command.Value.methodInfo.GetCustomAttributes<ArgumentAttribute>();
            result.AppendLine($"{command.Key}: {command.Value.commandDescription}");
            foreach (var argument in argumentAttributes)
                result.AppendLine($"\t- {argument.Name}: {argument.Help}");
        }
        return result.ToString();
    }
}

public class CommandNotEnoughArgumentsException : Exception
{
    public CommandNotEnoughArgumentsException() : base("Input does not have enough arguments for command")
    {
    }
}

public class CommandTypeNotSupportedException : Exception
{
    public CommandTypeNotSupportedException(Type type) : base($"Type {type.Name} is not supported")
    {
    }
}

public class CommandArgumentException : Exception
{
    public CommandArgumentException(string argument, Type type) : base($"Could not parse input {argument} to type {type.Name}")
    {
    }
}

public class CommandAttribute : Attribute
{
    public CommandAttribute(string commandName, string description)
    {
        CommandName = commandName;
        Description = description;
    }
    public string CommandName { get; }
    public string Description { get; }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ArgumentAttribute : Attribute
{
    public ArgumentAttribute(Type type, string name, string help)
    {
        Type = type;
        Name = name;
        Help = help;
    }

    public Type Type { get; }
    public string Name { get; }
    public string Help { get; }
}
