using System;
using System.Text;

namespace snakescript {
    class ParserException : Exception {
        public ParserException(string message)
                : base(message) {
        }
    }

    class UnexpectedEOFExecteption : ParserException {
        public UnexpectedEOFExecteption(int line, int pos)
                : base("Unexpected EOF at line " + line + ", pos " + pos) {
        }
    }

    class ExpectedOtherTokenExecteption : ParserException {
        private static string expectedTokenTypes(TokenType[] types) {
            if(types.Length < 1) {
                return "";
            } else if(types.Length < 2) {
                return types[0].ToString();
            } else if(types.Length < 3) {
                return types[0] + " or " + types[1];
            }

            var listStr = new StringBuilder();
            foreach(var type in types) {
                if(type != types[types.Length - 1]) {
                    listStr.Append(type + ", ");
                } else {
                    listStr.Append("or ");
                    listStr.Append(type);
                }
            }
            return listStr.ToString();
        }

        public ExpectedOtherTokenExecteption(
                TokenType[] expected, TokenType actual, int line, int pos
            ) : base(
                "Expected "
                + ExpectedOtherTokenExecteption.expectedTokenTypes(expected)
                + " but received " + actual + " at line " + line + ", pos "
                + pos
            ) {
        }
    }
}