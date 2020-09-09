using System.Collections.Generic;

namespace snakescript {
    class Compiler {
        private static VmValueType[] valueTypeFromStr(SymbolToken typeTok) {
            switch(typeTok.Source) {
                case "#":
                    return new VmValueType[] { VmValueType.Num };
                
                case "@":
                    return new VmValueType[] { VmValueType.Chr };
                
                case "??":
                    return new VmValueType[] { VmValueType.Bool };
            }
            
            return new VmValueType[] { VmValueType.UndDef };
        }

        // <type-name> ::= <raw-type-name> | <l-bracket> <type-name> <r-bracket>
        //                  | <l-parenth> <type-name> <type-name> <r-parenth>
        // <raw-type-name> ::= /([#@])|(\?\?)/
        // <tuple> ::= <l-parenth> <value> <value> <r-parenth>
        // <list> ::= <l-bracket> { <value> } <r-bracket>
        private static VmValueType[] toVmValueType(CompoundToken typeTok) {
            if(typeTok.Children.Length == 1) {
                return valueTypeFromStr(typeTok.Children[0] as SymbolToken);
            } else if(typeTok.Children[0].Type == TokenType.LParenth) {
                var types = new List<VmValueType>();
                types.Add(VmValueType.Tup);

                VmValueType[] item1Types, item2Types;
                if(typeTok.Children[1] is SymbolToken) {
                    item1Types = valueTypeFromStr(
                        typeTok.Children[1] as SymbolToken
                    );
                } else {
                    item1Types = toVmValueType(
                        typeTok.Children[1] as CompoundToken
                    );
                }
                if(typeTok.Children[2] is SymbolToken) {
                    item2Types = valueTypeFromStr(
                        typeTok.Children[2] as SymbolToken
                    );
                } else {
                    item2Types = toVmValueType(
                        typeTok.Children[2] as CompoundToken
                    );
                }

                foreach(var type in item1Types) {
                    types.Add(type);
                }
                types.Add(VmValueType.UndDef);
                foreach(var type in item2Types) {
                    types.Add(type);
                }

                return types.ToArray();
            } else if(typeTok.Children[0].Type == TokenType.LBracket) {
                var types = new List<VmValueType>();
                types.Add(VmValueType.Ls);

                VmValueType[] subTypes;
                if(typeTok.Children[1] is SymbolToken) {
                    subTypes = valueTypeFromStr(
                        typeTok.Children[1] as SymbolToken
                    );
                } else {
                    subTypes = toVmValueType(
                        typeTok.Children[1] as CompoundToken
                    );
                }

                foreach(var type in subTypes) {
                    types.Add(type);
                }

                return types.ToArray();
            }

            return new VmValueType[] { VmValueType.UndDef };
        }

        // <func-def> ::= <func-op> <ident> <type-op> <type> 
        //                                          <bool-op> <type> <body>
        private static Function compileFunc(CompoundToken funcDef) {
            var ident = funcDef.Children[1] as SymbolToken;
            var typeTok = funcDef.Children[3] as CompoundToken;
            var retTypeTok = funcDef.Children[5] as CompoundToken;
            var body = funcDef.Children[6] as CompoundToken;

            // <body> ::= <l-brace> { <stmt> } <r-brace>
            var opCodes = new List<OpCode>();
            for(int i = 1; i < body.Children.Length - 2; i++) {
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
        private static OpCode[] compileStmt(CompoundToken stmt) {
            return new OpCode[] {};
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
            return (new OpCode[] {}, new Function[] {});
        }
    }
}