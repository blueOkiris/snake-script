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

        private void execute(OpCode opCode, ref VmStackFrame stackFrame) {
            var localStack = stackFrame.LocalStack;
            var varLookup = stackFrame.CurrentVariables;

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
                
                case Instruction.Input: {
                        var input = Console.ReadLine();
                        var chars = new List<VmValue>();
                        for(int i = 0; i < input.Length; i++) {
                            chars.Add(new VmChar(input[i]));
                        }
                        localStack.Push(new VmList(chars[0].Types, chars));
                    }
                    break;
                
                case Instruction.Print: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        Console.Write(tos.ToString());
                    }
                    break;
                
                case Instruction.PopAnyPushStr: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        var valStr = tos.ToString();
                        var valStrList = new List<VmValue>();
                        foreach(var chr in valStr) {
                            valStrList.Add(new VmChar(chr));
                        }
                        localStack.Push(
                            new VmList(valStrList[0].Types, valStrList)
                        );
                    }
                    break;
            
                case Instruction.PopNumPushChr: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        if(!VmValue.ShareType(tos, new VmNum(0))) {
                            throw new TypeException(
                                new VmValueType[] { VmValueType.Num },
                                tos.Types
                            );
                        }

                        localStack.Push(
                            new VmChar((char) (tos as VmNum).Value)
                        );
                    }
                    break;
            
                case Instruction.Pop2PushTuple: {
                        if(localStack.Count < 2) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        var sos = localStack.Pop();

                        var tup = new VmTuple(tos, sos, tos.Types, sos.Types);
                        localStack.Push(tup);
                    }
                    break;
            
                case Instruction.PopNumChrPushBool: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        if(!VmValue.ShareType(tos, new VmNum(0))
                                && !VmValue.ShareType(tos, new VmChar('0'))) {
                            throw new TypeException(
                                new VmValueType[] { 
                                    VmValueType.Num,
                                    VmValueType.Chr
                                },
                                tos.Types
                            );
                        }

                        if(tos is VmNum) {
                            localStack.Push(
                                new VmBool((tos as VmNum).Value != 0)
                            );
                        } else if(tos is VmChar) {
                            localStack.Push(
                                new VmBool(((int) (tos as VmChar).Value) != 0)
                            );
                        }
                    }
                    break;

                case Instruction.PopStrPushAny: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        if(!VmValue.ShareType(
                                tos, new VmList(
                                    new VmChar(' ').Types, new List<VmValue>()
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
                                    execute(inst, ref stackFrame);
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
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
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

                        CallStack.Push(stackFrame);
                        stackFrame = new VmStackFrame(
                            newLocal, function.OpCodes, function.OutputTypes
                        );
                        stackFrame.InstructionCounter = -1;
                    }
                    break;
                
                case Instruction.Return: {
                        var tos = localStack.Pop();
                        if(!VmValue.ShareType(
                                tos, new VmValue(stackFrame.ReturnTypes))) {
                            throw new TypeException(
                                stackFrame.ReturnTypes, tos.Types
                            );
                        }

                        stackFrame = CallStack.Pop();
                        stackFrame.LocalStack.Push(tos);
                    }
                    break;
                
                case Instruction.WhileStart: {
                        if(localStack.Count < 1) {
                            throw new StackUnderflowException();
                        }
                        var tos = localStack.Pop();
                        if(!VmValue.ShareType(tos, new VmBool(true))) {
                            throw new TypeException(
                                new VmBool(true).Types, tos.Types
                            );
                        }

                        if(!(tos as VmBool).Value) {
                            var endLabelName =
                                opCode.Argument.Replace("WH", "EW");
                            while(stackFrame.OpCodes[
                                        stackFrame.InstructionCounter
                                    ].Inst != Instruction.WhileEnd
                                    || stackFrame.OpCodes[
                                            stackFrame.InstructionCounter
                                    ].Argument != endLabelName) {
                                stackFrame.InstructionCounter++;
                            }
                        }
                    }
                    break;
                
                case Instruction.WhileEnd: {
                        var startLabelName =
                            opCode.Argument.Replace("EW", "WH");
                        while(stackFrame.OpCodes[
                                    stackFrame.InstructionCounter
                                ].Inst != Instruction.WhileStart
                                || stackFrame.OpCodes[
                                        stackFrame.InstructionCounter
                                ].Argument != startLabelName) {
                            stackFrame.InstructionCounter--;
                        }
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
                var stackFrame = CallStack.Pop();
                if(stackFrame.InstructionCounter < stackFrame.OpCodes.Count) {
                    execute(
                        stackFrame.OpCodes[stackFrame.InstructionCounter],
                        ref stackFrame
                    );
                    stackFrame.InstructionCounter++;
                    CallStack.Push(stackFrame);
                }
            }
        }
    }
}