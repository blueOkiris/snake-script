using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace snakescript {
    class Lexer {
        private static bool isEven(int n) => n % 2 == 0;

        public static string DeSnakeCode(string code) {
            var newCode = new StringBuilder();

            var lines = code.Split('\n');
            // I just wanted to see if I could do this in a python style lol
            foreach(var ind in Enumerable.Range(0, lines.Length)) {
                if(isEven(ind)) {
                    newCode.Append(lines[ind]);
                } else {
                    newCode.Append(
                        lines[ind].ToCharArray().Reverse().ToArray()
                    );
                }
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
}