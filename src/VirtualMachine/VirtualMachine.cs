using System.Collections.Generic;

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
                
                case Instruction.PushNum: {
                        var valueStr = opCode.Argument;
                        var num = new VmNum(double.Parse(valueStr));

                        localStack.Push(num);
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