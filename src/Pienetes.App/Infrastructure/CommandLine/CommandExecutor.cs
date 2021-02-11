using System;
using System.Threading.Tasks;
using Pienetes.App.Domain.Services;

namespace Pienetes.App.Infrastructure.CommandLine
{
    public class CommandExecutor : ICommandExecutor
    {
        public Task Execute(string command)
        {
            Console.WriteLine("Would have run: " + command);
            return Task.CompletedTask;
        }
    }
}