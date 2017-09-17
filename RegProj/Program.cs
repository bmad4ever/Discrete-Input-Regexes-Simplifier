using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegProj
{
    class Program
    {
        private int a;

        static void Main(string[] args)
        {
            List<Command> testCommands = new List<Command>();
            testCommands.Add(nc("one",@"(1..){2,3}w*?b"));
            testCommands.Add(nc("two", @"(1..){2,3}w*?c"));
            //testCommands.Add(nc("three", @"(1..){2,3}(z)c"));
            Console.WriteLine(CommandsCompiler.CompileCommands(testCommands));

            

        }

        static Command nc(String name, String regex)
        {
            Command command = new Command();
            command.regex = regex;
            command.name = name;
            return command;
        }

    }
}
