using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace snakescript {
    partial class Parser {
        // <func-def> ::= <func-op> <ident> <type-op> <type> 
        //                                          <bool-op> <type> <body>
        private static CompoundToken parseFuncDef(
                ref int i, SymbolToken[] tokens) {
            var funcOp = tokens[i];

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
            if(tokens[i].Type != TokenType.TypeOp) {
                throw new ExpectedOtherTokenExecteption(
                    new TokenType[] { TokenType.TypeOp }, tokens[i].Type,
                    tokens[i].Line, tokens[i].Pos
                );
            }
            var typeOp = tokens[i];
            
            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            var typeName = parseTypeName(ref i, tokens);
            
            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            if(tokens[i].Type != TokenType.ToOp) {
                throw new ExpectedOtherTokenExecteption(
                    new TokenType[] { TokenType.ToOp }, tokens[i].Type,
                    tokens[i].Line, tokens[i].Pos
                );
            }
            var to = tokens[i];
            
            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            var retTypeName = parseTypeName(ref i, tokens);

            i++;
            if(i >= tokens.Length) {
                throw new UnexpectedEOFExecteption(
                    tokens[i - 1].Line, tokens[i - 1].Pos
                );
            }
            var body = parseBody(ref i, tokens);

            var children = new List<Token>();
            children.Add(funcOp);
            children.Add(name);
            children.Add(typeOp);
            children.Add(typeName);
            children.Add(to);
            children.Add(retTypeName);
            children.Add(body);

            return new CompoundToken(TokenType.FuncDef, children.ToArray());
        }

        // <stmt> ::= <op> | <while> | <func-call> | <return> | <value>
        //          | <to-str> | <to-chr> | <mk-tup> | <parse-str> | <to-bool>
        private static CompoundToken parseStmt(
                ref int i, SymbolToken[] tokens) {
            var children = new List<Token>();
            if(tokens[i].Type == TokenType.WhileOp) {
                children.Add(parseWhile(ref i, tokens));
            } else if(tokens[i].Type == TokenType.LParenth) {
                children.Add(parseFuncCall(ref i, tokens));
            } else if(tokens[i].Type == TokenType.Return) {
                children.Add(tokens[i]);
            } else if(isOperator(tokens[i].Type)) {
                children.Add(parseOp(ref i, tokens));
            } else if(isValue(tokens[i].Type)) {
                children.Add(parseValue(ref i, tokens));
            } else if(tokens[i].Type == TokenType.ToStr
                    || tokens[i].Type == TokenType.ToChr
                    || tokens[i].Type == TokenType.MakeTuple
                    || tokens[i].Type == TokenType.ParseStr
                    || tokens[i].Type == TokenType.ToBool) {
                children.Add(tokens[i]);
            } else {
                throw new ExpectedOtherTokenExecteption(
                    new TokenType[] {
                        TokenType.While, TokenType.FuncCall, TokenType.Return,
                        TokenType.Op
                    }, tokens[i].Type, tokens[i].Line, tokens[i].Pos
                );
            }
            return new CompoundToken(TokenType.Stmt, children.ToArray());
        }

        // <program> ::= { <func-def> | <stmt> }
        public static CompoundToken BuildProgram(SymbolToken[] tokens) {
            var children = new List<Token>();

            for(int i = 0; i < tokens.Length; i++) {
                if(tokens[i].Type == TokenType.FuncOp) {
                    children.Add(parseFuncDef(ref i, tokens));
                } else {
                    children.Add(parseStmt(ref i, tokens));
                }
            }

            return new CompoundToken(TokenType.Program, children.ToArray());
        }
    }

    partial class Lexer {
        private static bool isEven(int n) => n % 2 == 0;

        public static string DeSnakeCode(string code) {
            var newCode = new StringBuilder();

            var lines = code.Split('\n');
            // I just wanted to see if I could do this in a python style lol
            foreach(var ind in Enumerable.Range(0, lines.Length)) {
                if(lines[ind].Length != 80 
                        && !(ind == lines.Length - 1 && lines[ind] == "")) {
                    throw new Exception(
                        "Incorrect number of characters per line! Expected 80"
                        + " characters on line " + (ind + 1));
                }

                if(isEven(ind)) {
                    newCode.Append(lines[ind]);
                } else {
                    newCode.Append(
                        lines[ind].ToCharArray().Reverse().ToArray()
                    );
                }
                newCode.Append('\n');
            }

            return newCode.ToString();
        }

        public static SymbolToken[] Tokens(string code) {
            var tokens = new List<SymbolToken>();

            var len = code.Length;
            for(int ind = 0, line = 1, col = 1; ind < len; ind++, col++) {
                if(code[ind] == '\n') {
                    line++;
                    col = 0;
                }

                foreach(var pattern in patternsToTypes.Keys) {
                    var match = pattern.Match(code.Substring(ind));
                    if(match.Success && match.Index == 0) {
                        tokens.Add(
                            new SymbolToken(
                                patternsToTypes[pattern], match.Value, line, col
                            )
                        );
                        col += match.Length - 1;
                        ind += match.Length - 1;
                        break;
                    }
                }
            }

            return tokens.ToArray();
        }
    }
}