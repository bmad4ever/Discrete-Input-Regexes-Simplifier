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
                regex = regex,
                name = name
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
            var finalregex = CommandsCompiler.CompileCommands(commands);
            Assert.AreEqual(expectedResult, finalregex);
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
            var commands = new List<Command> { baseCommand };
            for (var i = 0; i < commands.Count; i++)
            {
                commands.Add(testCommands[i]);
                if (swap)
                {
                    var temp = testCommands[i];
                    testCommands[i] = testCommands[0];
                    testCommands[0] = temp;
                }
                Assert.AreEqual(assertStrings[i-1], CommandsCompiler.CompileCommands(commands));
                commands.Remove(testCommands[i]);
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
        }

        [TestMethod]
        public void TestContainedCases()
        {
            Command testCommandbase = Nc("one", @"(1..){2,5}b");
            Command[] testCommands = new Command[]
            {
                Nc("one",@"(1..){2,3}c"),
                Nc("one",@"(1..){3,5}c"),
                Nc("one",@"(1..){3,4}c")
            };
            string[] testComStrings = new string[]
            {
                @"(1..){2,3}+((1..){0,2}+(?<b>b)|(?<c>c))",
                @"(1..){2,3}+((1..){0,2}+((?<b>b)|(?<c>c)))",
                @"(1..){2,3}+({0,1}+({0,1}+(?<b>b)|(?<c>c)))"
            };
            TestAnOperation(testCommandbase, testCommands, testComStrings,false);
            testComStrings = new string[]
            {
                @"(1..){2,3}+((?<c>c)|(1..){0,2}+(?<b>b))",
                @"(1..){2,3}+((1..){0,2}+((?<c>c)|(?<b>b)))",
                @"(1..){2,3}+((1..){0,1}+((?<c>c)|(1..){0,1}(?<b>b)))"
            };
            TestAnOperation(testCommandbase, testCommands, testComStrings,true);
        }

        [TestMethod]
        public void TestOutterCases()
        {

        }

        [TestMethod]
        public void TestCrossCases()
        {

        }

        [TestMethod]
        public void TestComplexCases()
        {

        }
    }
}
