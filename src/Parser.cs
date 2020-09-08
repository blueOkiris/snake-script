using System.Collections.Generic;

namespace snakescript {
    class Lexer {
        
        public static SymbolToken[] Tokens(string code) {
            var tokens = new List<SymbolToken>();

            var len = code.Length;
            for(int ind = 0, line = 1, col = 1; ind < len; ind++, col++) {
                if(code[ind] == '\n') {
                    line++;
                    col = 0;
                }

                foreach(var pattern in TokenRegExs.PatternsToTypes.Keys) {
                    var match = pattern.Match(code.Substring(ind));
                    if(match.Success && match.Index == 0) {
                        tokens.Add(
                            new SymbolToken(
                                TokenRegExs.PatternsToTypes[pattern],
                                match.Value, line, col
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