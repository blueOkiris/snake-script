using System.Collections.Generic;

namespace snakescript {
    struct VirtualMachine {
        public Stack<VmStackFrame> CallStack;
        public OpCode[] OpCodes;
        public Function[] Functions;

        public VirtualMachine(OpCode[] opCodes, Function[] functions) {
            CallStack = new Stack<VmStackFrame>();
            OpCodes = opCodes;
            Functions = functions;
        }

        private void execute(OpCode opCode) {
            
        }

        public void Run() {
            foreach(var opCode in OpCodes) {
                execute(opCode);
            }
        }
    }
}