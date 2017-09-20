using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegProj;

namespace UnitTests
{
    [TestClass]
    public class UnitTests
    {
        private static Command Nc(string name, string regex)
        {
            var command = new Command
            {
                Regex = regex,
                Name = name
            };
            return command;
        }

        /// <summary>
        /// check if the output for a certain groups of regexes is the expected
        /// </summary>
        /// <param name="testCommands"></param>
        /// <param name="expectedResult"></param>
        static void SimpleTest(Command[] testCommands, string expectedResult)
        {
            var commands = new List<Command>(testCommands);
            var finalregex = CommandsCompiler.CompileCommands(commands,false);
            Assert.AreEqual("^("+expectedResult+")", finalregex);
        }
        
        /// <summary>
        /// tests if base in conjuction with each one of the commands in testCommands.
        /// the testCommands pair with the assertions made (1 to 1 relationship).
        /// </summary>
        /// <param name="baseCommand"></param>
        /// <param name="testCommands"></param>
        /// <param name="assertStrings"></param>
        /// <param name="swap"></param>
        static void TestAnOperation(Command baseCommand, Command[] testCommands,string[] assertStrings,bool swap=false)
        {
            if (testCommands == null) throw new ArgumentNullException(nameof(testCommands));
            if (assertStrings == null) throw new ArgumentNullException(nameof(assertStrings));
            if(testCommands.Length != assertStrings.Length) throw new Exception("Number of assertions does't match number of testCommands");
            List<Command> commands;

            for (var i = 0; i < testCommands.Length; i++)
            {
               commands = !swap?
                    new List<Command> { baseCommand, testCommands[i] } :
                new List<Command> { testCommands[i], baseCommand };
                Assert.AreEqual("^("+assertStrings[i]+")", CommandsCompiler.CompileCommands(commands,false));
            }
        }

        [TestMethod]
        public void TestBasicOperations()
        {
            var c1 = new Command[]
            {
                Nc("b", @"(1..){2,3}w*?b"),
                Nc("c", @"(1..){2,3}w*?c")
            };
            var list1 = new List<Command>(c1);
            SimpleTest(c1,
                @"(1..){2,3}w*?((?<b>b)|(?<c>c))"
            );
            Assert.IsTrue(CommandsCompiler.ValidatesCommands(list1));
            list1.Add(Nc("some",@"**"));
            Assert.IsFalse(CommandsCompiler.ValidatesCommands(list1));

            SimpleTest(new Command[]
                {
                    Nc("1a","1a"),
                    Nc("2b","2b")
                },
                @"(1(?<1a>a)|2(?<2b>b))"
            );


            SimpleTest(new Command[]
                {
                    Nc("1a","1?a"),
                },
                @"1?(?<1a>a)"
            );

            SimpleTest(new Command[]
                {
                    Nc("1a","1{0,1}a"),
                },
                @"1?(?<1a>a)"
            );
        }

        [TestMethod]
        public void TestContainedCases()
        {
            Command testCommandbase = Nc("b", @"(1..){2,5}b");
            Command[] testCommands = new Command[]
            {
                Nc("c",@"(1..){2,3}c"),
                Nc("c",@"(1..){3,5}c"),
                Nc("c",@"(1..){3,4}c")
            };
            string[] testComStrings = new string[]
            {
                @"(1..){2,3}+((1..){0,2}(?<b>b)|(?<c>c))",
                @"(1..){2}((1..){1,3}((?<b>b)|(?<c>c))|(?<b>b))",
                @"(1..){2}((1..){1,2}+((1..)?(?<b>b)|(?<c>c))|(?<b>b))"
            };
            TestAnOperation(testCommandbase, testCommands, testComStrings,false);
            testComStrings = new string[]
            {
                @"(1..){2,3}+((?<c>c)|(1..){0,2}(?<b>b))",
                @"(1..){2}((1..){1,3}((?<c>c)|(?<b>b))|(?<b>b))",
                @"(1..){2}((1..){1,2}+((?<c>c)|(1..)?(?<b>b))|(?<b>b))"
            };
            TestAnOperation(testCommandbase, testCommands, testComStrings,true);
        }

        [TestMethod]
        public void TestOutterCases()
        {
            Command testCommandbase = Nc("b", @"(1..){3,6}b");
            Command[] testCommands = new Command[]
            {
                Nc("c",@"(1..){6,9}c"),
                Nc("c",@"(1..){7,9}c"),
                Nc("c",@"(1..){0,2}c"),
                Nc("c",@"(1..){1,3}c")
            };
            string[] testComStrings = new string[]
            {
                @"(1..){3,6}+((?<b>b)|(1..){0,3}(?<c>c))",
                @"(1..){3,6}+((?<b>b)|(1..){1,2}(?<c>c))",
                @"(1..){0,2}+((1..){1,3}(?<b>b)|(?<c>c))",
                @"(1..){1,3}+((1..){0,3}(?<b>b)|(?<c>c))"
            };
            TestAnOperation(testCommandbase, testCommands, testComStrings, false);
        }

        [TestMethod]
        public void TestCrossCases()
        {
            Command testCommandbase = Nc("b", @"(1..){3,7}b");
            Command[] testCommands = new Command[]
            {
                Nc("c",@"(1..){5,9}c"),
                Nc("c",@"(1..){1,5}c")
            };
            string[] testComStrings = new string[]
            {
                @"(1..){3,4}+((1..){1,3}+((?<b>b)|(1..){0,2}(?<c>c))|(?<b>b))",
                @"(1..){1,2}+((1..){1,3}+((1..){0,2}(?<b>b)|(?<c>c))|(?<c>c))"
            };
            TestAnOperation(testCommandbase, testCommands, testComStrings, false);
        }

        [TestMethod]
        public void TestComplexCases()
        {

        }
    }
}
