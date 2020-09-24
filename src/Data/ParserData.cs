using System.Collections.Generic;
using System.Text;

namespace snakescript {
    enum TokenType {
        RawTypeName, Ident, Num, Bool, Char, WhileOp, Return, StackOp,
        ListTupleOp, MathOp, BoolOp, MakeTuple, ToStr, ToChr, ParseStr, ToBool,
        Str, FuncOp, LBracket, RBracket, LParenth, RParenth, LBrace, RBrace,
        TypeOp, ToOp, IoOp, RoundOp, AsgnOp, FileIoOp,
        
        TypeName, List, Tuple, Value, Op, FuncCall, Stmt, While, Body, FuncDef,

        Program
    }

    class Token {
        public TokenType Type;

        public Token(TokenType type) {
            Type = type;
        }
    }

    class SymbolToken : Token {
        public string Source;
        public int Line;
        public int Pos;

        public SymbolToken(TokenType type, string source, int line, int pos)
                : base(type) {
            Source = source;
            Line = line;
            Pos = pos;
        }

        public override string ToString() {
            if(Line % 2 == 0) {
                return
                    "Symbol Token: { '" + Source
                        + "', (" + Line + ":" + Pos + "), " + Type + " }";
            } else {
                return
                    "Symbol Token: { '" + Source
                        + "', (" + Line + ":" + (80 - Pos) + "), " + Type
                        + " }";
            }
        }
    }

    class CompoundToken : Token {
        public Token[] Children;

        public CompoundToken(TokenType type, Token[] children) : base(type) {
            // Make sure we make COPIES of children, not just add their refs
            var childs = new List<Token>();
            foreach(var child in children) {
                Token childCopy;

                if(child is SymbolToken) {
                    childCopy = new SymbolToken(
                        child.Type,
                        (child as SymbolToken).Source,
                        (child as SymbolToken).Line, (child as SymbolToken).Pos
                    );
                } else if(child is CompoundToken) {
                    childCopy = new CompoundToken(
                        child.Type, (child as CompoundToken).Children
                    );
                } else {
                    childCopy = new Token(child.Type);
                }
                childs.Add(childCopy);
            }
            Children = childs.ToArray();
        }

        private string toString(CompoundToken token, int tabInd = -1) {
            var tokenStr = new StringBuilder();
            if(token.Type == TokenType.Program) {
                tokenStr.Append("Program:\n");
            } else {
                for(int i = 0; i < tabInd; i++) {
                    tokenStr.Append("|--");
                }
                tokenStr.Append(token.Type);
                tokenStr.Append('\n');
            }
            foreach(var child in token.Children) {
                if(!(child is CompoundToken)) {
                    for(int i = 0; i < tabInd + 1; i++) {
                        tokenStr.Append("|--");
                    }
                    tokenStr.Append(child.ToString());
                    tokenStr.Append('\n');
                } else {
                    tokenStr.Append(
                        toString(child as CompoundToken, tabInd + 1)
                    );
                }
            }
            return tokenStr.ToString();
        }

        public override string ToString() {
            return toString(this);
        }
    }
}
