using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace snakescript {
    struct NativeFunction {
        public string Name;
        public Func<object, object> Code;

        public NativeFunction(string sourceFile, string name) {
            var source = "";
            try {
                source = File.ReadAllText(sourceFile);
            } catch(Exception e) {
                Console.WriteLine("Failed to read file: " + sourceFile);
                Console.WriteLine(e.Message);
            }
            Name = name;
            Code = compile(source);
        }

        private static Func<object, object> compile(string source) {
            var options = ScriptOptions.Default.AddReferences(
                new Assembly[] {
                    typeof(Object).Assembly, typeof(List<Object>).Assembly
                }
            );
            var func = CSharpScript.EvaluateAsync<Func<object, object>>(
                source, options
            ).Result;
            return func;
        }

        public VmValue Execute(VmValue input) {
            return VmValue.FromObject(Code(input));
        }
    }
}