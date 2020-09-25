using System;
using System.IO;

namespace snakescript {
    class Program {
        static void Main(string[] args) {
            if(args.Length < 1) {
                Console.WriteLine("No input file provided");
                Environment.Exit(-1);
            } else {
                var debug = false;
                var fileName = args[0];
                
                foreach(var arg in args) {
                    if(arg == args[0])
                        continue;
                    
                    if(arg == "--debug") {
                        debug = true;
                    } else {
                        Console.WriteLine("Unexpected argument: '" + arg + "'");
                        Environment.Exit(-1);
                    }
                }

                // File name is actually a MODULE DIRECTORY
                if(Directory.Exists(fileName)) {
                    fileName += "/main.snake";
                }

                var code = "";
                try {
                    code = File.ReadAllText(fileName);
                } catch(Exception e) {
                    Console.WriteLine("Error reading file: '" + fileName + "'");
                    Console.WriteLine(e.Message);
                    Environment.Exit(-1);
                }

                var deSnakedCode = "";
                try {
                    deSnakedCode = Lexer.DeSnakeCode(code);
                } catch(Exception e) {
                    Console.WriteLine("Desnaking Error: " + e.Message);
                    Environment.Exit(-1);
                }
                var tokens = Lexer.Tokens(deSnakedCode);
                if(debug) {
                    Console.WriteLine("Desnaked code:");
                    Console.WriteLine(deSnakedCode);
                    Console.WriteLine();
                    Console.WriteLine("Lexer Output:");
                    foreach(var token in tokens) {
                        Console.WriteLine(token);
                    }
                }

                CompoundToken ast = null;
                try {
                    ast = Parser.BuildProgram(tokens);
                } catch(ParserException pe) {
                    Console.WriteLine("Parser Error: " + pe.Message);
                    Environment.Exit(-1);
                }
                if(debug && ast != null) {
                    Console.WriteLine();
                    Console.WriteLine("Parser Output:");
                    Console.WriteLine(ast);
                }

                var insts = Compiler.Translate(ast);
                if(debug) {
                    Console.WriteLine(Compiler.OutputString(insts));
                }
                var vm = new VirtualMachine(insts.Item1, insts.Item2, debug);
                
                try {
                    Directory.SetCurrentDirectory(
                        Path.GetDirectoryName(fileName)
                    );
                    vm.Run();
                } catch(Exception e) {
                    Console.WriteLine("Runtime Error: " + e.Message);
                    Environment.Exit(-1);
                }
            }
        }
    }
}
