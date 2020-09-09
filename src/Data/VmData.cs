using System.Collections.Generic;

namespace snakescript {
    enum OpCode {

    }

    struct Function {
        public VmValueType InputType;
        public VmValueType OutputType;
        public OpCode[] OpCodes;

        public Function(
                VmValueType inputType, VmValueType outputType,
                OpCode[] opCodes) {
            InputType = inputType;
            OutputType = outputType;
            OpCodes = opCodes;
        }
    }

    struct VmStackFrame {
        public Dictionary<string, VmVar> CurrentVariables;
        public Stack<VmValue> LocalStack;

        public VmStackFrame(Stack<VmValue> initialStack) {
            LocalStack = initialStack;
            CurrentVariables = new Dictionary<string, VmVar>();
        }
    }

    enum VmValueType {
        Chr, Num, Bool, Var,
        Ls, Tup, UndDef
    }

    class VmValue {
        public VmValueType Type;

        public VmValue(VmValueType type) {
            Type = type;
        }
    }

    class VmChar : VmValue {
        public char Value;

        public VmChar(char value) : base(VmValueType.Chr) {
            Value = value;
        }
    }

    class VmNum : VmValue {
        public double Value;

        public VmNum(double value) : base(VmValueType.Num) {
            Value = value;
        }
    }

    class VmBool : VmValue {
        public bool Value;

        public VmBool(bool value) : base(VmValueType.Bool) {
            Value = value;
        }
    }

    class VmUnDef : VmValue {
        public VmUnDef() : base(VmValueType.UndDef) {}
    }

    class VmVar : VmValue {
        public string Name;
        public VmValue Value;

        public VmVar(string name) : base(VmValueType.Var) {
            Name = name;
            Value = new VmUnDef();
        }
    }

    class VmList<T> : VmValue {
        public List<T> Values;
        public VmValueType SubType;

        public VmList(VmValueType subType, List<T> values)
                : base(VmValueType.Ls) {
            Values = values;
            SubType = subType;
        }
    }

    class VmTuple<T1, T2> : VmValue {
        public T1 Item1;
        public T2 Item2;
        
        public VmTuple(T1 item1, T2 item2) : base(VmValueType.Tup) {
            Item1 = item1;
            Item2 = item2;
        }
    }
}