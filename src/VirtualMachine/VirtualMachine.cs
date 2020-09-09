using System;
using System.Collections.Generic;
using System.Text;

namespace snakescript {
    struct VirtualMachine {
        public Stack<VmStackFrame> CallStack;
        public OpCode[] OpCodes;
        public Dictionary<string, Function> Functions;

        public VirtualMachine(OpCode[] opCodes, Function[] functions) {
            CallStack = new Stack<VmStackFrame>();
            OpCodes = opCodes;
            Functions = new Dictionary<string, Function>();
            foreach(var function in functions) {
                Functions.Add(function.Name, function);
            }
        }

        private void execute(OpCode opCode) {
            var localStack = CallStack.Peek().LocalStack;
            var varLookup = CallStack.Peek().CurrentVariables;

            switch(opCode.Inst) {
                case Instruction.Pop: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        localStack.Pop();
                    }
                    break;

                case Instruction.Duplicate: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        localStack.Push(localStack.Peek());
                    }
                    break;

                case Instruction.Swap: {
                        if(localStack.Count < 2) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        var sos = localStack.Pop();
                        localStack.Push(tos);
                        localStack.Push(sos);
                    }
                    break;
                
                case Instruction.Return: {
                        var tos = localStack.Pop();
                        if(!VmValue.ShareType(
                                tos, new VmValue(CallStack.Peek().ReturnTypes)
                        )) {
                            throw new TypeException(
                                CallStack.Peek().ReturnTypes, tos.Types
                            );
                        }
                        CallStack.Pop();
                        CallStack.Peek().LocalStack.Push(tos);
                    }
                    break;
                
                case Instruction.Input: {
                        var input = Console.ReadLine();
                        var chars = new List<VmValue>();
                        for(int i = input.Length - 1; i >= 0; i--) {
                            chars.Add(new VmChar(input[i]));
                        }
                        localStack.Push(new VmList(VmValueType.Chr, chars));
                    }
                    break;
                
                case Instruction.Print: {
                        var tos = localStack.Pop();
                        Console.Write(tos.ToString());
                    }
                    break;
                
                case Instruction.PopStrPushAny: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        if(!VmValue.ShareType(
                                tos,
                                new VmValue(
                                    new VmValueType[] {
                                        VmValueType.Ls, VmValueType.Chr
                                    }
                                )
                            )
                        ) {
                            throw new TypeException(
                                new VmValueType[] {
                                    VmValueType.Ls, VmValueType.Chr
                                }, tos.Types
                            );
                        }
                        var tosStr = new StringBuilder();
                        for(int i = 0; i < (tos as VmList).Values.Count; i++) {
                            var val = (tos as VmList).Values[i];
                            tosStr.Append((val as VmChar).Value);
                        }

                        var tokens = Lexer.Tokens(tosStr.ToString());
                        CompoundToken valueAst = Parser.BuildProgram(tokens);
                        foreach(CompoundToken stmt in valueAst.Children) {
                            if(stmt.Type != TokenType.Stmt) {
                                throw new Exception("Expected stmt in input");
                            }

                            foreach(CompoundToken value in stmt.Children) {
                                if(value.Type != TokenType.Value) {
                                    throw new Exception(
                                        "Expected value in input"
                                    );
                                }
                                var valCodes = Compiler.CompileValue(value);
                                foreach(var inst in valCodes) {
                                    execute(inst);
                                }
                            }
                        }
                    }
                    break;
                
                case Instruction.PushNum: {
                        var valueStr = opCode.Argument;
                        var num = new VmNum(double.Parse(valueStr));

                        localStack.Push(num);
                    }
                    break;
                
                case Instruction.PushChar: {
                        var valueStr = opCode.Argument.Substring(
                            1, opCode.Argument.Length - 2
                        );
                        if(valueStr[0] == '\\') {
                            switch(valueStr[1]) {
                                case '\'':
                                    localStack.Push(new VmChar('\''));
                                    break;
                                
                                case 'n':
                                    localStack.Push(new VmChar('\n'));
                                    break;
                                
                                case 't':
                                    localStack.Push(new VmChar('\t'));
                                    break;
                                
                                case 'r':
                                    localStack.Push(new VmChar('\r'));
                                    break;
                                
                                case '\\':
                                    localStack.Push(new VmChar('\\'));
                                    break;
                            }
                        } else {
                            localStack.Push(new VmChar(valueStr[0]));
                        }
                    }
                    break;
                
                case Instruction.PushBool: {
                        var valueStr = opCode.Argument;

                        if(valueStr == "?t") {
                            var chr = new VmBool(true);
                            localStack.Push(chr);
                        } else if(valueStr == "?f") {
                            var chr = new VmBool(false);
                            localStack.Push(chr);
                        }
                    }
                    break;
                
                case Instruction.PushIdent: {
                        var varName = opCode.Argument;
                        if(!varLookup.ContainsKey(varName)) {
                            varLookup.Add(varName, new VmVar(varName));
                        } else {
                            localStack.Push(varLookup[varName].Value);
                        }
                    }
                    break;
                
                case Instruction.FuncCall: {
                        var tos = localStack.Pop();
                        var newLocal = new Stack<VmValue>();
                        newLocal.Push(tos);

                        var function = Functions[opCode.Argument];
                        if(!VmValue.ShareType(
                                tos, new VmValue(function.OutputTypes))) {
                            throw new TypeException(
                                function.OutputTypes, tos.Types
                            );
                        }

                        CallStack.Push(
                            new VmStackFrame(
                                newLocal, function.OpCodes, function.OutputTypes
                            )
                        );
                    }
                    break;
            }
        }

        public void Run() {
            CallStack.Push(
                new VmStackFrame(
                    new Stack<VmValue>(), OpCodes, new VmValueType[] {}
                )
            );

            while(CallStack.Count > 0) {
                if(CallStack.Peek().OpCodes.Count > 0) {
                    execute(CallStack.Peek().OpCodes.Pop());
                } else {
                    CallStack.Pop();
                }
            }
        }
    }
}