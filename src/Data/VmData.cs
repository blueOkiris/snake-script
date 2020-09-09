using System.Collections.Generic;
using System.Text;

namespace snakescript {
    enum Instruction {
        // Ops
        Pop, Duplicate, Swap,
        Pop2PushSum, Pop2PushDifference, Pop2PushProduct, Pop2PushQuotient,
        Pop2PushPower,
        Pop2PushEqual, Pop2PushGreaterThan, Pop2PushLessThan, Pop1PushNot,
        Pop2PushAnd, Pop2PushOr,
        Pop2PushConcat, Pop2PushWithRemovedInd, Pop2PushListPushListAtInd,
        PopListPushUnzipped, PopItemsOfSameTypePushList,
        Print, Input,
        Round,

        Return,

        // Conversions
        PopAnyPushStr, PopNumPushChr, PopNumChrPushBool, Pop2PushTuple,
        PopStrPushAny,

        // Flow control
        Label, Goto, FuncCall,

        // Values
        PushNum, PushChar, PushBool, PushIdent
    }

    struct OpCode {
        public Instruction Inst;
        public string Argument;

        public OpCode(Instruction inst, string arg = "") {
            Inst = inst;
            Argument = arg;
        }

        public override string ToString() {
            var opCodeStr = new StringBuilder();

            if(Argument != "") {
                opCodeStr.Append("( ");
                opCodeStr.Append(Inst);
                opCodeStr.Append(", '");
                opCodeStr.Append(Argument);
                opCodeStr.Append("' )");
            } else {
                opCodeStr.Append(Inst);
            }

            return opCodeStr.ToString();
        }
    }

    struct Function {
        public VmValueType[] InputTypes;
        public VmValueType[] OutputTypes;
        public string Name;
        public OpCode[] OpCodes;

        public Function(
                string name, 
                VmValueType[] inputTypes, VmValueType[] outputTypes,
                OpCode[] opCodes) {
            Name = name;
            InputTypes = inputTypes;
            OutputTypes = outputTypes;
            OpCodes = opCodes;
        }

        public override string ToString() {
            var funcStr = new StringBuilder();

            funcStr.Append("Function ");
            funcStr.Append(Name);
            funcStr.Append(" : {");
            foreach(var type in InputTypes) {
                funcStr.Append(' ');
                funcStr.Append(type);
                if(type != InputTypes[InputTypes.Length - 1]) {
                    funcStr.Append(',');
                }
            }
            funcStr.Append(" } -> {");
            foreach(var type in OutputTypes) {
                funcStr.Append(' ');
                funcStr.Append(type);
                if(type != InputTypes[InputTypes.Length - 1]) {
                    funcStr.Append(',');
                }
            }
            funcStr.Append(" } {\n");
            foreach(var opCode in OpCodes) {
                funcStr.Append("     ");
                funcStr.Append(opCode);
                funcStr.Append('\n');
            }
            funcStr.Append('}');

            return funcStr.ToString();
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