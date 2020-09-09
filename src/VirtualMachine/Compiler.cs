using System.Collections.Generic;

namespace snakescript {
    class Compiler {
        private static Function compileFunc(CompoundToken funcDef) {
            return new Function(VmValueType.UndDef, VmValueType.UndDef, null);
        }

        private static OpCode[] compileStmt(CompoundToken stmt) {
            return new OpCode[] {};
        }

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
            return (new OpCode[] {}, new Function[] {});
        }
    }
}