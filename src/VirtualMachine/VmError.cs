using System;
using System.Text;

namespace snakescript {
    class StackUnderflowException : Exception {
        public StackUnderflowException()
                : base("Stack underflow! Not enough items on local stack") {    
        }
    }

    class TypeException : Exception {
        private static string getMessage(
                VmValueType[] expected, VmValueType[] actual) {
            var message = new StringBuilder();
            message.Append("Expected types {");
            foreach(var type in expected) {
                message.Append(' ');
                message.Append(type);
                if(type != expected[expected.Length - 1]) {
                    message.Append(',');
                }
            }
            message.Append(" }, but received types { ");
            foreach(var type in actual) {
                message.Append(' ');
                message.Append(type);
                if(type != actual[actual.Length - 1]) {
                    message.Append(',');
                }
            }
            message.Append(" }");
            return message.ToString();
        }

        public TypeException(VmValueType[] expected, VmValueType[] actual) 
                : base(TypeException.getMessage(expected, actual)) {
        }
    }
}