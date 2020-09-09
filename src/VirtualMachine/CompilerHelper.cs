using System.Collections.Generic;

namespace snakescript {
    partial class Compiler {
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
                case TokenType.StackOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case ">>":
                            opCodes.Add(OpCode.Pop);
                            break;
                        
                        case "><":
                            opCodes.Add(OpCode.Duplicate);
                            break;
                        
                        case "<>":
                            opCodes.Add(OpCode.Swap);
                            break;
                    }
                    break;
                
                case TokenType.MathOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case "+":
                            opCodes.Add(OpCode.Pop2PushSum);
                            break;
                        
                        case "-":
                            opCodes.Add(OpCode.Pop2PushDifference);
                            break;
                        
                        case "*":
                            opCodes.Add(OpCode.Pop2PushProduct);
                            break;
                        
                        case "^":
                            opCodes.Add(OpCode.Pop2PushPower);
                            break;
                    }
                    break;
                
                case TokenType.BoolOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case "?=":
                            opCodes.Add(OpCode.Pop2PushEqual);
                            break;
                        
                        case "?>":
                            opCodes.Add(OpCode.Pop2PushGreaterThan);
                            break;
                        
                        case "?<":
                            opCodes.Add(OpCode.Pop2PushLessThan);
                            break;
                        
                        case "?!":
                            opCodes.Add(OpCode.Pop1PushNot);
                            break;
                        
                        case "?&":
                            opCodes.Add(OpCode.Pop2PushAnd);
                            break;
                        
                        case "?|":
                            opCodes.Add(OpCode.Pop2PushOr);
                            break;
                    }
                    break;
                
                case TokenType.ListTupleOp:
                    switch((op.Children[0] as SymbolToken).Source) {
                        case "++":
                            opCodes.Add(OpCode.Pop2PushConcat);
                            break;
                        
                        case "--":
                            opCodes.Add(
                                OpCode.Pop2PushWithRemovedInd
                            );
                            break;
                        
                        case "@@":
                            opCodes.Add(
                                OpCode.Pop2PushListPushListAtInd
                            );
                            break;
                        
                        case "[]":
                            opCodes.Add(
                                OpCode.PopItemsOfSameTypePushList
                            );
                            break;
                        
                        case "][":
                            opCodes.Add(OpCode.PopListPushUnzipped);
                            break;
                    }
                    break;
            }
            
            return opCodes.ToArray();
        }
    }
}