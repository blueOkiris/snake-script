using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace snakescript {
    enum TokenType {
        RawTypeName, Ident, Num, Bool, Char, WhileOp, Return, StackOp,
        ListTupleOp, MathOp, BoolOp, MakeTuple, ToStr, ToChr, ParseStr, ToBool,
        Str, FuncOp, LBracket, RBracket, LParenth, RParenth, LBrace, RBrace,
        TypeOp,
        
        TypeName, List, Tuple, Value, Op, FuncCall, Stmt, While, Body, FuncDef,

        Program
    }

    class TokenRegExs {
        public static Dictionary<Regex, TokenType> PatternsToTypes =
                new Dictionary<Regex, TokenType>() {
            { 
                new Regex(@"'(\\.|[^'])'", RegexOptions.Compiled),
                TokenType.Char
            }, { 
                new Regex(@"'(\\.|[^'])*'", RegexOptions.Compiled),
                TokenType.Str
            }, { 
                new Regex(@"([#@])|(\?\?)", RegexOptions.Compiled),
                TokenType.RawTypeName
            }, { 
                new Regex(@"[A-Za-z_]+", RegexOptions.Compiled),
                TokenType.Ident
            }, { 
                new Regex(
                    @"([0-9]+\.?|[0-9]*\.[0-9]+)(e[0-9]+)?",
                    RegexOptions.Compiled
                ), TokenType.Num
            }, { 
                new Regex(@"(\?t|\?f)", RegexOptions.Compiled),
                TokenType.Bool
            }, { 
                new Regex(@"\[\?\]", RegexOptions.Compiled),
                TokenType.WhileOp
            }, { 
                new Regex(@"<<", RegexOptions.Compiled),
                TokenType.Return
            }, { 
                new Regex(@"(>>|><|<>)", RegexOptions.Compiled),
                TokenType.StackOp
            }, { 
                new Regex(@"(\+\+|--|@@|\]\[|\[\])", RegexOptions.Compiled),
                TokenType.ListTupleOp
            }, { 
                new Regex(@"[\+\-\*\/\^]", RegexOptions.Compiled),
                TokenType.MathOp
            }, { 
                new Regex(@"\?[><=!&\|]", RegexOptions.Compiled),
                TokenType.BoolOp
            }, { 
                new Regex(@"\(\)", RegexOptions.Compiled),
                TokenType.MakeTuple
            }, { 
                new Regex(@"\$", RegexOptions.Compiled),
                TokenType.ToStr
            }, { 
                new Regex(@"`", RegexOptions.Compiled),
                TokenType.ToChr
            }, { 
                new Regex(@"!\?", RegexOptions.Compiled),
                TokenType.ParseStr
            }, { 
                new Regex(@"\?", RegexOptions.Compiled),
                TokenType.ToBool
            }, { 
                new Regex(@"\\", RegexOptions.Compiled),
                TokenType.FuncDef
            }, { 
                new Regex(@"\[", RegexOptions.Compiled),
                TokenType.LBracket
            }, { 
                new Regex(@"\]", RegexOptions.Compiled),
                TokenType.RBracket
            }, { 
                new Regex(@"\(", RegexOptions.Compiled),
                TokenType.LParenth
            }, { 
                new Regex(@"\)", RegexOptions.Compiled),
                TokenType.RParenth
            }, { 
                new Regex(@"\{", RegexOptions.Compiled),
                TokenType.LBrace
            }, { 
                new Regex(@"\}", RegexOptions.Compiled),
                TokenType.RBrace
            }, { 
                new Regex(@":", RegexOptions.Compiled),
                TokenType.TypeOp
            }
        };
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
            return
                "Symbol Token: { '" + Source
                    + "', (" + Line + ":" + Pos + "), " + Type + " }";
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
