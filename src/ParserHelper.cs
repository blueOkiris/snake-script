using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace snakescript {
    partial class Lexer {
        private static Dictionary<Regex, TokenType> patternsToTypes =
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
                TokenType.FuncOp
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
}
