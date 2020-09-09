using System.Collections.Generic;

namespace snakescript {
    partial class Compiler {
        public static int WhileInd = 0;

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

        // <op> ::= <stack-op> | <math-op> | <bool-op> | <ls-tp-op>
        private static OpCode[] compileOp(CompoundToken op) {
            var opCodes = new List<OpCode>();

            switch(op.Children[0].Type) {
                case TokenType.IoOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case ".":
                            opCodes.Add(new OpCode(Instruction.Print));
                            break;
                        
                        case ",":
                            opCodes.Add(new OpCode(Instruction.Input));
                            break;
                    }
                    break;

                case TokenType.RoundOp:
                    opCodes.Add(new OpCode(Instruction.Round));
                    break;
                
                case TokenType.StackOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case ">>":
                            opCodes.Add(new OpCode(Instruction.Pop));
                            break;
                        
                        case "><":
                            opCodes.Add(new OpCode(Instruction.Duplicate));
                            break;
                        
                        case "<>":
                            opCodes.Add(new OpCode(Instruction.Swap));
                            break;
                    }
                    break;
                
                case TokenType.MathOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case "+":
                            opCodes.Add(new OpCode(Instruction.Pop2PushSum));
                            break;
                        
                        case "-":
                            opCodes.Add(
                                new OpCode(Instruction.Pop2PushDifference)
                            );
                            break;
                        
                        case "*":
                            opCodes.Add(
                                new OpCode(Instruction.Pop2PushProduct)
                            );
                            break;
                        
                        case "^":
                            opCodes.Add(new OpCode(Instruction.Pop2PushPower));
                            break;
                    }
                    break;
                
                case TokenType.BoolOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case "?=":
                            opCodes.Add(new OpCode(Instruction.Pop2PushEqual));
                            break;
                        
                        case "?>":
                            opCodes.Add(
                                new OpCode(Instruction.Pop2PushGreaterThan)
                            );
                            break;
                        
                        case "?<":
                            opCodes.Add(
                                new OpCode(Instruction.Pop2PushLessThan)
                            );
                            break;
                        
                        case "?!":
                            opCodes.Add(new OpCode(Instruction.Pop1PushNot));
                            break;
                        
                        case "?&":
                            opCodes.Add(new OpCode(Instruction.Pop2PushAnd));
                            break;
                        
                        case "?|":
                            opCodes.Add(new OpCode(Instruction.Pop2PushOr));
                            break;
                    }
                    break;
                
                case TokenType.ListTupleOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case "++":
                            opCodes.Add(new OpCode(Instruction.Pop2PushConcat));
                            break;
                        
                        case "--":
                            opCodes.Add(
                                new OpCode(Instruction.Pop2PushWithRemovedInd)
                            );
                            break;
                        
                        case "@@":
                            opCodes.Add(
                                new OpCode(
                                    Instruction.Pop2PushListPushListAtInd
                                )
                            );
                            break;
                        
                        case "[]":
                            opCodes.Add(
                                new OpCode(
                                    Instruction.PopItemsOfSameTypePushList
                                )
                            );
                            break;
                        
                        case "][":
                            opCodes.Add(
                                new OpCode(Instruction.PopListPushUnzipped)
                            );
                            break;
                    }
                    break;
            }
            
            return opCodes.ToArray();
        }

        // <while> ::= <while-op> <body>
        private static OpCode[] compileWhile(CompoundToken wh) {
            var opCodes = new List<OpCode>();

            opCodes.Add(new OpCode(Instruction.WhileStart, "WH_" + WhileInd));
            var body = wh.Children[1] as CompoundToken;
            for(int i = 1; i < body.Children.Length - 1; i++) {
                var stmtCodes = compileStmt(body.Children[i] as CompoundToken);
                foreach(var opCode in stmtCodes) {
                    opCodes.Add(opCode);
                }
            }
            opCodes.Add(new OpCode(Instruction.WhileEnd, "EW_" + WhileInd));
            WhileInd++;

            return opCodes.ToArray();
        }

        // <value> ::= <ident> | <char> | <num> | <bool>
        //              | <str> | <list> | <tuple>
        // <list> ::= <l-bracket> { <value> } <r-bracket>
        // <tuple> ::= <l-parenth> <value> <value> <r-parenth>
        public static OpCode[] CompileValue(CompoundToken value) {
            var opCodes = new List<OpCode>();
            
            OpCode[] tempArr;
            Token subChild = value.Children[0];
            switch(subChild.Type) {
                case TokenType.Num:
                    opCodes.Add(
                        new OpCode(
                            Instruction.PushNum,
                            (subChild as SymbolToken).Source
                        )
                    );
                    break;

                case TokenType.Char:
                    opCodes.Add(
                        new OpCode(
                            Instruction.PushChar,
                            (subChild as SymbolToken).Source
                        )
                    );
                    break;

                case TokenType.Bool:
                    opCodes.Add(
                        new OpCode(
                            Instruction.PushBool,
                            (subChild as SymbolToken).Source
                        )
                    );
                    break;

                case TokenType.Ident:
                    opCodes.Add(
                        new OpCode(
                            Instruction.PushIdent,
                            (subChild as SymbolToken).Source
                        )
                    );
                    break;
                
                case TokenType.Str:
                    for(int i = (subChild as SymbolToken).Source.Length - 2;
                            i >= 1; i--) {
                        opCodes.Add(
                            new OpCode(
                                Instruction.PushChar,
                                (subChild as SymbolToken).Source[i] + ""
                            )
                        );
                    }
                    opCodes.Add(
                        new OpCode(Instruction.PopItemsOfSameTypePushList)
                    );
                    break;
                
                case TokenType.List:
                    for(
                            int i =
                                (subChild as CompoundToken).Children.Length - 2;
                            i >= 1; i--) {
                        tempArr = CompileValue(
                            (subChild as CompoundToken).Children[i]
                                as CompoundToken
                        );
                        foreach(var opCode in tempArr) {
                            opCodes.Add(opCode);
                        }
                    }
                    opCodes.Add(
                        new OpCode(Instruction.PopItemsOfSameTypePushList)
                    );
                    break;
                
                case TokenType.Tuple:
                    tempArr = CompileValue(
                        (subChild as CompoundToken).Children[2]
                            as CompoundToken
                    );
                    foreach(var opCode in tempArr) {
                        opCodes.Add(opCode);
                    }
                    tempArr = CompileValue(
                        (subChild as CompoundToken).Children[1]
                            as CompoundToken
                    );
                    foreach(var opCode in tempArr) {
                        opCodes.Add(opCode);
                    }
                    opCodes.Add(new OpCode(Instruction.Pop2PushTuple));
                    break;
            }

            return opCodes.ToArray();
        }
    }
}