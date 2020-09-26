using System;
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
        SetVar,
        Print, Input, WriteFile, ReadFile,
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

    class VmValue : IComparable<VmValue> {
        public VmValueType[] Types;

        public VmValue(VmValueType[] types) {
            Types = types;
        }

        public static bool ShareType(VmValue val1, VmValue val2) {
            var compTypes1 = val1.Types;
            var compTypes2 = val2.Types;
            if(compTypes1[0] == VmValueType.Var) {
                compTypes1 = (val1 as VmVar).Value.Types;
            }
            if(compTypes2[0] == VmValueType.Var) {
                compTypes2 = (val2 as VmVar).Value.Types;
            }
            if(compTypes1.Length != compTypes2.Length) {
                return false;
            }

            for(int i = 0; i < compTypes1.Length; i++) {
                if(compTypes1[i] != compTypes2[i]) {
                    return false;
                }
            }

            return true;
        }

        public int CompareTo(VmValue other) {
            if(Types[0] == VmValueType.Ls) {
                if((this as VmList).Values.Count
                        != (other as VmList).Values.Count) {
                    return 0;
                } else {
                    var comp = 0;
                    for(int i = 0; i < (this as VmList).Values.Count; i++) {
                        comp += (this as VmList).Values[i].CompareTo(
                            (other as VmList).Values[i]
                        );
                    }

                    return comp;
                }
            } else if(Types[0] == VmValueType.Tup) {
                var comp1 = (this as VmTuple).Item1.CompareTo(
                    (other as VmTuple).Item1
                );
                var comp2 = (this as VmTuple).Item1.CompareTo(
                    (other as VmTuple).Item1
                );

                return comp1 + comp2;
            } else if(other.Types[0] == VmValueType.Var
                    && Types[0] == VmValueType.Var) {
                return (this as VmVar).Value.CompareTo((other as VmVar).Value);
            } else if(Types[0] == VmValueType.Var) {
                return (this as VmVar).Value.CompareTo(other);
            } else if(other.Types[0] == VmValueType.Var) {
                return CompareTo((other as VmVar).Value);
            } else {
                switch(Types[0]) {
                    case VmValueType.Num:
                        return (this as VmNum).Value.CompareTo(
                            (other as VmNum).Value
                        );
                    case VmValueType.Chr:
                        return (this as VmNum).Value.CompareTo(
                            (other as VmNum).Value
                        );
                    case VmValueType.Bool:
                        return (this as VmNum).Value.CompareTo(
                            (other as VmNum).Value
                        );
                }
            }

            return 0;
        }

        public virtual object GetValue() {
            return null;
        }

        public override string ToString() {
            return "generic value";
        }

        public static VmValue FromObject(object obj) {
            if(obj is double) {
                return new VmNum((double) obj);
            } else if(obj is char) {
                return new VmChar((char) obj);
            } else if(obj is bool) {
                return new VmBool((bool) obj);
            } else if(obj is List<object>) {
                var values = new List<VmValue>();
                var subTypes = new List<VmValueType>();
                foreach(var subObj in (obj as List<object>)) {
                    var val = VmValue.FromObject(subObj);
                    if(subTypes.Count < 1) {
                        foreach(var type in val.Types) {
                            subTypes.Add(type);
                        }
                    }
                    values.Add(val);
                }
                return new VmList(subTypes.ToArray(), values);
            } else if(obj is Tuple<object, object>) {
                var item1 = VmValue.FromObject(
                    (obj as Tuple<object, object>).Item1
                );
                var item2 = VmValue.FromObject(
                    (obj as Tuple<object, object>).Item2
                );
                return new VmTuple(item1, item2, item1.Types, item2.Types);
            } else {
                return new VmUnDef();
            }
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

        public override object GetValue() {
            return Value;
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

        public override object GetValue() {
            return Value;
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

        public override object GetValue() {
            return Value;
        }
    }

    class VmUnDef : VmValue {
        public VmUnDef()
            : base(new VmValueType[] { VmValueType.UndDef }) {}

        public override string ToString() {
            return "undefined";
        }

        public override object GetValue() {
            return null;
        }
    }

    class VmVar : VmValue {
        public string Name;
        public VmValue Value;

        public VmVar(string name)
                : base(new VmValueType[] { VmValueType.UndDef }) {
            Name = name;
            Value = new VmUnDef();
        }

        public override string ToString() {
            if(Types[0] == VmValueType.UndDef) {
                return "var";
            } else {
                return Value.ToString();
            }
        }

        public override object GetValue() {
            return Value;
        }
    }

    class VmList : VmValue {
        public List<VmValue> Values;

        public VmList(VmValueType[] subTypes, List<VmValue> values)
                : base(new VmValueType[] { VmValueType.Ls }) {
            Values = values;
            var types = new List<VmValueType>();
            types.Add(VmValueType.Ls);
            foreach(var type in subTypes) {
                types.Add(type);
            }
            Types = types.ToArray();
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

        public override object GetValue() {
            var listValue = new List<object>();
            foreach(var value in Values) {
                listValue.Add(value);
            }
            return listValue;
        }
    }

    class VmTuple : VmValue {
        public VmValue Item1, Item2;
        
        public VmTuple(
                VmValue item1, VmValue item2,
                VmValueType[] subTypes1, VmValueType[] subTypes2)
                : base(
                    new VmValueType[] { VmValueType.Tup } 
                ) {
            Item1 = item1;
            Item2 = item2;

            var types = new List<VmValueType>();
            types.Add(VmValueType.Tup);
            foreach(var type in subTypes1) {
                types.Add(type);
            }
            foreach(var type in subTypes2) {
                types.Add(type);
            }
            Types = types.ToArray();
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

        public override object GetValue() {
            return ( Item1, Item2 );
        }
    }
}