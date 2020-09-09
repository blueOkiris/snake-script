using System.Collections.Generic;

namespace snakescript {
    enum OpCode {

    }

    struct Function {
        public VmValueType[] InputType;
        public VmValueType[] OutputType;
        public string Name;
        public OpCode[] OpCodes;

        public Function(
                string name, 
                VmValueType[] inputType, VmValueType[] outputType,
                OpCode[] opCodes) {
            Name = name;
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
        public VmValueType[] Type;

        public VmValue(VmValueType[] type) {
            Type = type;
        }
    }

    class VmChar : VmValue {
        public char Value;

        public VmChar(char value)
                : base(new VmValueType[] { VmValueType.Chr }) {
            Value = value;
        }
    }

    class VmNum : VmValue {
        public double Value;

        public VmNum(double value)
                : base(new VmValueType[] { VmValueType.Num }) {
            Value = value;
        }
    }

    class VmBool : VmValue {
        public bool Value;

        public VmBool(bool value)
                : base(new VmValueType[] { VmValueType.Bool }) {
            Value = value;
        }
    }

    class VmUnDef : VmValue {
        public VmUnDef()
            : base(new VmValueType[] { VmValueType.UndDef }) {}
    }

    class VmVar : VmValue {
        public string Name;
        public VmValue Value;

        public VmVar(string name)
                : base(new VmValueType[] { VmValueType.Var }) {
            Name = name;
            Value = new VmUnDef();
        }
    }

    class VmList : VmValue {
        public List<VmValue> Values;

        public VmList(VmValueType subType, List<VmValue> values)
                : base(new VmValueType[] { VmValueType.Ls, subType }) {
            Values = values;
        }
    }

    class VmTuple : VmValue {
        public VmValue Item1, Item2;
        
        public VmTuple(
                VmValue item1, VmValue item2,
                VmValueType subType1, VmValueType subType2)
                : base(
                    new VmValueType[] { VmValueType.Tup, subType1, subType2 } 
                ) {
            Item1 = item1;
            Item2 = item2;
        }
    }
}