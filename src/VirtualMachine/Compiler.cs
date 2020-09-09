using System.Collections.Generic;
using System.Text;

namespace snakescript {
    partial class Compiler {
        // <func-def> ::= <func-op> <ident> <type-op> <type> 
        //                                          <bool-op> <type> <body>
        private static Function compileFunc(CompoundToken funcDef) {
            var ident = funcDef.Children[1] as SymbolToken;
            var typeTok = funcDef.Children[3] as CompoundToken;
            var retTypeTok = funcDef.Children[5] as CompoundToken;
            var body = funcDef.Children[6] as CompoundToken;

            // <body> ::= <l-brace> { <stmt> } <r-brace>
            var opCodes = new List<OpCode>();
            for(int i = 1; i < body.Children.Length - 1; i++) {
                var stmtCodes = compileStmt(body.Children[i] as CompoundToken);
                foreach(var opCode in stmtCodes) {
                    opCodes.Add(opCode);
                }
            }

            var type = toVmValueType(typeTok);
            var retType = toVmValueType(typeTok);
            
            return new Function(ident.Source, type, retType, opCodes.ToArray());
        }
        
        // <stmt> ::= <op> | <while> | <func-call> | <return> | <value>
        //          | <to-str> | <to-chr> | <mk-tup> | <parse-str> | <to-bool>
        private static OpCode[] compileStmt(CompoundToken stmt) {
            var opCodes = new List<OpCode>();
            OpCode[] tempArr;

            foreach(var child in stmt.Children) {
                switch(child.Type) {
                    case TokenType.Op:
                        tempArr = compileOp(child as CompoundToken);
                        foreach(var opCode in tempArr) {
                            opCodes.Add(opCode);
                        }
                        break;
                    
                    case TokenType.While:
                        break;
                    
                    case TokenType.FuncCall:
                        break;

                    case TokenType.Return:
                        opCodes.Add(OpCode.Return);
                        break;
                }
            }

            return opCodes.ToArray();
        }

        // <program> ::= { <func-def> | <stmt> }
        public static (OpCode[], Function[]) Translate(CompoundToken ast) {
            var opCodes = new List<OpCode>();
            var funcs = new List<Function>();

            var children = ast.Children;
            foreach(var child in children) {
                switch(child.Type) {
                    case TokenType.FuncDef:
                        funcs.Add(compileFunc(child as CompoundToken));
                        break;
                    
                    case TokenType.Stmt:
                        var stmtOpCodes = compileStmt(child as CompoundToken);
                        foreach(var opCode in stmtOpCodes) {
                            opCodes.Add(opCode);
                        }
                        break;
                }
            }
            return (opCodes.ToArray(), funcs.ToArray());
        }

        public static string OutputString(
                (OpCode[], Function[]) compilerOutput) {
            var outputStr = new StringBuilder();

            outputStr.Append("Functions:\n");
            foreach(var func in compilerOutput.Item2) {
                outputStr.Append(func);
                outputStr.Append('\n');
            }
            outputStr.Append('\n');
            foreach(var opcode in compilerOutput.Item1) {
                outputStr.Append(opcode);
                outputStr.Append('\n');
            }

            return outputStr.ToString();
        }
    }
}