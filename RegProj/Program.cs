using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RegProj;

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
        delegate Stopwatch Exec(Command[] commands, String[] tomatch, int reps, bool compile);
        delegate Stopwatch ExecRegex(Regex regex, String[] tomatch, int reps);

        private static Exec OrJoins = (c, m, r, comp) =>
        {
            String finalReg = "^(";
            foreach (var command in c)
            {
                finalReg += command.Regex.Substring(0, command.Regex.Length - 1) +
                           "(?<" + command.Name + ">" + command.Regex[command.Regex.Length - 1] + ")|";
            }
            finalReg.Substring(0, finalReg.Length - 1);
            finalReg += ")";
            Regex compiledRegex = new Regex(finalReg, (comp ? RegexOptions.Compiled : 0) | RegexOptions.ExplicitCapture);

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

            for (int i = r; r >= 0; --r)
                foreach (var text in m)
                    compiledRegex.Match(text);

            sw.Stop();
            return sw;
        };

        private static ExecRegex HardCompiled = (regex, m, reps) =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = reps; reps >= 0; --reps)
                foreach (var text in m)
                    regex.Match(text);
            sw.Stop();
            return sw;
        };


        private static void HardCompileRegex(List<RegexCompilationInfo> compilationInfos, bool orJoin, string name, Command[] c)
        {
            string finalReg;
            if (orJoin)
            {
                finalReg = "^(";
                foreach (var command in c)
                {
                    finalReg += command.Regex.Substring(0, command.Regex.Length - 1) +
                                "(?<" + command.Name + ">" + command.Regex[command.Regex.Length - 1] + ")|";
                }

                finalReg.Substring(0, finalReg.Length - 1);
                finalReg += ")";
                name = "OrJoin" + name;
            }
            else
            {
                finalReg = CommandsCompiler.CompileCommands(c.ToList());
                name = "PreserveOrderSimplification" + name;
            }



            var expr = new RegexCompilationInfo(finalReg,
                RegexOptions.ExplicitCapture,
                name,
                "RegProj",
                true);
            compilationInfos.Add(expr);

        }

        static void Main(string[] args)
        {
            /*
            Generate dll with regexes...
            
            List<RegexCompilationInfo> compilationList = new List<RegexCompilationInfo>();

            HardCompileRegex(compilationList, true, "perf1", CommandsP1);
            HardCompileRegex(compilationList, false, "perf1", CommandsP1);

            HardCompileRegex(compilationList, true, "perf2", CommandsP2);
            HardCompileRegex(compilationList, false, "perf2", CommandsP2);

            HardCompileRegex(compilationList, true, "perf12", CommandsP12);
            HardCompileRegex(compilationList, false, "perf12", CommandsP12);

            HardCompileRegex(compilationList, true, "perf22", CommandsP22);
            HardCompileRegex(compilationList, false, "perf22", CommandsP22);

            HardCompileRegex(compilationList, true, "perf3", CommandsP3);
            HardCompileRegex(compilationList, false, "perf3", CommandsP3);

            HardCompileRegex(compilationList, true, "perf4", CommandsP4);
            HardCompileRegex(compilationList, false, "perf4", CommandsP4);

            AssemblyName assemName = new AssemblyName("RegexLib, Version=1.0.0.1001, Culture=neutral, PublicKeyToken=null");
            Regex.CompileToAssembly(compilationList.ToArray(), assemName);

            return;*/

            performance1();
            performance2();

            performance12();
            performance22();

            performance3_1();
            performance3_2();

            performance4_1();
            performance4_2();
        }

        static readonly Command[] CommandsP1 = new Command[]
        {
            nc("cap1",@"(1..){30,70}(2..){10,50}b"),
            nc("cap2",@"(1..){20,40}(2..){0,30}b"),
        };
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
            Command[] commands = CommandsP1;
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("HC OrJoin", HardCompiled, reps, text, new OrJoinperf1());
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Measeure("HC RespectSimp", HardCompiled, reps, text, new PreserveOrderSimplificationperf1());
            Console.WriteLine("--------------------------");
        }

        static readonly Command[] CommandsP2 = new Command[]
        {
            nc("cap1",@"(1..){3,7}(2..){1,5}b"),
            nc("cap2",@"(1..){2,4}(2..){0,3}b"),
        };
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
            Command[] commands = CommandsP2;

            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("HC OrJoin", HardCompiled, reps, text, new OrJoinperf2());
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Measeure("HC RespectSimp", HardCompiled, reps, text, new PreserveOrderSimplificationperf2());
            Console.WriteLine("--------------------------");
        }

        static readonly Command[] CommandsP12 = new Command[]
        {
            nc("cap1",@"(1..){30,70}(2..){10,50}b"),
            nc("cap2",@"(1..){20,40}(2..){0,30}b"),
        };
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
            Command[] commands = CommandsP12;
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("HC OrJoin", HardCompiled, reps, text, new OrJoinperf12());
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Measeure("HC RespectSimp", HardCompiled, reps, text, new PreserveOrderSimplificationperf12());
            Console.WriteLine("--------------------------");
        }

        static readonly Command[] CommandsP22 = new Command[]
        {
            nc("cap1",@"(1..){3,7}(2..){1,5}b"),
            nc("cap2",@"(1..){2,4}(2..){0,3}b"),
        };
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
            Command[] commands = CommandsP22;
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("HC OrJoin", HardCompiled, reps, text, new OrJoinperf22());
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Measeure("HC RespectSimp", HardCompiled, reps, text, new PreserveOrderSimplificationperf22());
            Console.WriteLine("--------------------------");
        }

        static readonly Command[] CommandsP3 = new Command[]
        {
            nc("cap1",@"(1..){3,7}(2..){1,5}z"),
            nc("cap1",@"(1..){3,7}(2..){1,5}a"),
            nc("cap1",@"(1..){3,7}(2..){1,5}b"),
            nc("cap2",@"(1..){2,4}(2..){0,3}b"),
        };
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
            Command[] commands = CommandsP3;
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("HC OrJoin", HardCompiled, reps, text, new OrJoinperf3());
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Measeure("HC RespectSimp", HardCompiled, reps, text, new PreserveOrderSimplificationperf3());
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
            Command[] commands = CommandsP3;
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("HC OrJoin", HardCompiled, reps, text, new OrJoinperf3());
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Measeure("HC RespectSimp", HardCompiled, reps, text, new PreserveOrderSimplificationperf3());
            Console.WriteLine("--------------------------");
        }

        static readonly Command[] CommandsP4 = new Command[]
            {
                nc("cap1",@"(1..){30,70}(2..){10,50}z"),
                nc("cap1",@"(1..){30,70}(2..){10,50}a"),
                nc("cap1",@"(1..){30,70}(2..){10,50}b"),
                nc("cap2",@"(1..){20,40}(2..){0,30}b"),
            };
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
            Command[] commands = CommandsP4;
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("HC OrJoin", HardCompiled, reps, text, new OrJoinperf4());
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Measeure("HC RespectSimp", HardCompiled, reps, text, new PreserveOrderSimplificationperf4());
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
            Command[] commands = CommandsP4;
            Measeure("Or Joins", OrJoins, reps, false, text, commands);
            Measeure("Or Joins (compiled)", OrJoins, reps, true, text, commands);
            Measeure("HC OrJoin", HardCompiled, reps, text, new OrJoinperf4());
            Measeure("RespectSimp", PreserveOrderSimplification, reps, false, text, commands);
            Measeure("RespectSimp (compiled)", PreserveOrderSimplification, reps, true, text, commands);
            Measeure("HC RespectSimp", HardCompiled, reps, text, new PreserveOrderSimplificationperf4());
            Console.WriteLine("--------------------------");
        }

        static Command nc(String name, String regex)
        {
            Command command = new Command();
            command.Regex = regex;
            command.Name = name;
            return command;
        }

        static void Measeure(String label, Exec toMeasure, int repetitions, bool compile, String[] tomatch, Command[] commands)
        {
            Stopwatch sw = toMeasure(commands, tomatch, repetitions, compile);
            Console.WriteLine(label);
            Console.WriteLine("Elapsed        = {0}", sw.Elapsed);
            Console.WriteLine("Elapsed (ms) by one = {0}", sw.Elapsed.TotalMilliseconds / Convert.ToDouble(repetitions));
            Console.WriteLine("(60 frames per second)\nHow many per frame={0}", 1000.0 / 60.0 * Convert.ToDouble(repetitions) / sw.Elapsed.TotalMilliseconds);
            Console.WriteLine();
            GC.Collect();
            GC.WaitForFullGCComplete();
            System.Threading.Thread.Sleep(500);
        }

        static void Measeure(String label, ExecRegex toMeasure, int repetitions, String[] tomatch, Regex regex)
        {
            Stopwatch sw = toMeasure(regex, tomatch, repetitions);
            Console.WriteLine(label);
            Console.WriteLine("Elapsed        = {0}", sw.Elapsed);
            Console.WriteLine("Elapsed (ms) by one = {0}", sw.Elapsed.TotalMilliseconds / Convert.ToDouble(repetitions));
            Console.WriteLine("(60 frames per second)\nHow many per frame={0}", 1000.0 / 60.0 * Convert.ToDouble(repetitions) / sw.Elapsed.TotalMilliseconds);
            Console.WriteLine();
            GC.Collect();
            GC.WaitForFullGCComplete();
            System.Threading.Thread.Sleep(500);
        }

    }
}
