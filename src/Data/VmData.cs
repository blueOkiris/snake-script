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
        WhileStart, WhileEnd, FuncCall,

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
        public List<OpCode> OpCodes;
        public VmValueType[] ReturnTypes;
        public int InstructionCounter;

        public VmStackFrame(
                Stack<VmValue> initialStack, OpCode[] opCodes,
                VmValueType[] returnTypes) {
            LocalStack = initialStack;
            CurrentVariables = new Dictionary<string, VmVar>();
            
            OpCodes = new List<OpCode>();
            for(int i = 0; i < opCodes.Length; i++) {
                OpCodes.Add(opCodes[i]);
            }

            ReturnTypes = returnTypes;
            InstructionCounter = 0;
        }
    }

    enum VmValueType {
        Chr, Num, Bool, Var,
        Ls, Tup, UndDef
    }

    class VmValue {
        public VmValueType[] Types;

        public VmValue(VmValueType[] types) {
            Types = types;
        }

        public static bool ShareType(VmValue val1, VmValue val2) {
            if(val1.Types.Length != val2.Types.Length) {
                return false;
            }

            for(int i = 0; i < val1.Types.Length; i++) {
                if(val1.Types[i] != val2.Types[i]) {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() {
            return "generic value";
        }
    }

    class VmChar : VmValue {
        public char Value;

        public VmChar(char value)
                : base(new VmValueType[] { VmValueType.Chr }) {
            Value = value;
        }

        public override string ToString() {
            return "" + Value;
        }
    }

    class VmNum : VmValue {
        public double Value;

        public VmNum(double value)
                : base(new VmValueType[] { VmValueType.Num }) {
            Value = value;
        }

        public override string ToString() {
            return "" + Value;
        }
    }

    class VmBool : VmValue {
        public bool Value;

        public VmBool(bool value)
                : base(new VmValueType[] { VmValueType.Bool }) {
            Value = value;
        }

        public override string ToString() {
            return Value ? "true" : "false";
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

        public override string ToString() {
            return "undefined";
        }
    }

    class VmList : VmValue {
        public List<VmValue> Values;

        public VmList(VmValueType subType, List<VmValue> values)
                : base(new VmValueType[] { VmValueType.Ls, subType }) {
            Values = values;
        }

        public override string ToString() {
            if(Types[1] == VmValueType.Chr) {
                var strStr = new StringBuilder();
                foreach(VmChar chr in Values) {
                    strStr.Append(chr);
                }
                return strStr.ToString();
            } else {
                var listStr = new StringBuilder();
                listStr.Append('{');
                foreach(var value in Values) {
                    listStr.Append(' ');
                    listStr.Append(value.ToString());
                    if(value != Values[Values.Count - 1]) {
                        listStr.Append(',');
                    }
                }
                listStr.Append(" }");
                return listStr.ToString();
            }
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

        public override string ToString() {
            var tupStr = new StringBuilder();
            tupStr.Append("( ");
            tupStr.Append(Item1.ToString());
            tupStr.Append(", ");
            tupStr.Append(Item2.ToString());
            tupStr.Append(" )");
            return tupStr.ToString();
        }
    }
}