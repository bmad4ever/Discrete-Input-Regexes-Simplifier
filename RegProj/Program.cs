using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RegProj
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="reps"></param>
        /// <returns>stopwatch with the exec time</returns>
        delegate Stopwatch Exec(Command[] commands,String[] tomatch,int reps,bool compile);

        private static Exec OrJoins = (c, m,r,comp) =>
        {
            String finalReg = "^(";
            foreach (var command in c)
            {
                finalReg += command.Regex.Substring(0, command.Regex.Length - 1) +
                           "(?<" + command.Name + ">" + command.Regex[command.Regex.Length - 1] + ")|";
            }
            finalReg.Substring(0, finalReg.Length - 1);
            finalReg += ")";
            Regex compiledRegex = new Regex(finalReg,(comp? RegexOptions.Compiled : 0) | RegexOptions.ExplicitCapture);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = r; r >= 0; --r)
                foreach (var text in m)
                    compiledRegex.Match(text);

            sw.Stop();
            return sw;
        };

        private static Exec PreserveOrderSimplification = (c, m, r, comp) =>
        {
            Regex compiledRegex = new Regex(CommandsCompiler.CompileCommands(c.ToList()), (comp ? RegexOptions.Compiled : 0) | RegexOptions.ExplicitCapture);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for(int i = r; r>=0; --r)
                foreach (var text in m)
                    compiledRegex.Match(text);

            sw.Stop();
            return sw;
        };

        static void Main(string[] args)
        {
            performance1();
            performance2();

            performance12();
            performance22();

            performance3_1();
            performance3_2();

            performance4_1();
            performance4_2();
        }

        static void performance1()
        {
            Console.WriteLine("PERFORMANCE TEST 1");
            Console.WriteLine("summary:low reps; big strings; 2 end variations");
            Console.WriteLine();

            int reps = 100;
            String[] text = new string[]
            {
                "133133133133133133133133133133133133133133133"+
                "133133133133133133133133133133133133133133133"+
                "233233233233233233233233233233233233233233233"+
                "233233233233233233233233233233233233233233233"+
                "b"
            };
            Command[] commands = new Command[]
            {
                nc("cap1",@"(1..){30,70}(2..){10,50}b"),
                nc("cap2",@"(1..){20,40}(2..){0,30}b"),
            };
            Measeure("Or Joins",OrJoins,reps,false,text,commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("RespectSimp", PreserveOrderSimplification, reps,false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Console.WriteLine("--------------------------");
        }

        static void performance2()
        {
            Console.WriteLine("PERFORMANCE TEST 2");
            Console.WriteLine("summary:low reps; small strings; 2 end variations");
            Console.WriteLine();

             int reps = 100;
                    String[] text = new string[]
                        {
                            "133133133233b"
                        };
                        Command[] commands = new Command[]
                        {
                            nc("cap1",@"(1..){3,7}(2..){1,5}b"),
                            nc("cap2",@"(1..){2,4}(2..){0,3}b"),
                        };
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Console.WriteLine("--------------------------");
        }

        static void performance12()
        {
            Console.WriteLine("PERFORMANCE TEST 1.2");
            Console.WriteLine("summary:high reps; big strings; 2 end variations");
            Console.WriteLine();

            int reps = 1000000;
            String[] text = new string[]
            {
                "133133133133133133133133133133133133133133133"+
                "133133133133133133133133133133133133133133133"+
                "233233233233233233233233233233233233233233233"+
                "233233233233233233233233233233233233233233233"+
                "b"
            };
            Command[] commands = new Command[]
            {
                nc("cap1",@"(1..){30,70}(2..){10,50}b"),
                nc("cap2",@"(1..){20,40}(2..){0,30}b"),
            };
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Console.WriteLine("--------------------------");
        }

        static void performance22()
        {
            Console.WriteLine("PERFORMANCE TEST 2.2");
            Console.WriteLine("summary:high reps; small strings; 2 end variations");
            Console.WriteLine();

            int reps = 1000000;
            String[] text = new string[]
                {
                            "133133133233b"
                };
            Command[] commands = new Command[]
            {
                            nc("cap1",@"(1..){3,7}(2..){1,5}b"),
                            nc("cap2",@"(1..){2,4}(2..){0,3}b"),
            };
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Console.WriteLine("--------------------------");
        }

        static void performance3_1()
        {
            Console.WriteLine("PERFORMANCE TEST 3.1");
            Console.WriteLine("summary:low reps; small strings; 4 end variations");
            Console.WriteLine();

            int reps = 100;
            String[] text = new string[]
            {
                "133133133233b"
            };
            Command[] commands = new Command[]
            {
                nc("cap1",@"(1..){3,7}(2..){1,5}z"),
                nc("cap1",@"(1..){3,7}(2..){1,5}a"),
                nc("cap1",@"(1..){3,7}(2..){1,5}b"),
                nc("cap2",@"(1..){2,4}(2..){0,3}b"),
            };
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Console.WriteLine("--------------------------");
        }

        static void performance3_2()
        {
            Console.WriteLine("PERFORMANCE TEST 3.2");
            Console.WriteLine("summary:high reps; small strings; 4 end variations");
            Console.WriteLine();

            int reps = 1000000;
            String[] text = new string[]
            {
                "133133133233b"
            };
            Command[] commands = new Command[]
            {
                nc("cap1",@"(1..){3,7}(2..){1,5}z"),
                nc("cap1",@"(1..){3,7}(2..){1,5}a"),
                nc("cap1",@"(1..){3,7}(2..){1,5}b"),
                nc("cap2",@"(1..){2,4}(2..){0,3}b"),
            };
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Console.WriteLine("--------------------------");
        }

        static void performance4_1()
        {
            Console.WriteLine("PERFORMANCE TEST 4.1");
            Console.WriteLine("summary:low reps; big strings; 4 end variations");
            Console.WriteLine();

            int reps = 100;
            String[] text = new string[]
            {
                "133133133133133133133133133133133133133133133"+
                "133133133133133133133133133133133133133133133"+
                "233233233233233233233233233233233233233233233"+
                "233233233233233233233233233233233233233233233"+
                "b"
            };
            Command[] commands = new Command[]
            {
                nc("cap1",@"(1..){30,70}(2..){10,50}z"),
                nc("cap1",@"(1..){30,70}(2..){10,50}a"),
                nc("cap1",@"(1..){30,70}(2..){10,50}b"),
                nc("cap2",@"(1..){20,40}(2..){0,30}b"),
            };
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Console.WriteLine("--------------------------");
        }

        static void performance4_2()
        {
            Console.WriteLine("PERFORMANCE TEST 4.2");
            Console.WriteLine("summary: high reps; big strings; 4 end variations");
            Console.WriteLine();

            int reps = 100000;
            String[] text = new string[]
            {
                "133133133133133133133133133133133133133133133"+
                "133133133133133133133133133133133133133133133"+
                "233233233233233233233233233233233233233233233"+
                "233233233233233233233233233233233233233233233"+
                "b"
            };
            Command[] commands = new Command[]
            {
                nc("cap1",@"(1..){30,70}(2..){10,50}z"),
                nc("cap1",@"(1..){30,70}(2..){10,50}a"),
                nc("cap1",@"(1..){30,70}(2..){10,50}b"),
                nc("cap2",@"(1..){20,40}(2..){0,30}b"),
            };
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Console.WriteLine("--------------------------");
        }

        static Command nc(String name, String regex)
        {
            Command command = new Command();
            command.Regex = regex;
            command.Name = name;
            return command;
        }

        static void Measeure(String label,Exec toMeasure, int repetitions,bool compile, String[] tomatch,Command[] commands)
        {
            Stopwatch sw = toMeasure(commands, tomatch,repetitions,compile);
            Console.WriteLine(label);
            Console.WriteLine("Elapsed={0}", sw.Elapsed);
            Console.WriteLine("(60 frames per second) How many per frame={0}", Convert.ToDouble(repetitions) / sw.Elapsed.TotalMilliseconds / 60.0);
            Console.WriteLine();
            GC.Collect();
            GC.WaitForFullGCComplete();
            System.Threading.Thread.Sleep(500);
        }

    }
}
