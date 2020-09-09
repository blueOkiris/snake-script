using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace snakescript {
    partial class Parser {
        // <op> ::= <stack-op> | <math-op> | <bool-op> | <ls-tp-op>
        private static bool isOperator(TokenType type) {
            return type == TokenType.StackOp || type == TokenType.MathOp
                || type == TokenType.ListTupleOp || type == TokenType.BoolOp
                || type == TokenType.RoundOp || type == TokenType.IoOp;
        }

        // <stmt> ::= <op> | <while> | <func-call> | <return> | <value>
        //          | <to-str> | <to-chr> | <mk-tup> | <parse-str> | <to-bool>
        private static bool isStmt(TokenType type) {
            return type == TokenType.WhileOp || type == TokenType.LParenth
                || type == TokenType.Return || isOperator(type)
                || type == TokenType.ToStr || type == TokenType.ToChr
                || type == TokenType.MakeTuple || type == TokenType.ParseStr
                || type == TokenType.ToBool;
        }

        // <value> ::= <ident> | <char> | <num> | <bool>
        //              | <str> | <list> | <tuple>
        private static bool isValue(TokenType type) {
            return type == TokenType.Ident || type == TokenType.Char
                || type == TokenType.Num || type == TokenType.Bool
                || type == TokenType.Str || type == TokenType.LBracket
                || type == TokenType.LParenth;
        }

        // <value> ::= <ident> | <char> | <num> | <bool> | <str>
        //              | <list> | <tuple>
        private static CompoundToken parseValue(
                ref int i, SymbolToken[] tokens) {
            var children = new List<Token>();

            if(tokens[i].Type == TokenType.LBracket) {
                children.Add(parseList(ref i, tokens));
            } else if(tokens[i].Type == TokenType.LParenth) {
                children.Add(parseTuple(ref i, tokens));
            } else {
                children.Add(tokens[i]);
            }

            return new CompoundToken(TokenType.Value, children.ToArray());
        }

        // <tuple> ::= <l-parenth> <value> <value> <r-parenth>
        private static CompoundToken parseTuple(
                ref int i, SymbolToken[] tokens) {
            var lpar = tokens[i];

            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            var item1 = parseValue(ref i, tokens);

            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            var item2 = parseValue(ref i, tokens);

            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            if(tokens[i].Type != TokenType.RParenth) {
                throw new ExpectedOtherTokenExecteption(
                    new TokenType[] { TokenType.RParenth }, tokens[i].Type,
                    tokens[i].Line, tokens[i].Pos
                );
            }
            var rpar = tokens[i];

            var children = new List<Token>();
            children.Add(lpar);
            children.Add(item1);
            children.Add(item2);
            children.Add(rpar);
            
            return new CompoundToken(TokenType.Tuple, children.ToArray());
        }

        // <list> ::= <l-bracket> { <value> } <r-bracket>
        private static CompoundToken parseList(
                ref int i, SymbolToken[] tokens) {
            var lbracket = tokens[i];

            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            var values = new List<Token>();
            while(tokens[i].Type != TokenType.RBracket) {
                values.Add(parseValue(ref i, tokens));

                i++;
                if(i >= tokens.Length) {
                    throw new UnexpectedEOFExecteption(
                        tokens[i - 1].Line, tokens[i - 1].Pos
                    );
                }
            }

            var rbracket = tokens[i];

            var children = new List<Token>();
            children.Add(lbracket);
            foreach(var value in values) {
                children.Add(value);
            }
            children.Add(rbracket);

            return new CompoundToken(TokenType.List, children.ToArray());
        }

        // <body> ::= <l-brace> { <stmt> } <r-brace>
        private static CompoundToken parseBody(
                ref int i, SymbolToken[] tokens) {
            if(tokens[i].Type != TokenType.LBrace) {
                throw new ExpectedOtherTokenExecteption(
                    new TokenType[] { TokenType.LBrace }, tokens[i].Type,
                    tokens[i].Line, tokens[i].Pos
                );
            }
            var lbrace = tokens[i];

            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            var stmts = new List<Token>();
            while(tokens[i].Type != TokenType.RBrace) {
                stmts.Add(parseStmt(ref i, tokens));
                i++;
                if(i >= tokens.Length) {
                    throw new UnexpectedEOFExecteption(
                        tokens[i - 1].Line, tokens[i - 1].Pos
                    );
                }
            }
            var rbrace = tokens[i];

            var children = new List<Token>();
            children.Add(lbrace);
            foreach(var stmt in stmts) {
                children.Add(stmt);
            }
            children.Add(rbrace);

            return new CompoundToken(TokenType.Body, children.ToArray());
        }

        // <while> ::= <while-op> <body>
        private static CompoundToken parseWhile(
                ref int i, SymbolToken[] tokens) {
            var whileOp = tokens[i];
            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            var child = parseBody(ref i, tokens);

            return new CompoundToken(
                TokenType.While, new Token[] { whileOp, child }
            );
        }
        
        // <func-call> ::= <l-parenth> <ident> <r-parenth>
        private static CompoundToken parseFuncCall(
                ref int i, SymbolToken[] tokens) {
            var lpar = tokens[i];

            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            if(tokens[i].Type != TokenType.Ident) {
                throw new ExpectedOtherTokenExecteption(
                    new TokenType[] { TokenType.Ident }, tokens[i].Type,
                    tokens[i].Line, tokens[i].Pos
                );
            }
            var name = tokens[i];
            
            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            if(tokens[i].Type != TokenType.RParenth) {
                throw new ExpectedOtherTokenExecteption(
                    new TokenType[] { TokenType.RParenth }, tokens[i].Type,
                    tokens[i].Line, tokens[i].Pos
                );
            }
            var rpar = tokens[i];

            var children = new List<Token>();
            children.Add(lpar);
            children.Add(name);
            children.Add(rpar);

            return new CompoundToken(TokenType.FuncCall, children.ToArray());
        }
        
        // <op> ::= <stack-op> | <math-op> | <bool-op> | <ls-tp-op>
        private static CompoundToken parseOp(
                ref int i, SymbolToken[] tokens) {
            if(!isOperator(tokens[i].Type)) {
                throw new ExpectedOtherTokenExecteption(
                    new TokenType[] { TokenType.Op }, tokens[i].Type,
                    tokens[i].Line, tokens[i].Pos
                );
            }
            return new CompoundToken(TokenType.Op, new Token[] { tokens[i] });
        }
        
        // <type-name> ::= <raw-type-name> | <l-bracket> <type-name> <r-bracket>
        //                  | <l-parenth> <type-name> <type-name> <r-parenth>
        private static CompoundToken parseTypeName(
                ref int i, SymbolToken[] tokens) {
            var children = new List<Token>();

            if(tokens[i].Type == TokenType.RawTypeName) {
                children.Add(tokens[i]);
            } else if(tokens[i].Type == TokenType.LBracket) {
                children.Add(tokens[i]);

                i++;
                if(i >= tokens.Length) {
                    throw new UnexpectedEOFExecteption(
                        tokens[i - 1].Line, tokens[i - 1].Pos
                    );
                }
                children.Add(parseTypeName(ref i, tokens));
                
                i++;
                if(i >= tokens.Length) {
                    throw new UnexpectedEOFExecteption(
                        tokens[i - 1].Line, tokens[i - 1].Pos
                    );
                }
                if(tokens[i].Type != TokenType.RBracket) {
                    throw new ExpectedOtherTokenExecteption(
                        new TokenType[] { TokenType.RBracket }, tokens[i].Type,
                        tokens[i].Line, tokens[i].Pos
                    );
                }
                children.Add(tokens[i]);
            } else if(tokens[i].Type == TokenType.LParenth) {
                children.Add(tokens[i]);

                i++;
                if(i >= tokens.Length) {
                    throw new UnexpectedEOFExecteption(
                        tokens[i - 1].Line, tokens[i - 1].Pos
                    );
                }
                children.Add(parseTypeName(ref i, tokens));
                
                i++;
                if(i >= tokens.Length) {
                    throw new UnexpectedEOFExecteption(
                        tokens[i - 1].Line, tokens[i - 1].Pos
                    );
                }
                if(tokens[i].Type != TokenType.RParenth) {
                    throw new ExpectedOtherTokenExecteption(
                        new TokenType[] { TokenType.RParenth }, tokens[i].Type,
                        tokens[i].Line, tokens[i].Pos
                    );
                }
                children.Add(tokens[i]);
            } else {
                if(tokens[i].Type != TokenType.RParenth) {
                    throw new ExpectedOtherTokenExecteption(
                        new TokenType[] {
                            TokenType.RawTypeName, TokenType.LBracket,
                            TokenType.LParenth 
                        }, tokens[i].Type, tokens[i].Line, tokens[i].Pos
                    );
                }
            }

            return new CompoundToken(TokenType.TypeName, children.ToArray());
        }
    }

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
                new Regex(@"\^\^", RegexOptions.Compiled),
                TokenType.RoundOp
            }, { 
                new Regex(@"[\.,]", RegexOptions.Compiled),
                TokenType.IoOp
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
            }, { 
                new Regex(@">", RegexOptions.Compiled),
                TokenType.ToOp
            }
        };
    }
}
