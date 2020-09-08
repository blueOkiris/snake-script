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

                var code = "";
                try {
                    code = File.ReadAllText(fileName);
                } catch(Exception e) {
                    Console.WriteLine("Error reading file: '" + fileName + "'");
                    Console.WriteLine(e.Message);
                    Environment.Exit(1);
                }

                var deSnakedCode = Lexer.DeSnakeCode(code);
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
            }
        }
    }
}
