/*
 * Original grammar:
 *
 *   Exp ::= Exp "+" Term   // Left recursion = left associativity (+)
 *   Exp ::= Term
 *   Term ::= Term "*" Pow  // Left recursion = left associativity (*)
 *   Term ::= Pow
 *   Pow ::= Fact "^" Pow   // Right recursion = right associativity (^)
 *   Pow ::= Fact
 *   Fact ::= "int"
 *   Fact ::= "(" Exp ")"
 *
 * LL(1) grammar:
 *
 *   Prog ::= Exp "EOF"
 *   Exp ::= Term ("+" Term)*
 *   Term ::= Pow ("*" Pow)*
 *   Pow ::= Fact ("^" Pow)?
 *   Fact ::= "int" | "(" Exp ")"
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public enum TokenCategory {
    INT, PLUS, TIMES, POW, OPEN_PAR, CLOSE_PAR, EOF, BAD_TOKEN
}

public class Token {
    public TokenCategory Category { get; }
    public String Lexeme { get; }

    public Token(TokenCategory category, String lexeme) {
        Category = category;
        Lexeme = lexeme;
    }

    public override String ToString() {
        return $"[{Category}, \"{Lexeme}\"]";
    }
}

public class Scanner {
    readonly String input;
    static readonly Regex regex = new Regex(
        @"(\d+)|([+])|([*])|(\^)|([(])|([)])|(\s)|(.)");

    public Scanner(String input) {
        this.input = input;
    }

    public IEnumerable<Token> Start() {
        foreach (Match m in regex.Matches(input)) {
            if (m.Groups[1].Success) {
                yield return new Token(TokenCategory.INT, m.Value);
            } else if (m.Groups[2].Success) {
                yield return new Token(TokenCategory.PLUS, m.Value);
            } else if (m.Groups[3].Success) {
                yield return new Token(TokenCategory.TIMES, m.Value);
            } else if (m.Groups[4].Success) {
                yield return new Token(TokenCategory.POW, m.Value);
            } else if (m.Groups[5].Success) {
                yield return new Token(TokenCategory.OPEN_PAR, m.Value);
            } else if (m.Groups[6].Success) {
                yield return new Token(TokenCategory.CLOSE_PAR, m.Value);
            } else if (m.Groups[7].Success) {
                // skip
            } else if (m.Groups[8].Success) {
                yield return new Token(TokenCategory.BAD_TOKEN, m.Value);
            }
        }
        yield return new Token(TokenCategory.EOF, null);
    }
}

public class SyntaxError : Exception { }

public class SemanticError : Exception {

    public SemanticError(String message): base(message) { }
}

public class Parser {
    IEnumerator<Token> tokenStream;

    public Parser(IEnumerator<Token> tokenStream) {
        this.tokenStream = tokenStream;
        this.tokenStream.MoveNext();
    }

    public TokenCategory Current {
        get { return tokenStream.Current.Category; }
    }

    public Token Expect(TokenCategory category) {
        if (Current == category) {
            Token current = tokenStream.Current;
            tokenStream.MoveNext();
            return current;
        } else {
            throw new SyntaxError();
        }
    }

    public Node Prog() {
        var result = new Prog() {
            Exp()
        };
        Expect(TokenCategory.EOF);
        return result;
    }

    public Node Exp() {
        var result = Term();
        while (Current == TokenCategory.PLUS) {
            var node = new Plus() {
                AnchorToken = Expect(TokenCategory.PLUS)
            };
            node.Add(result);
            node.Add(Term());
            result = node;
        }
        return result;
    }

    public Node Term() {
        var result = Pow();
        while (Current == TokenCategory.TIMES) {
            var node = new Times() {
                AnchorToken = Expect(TokenCategory.TIMES)
            };
            node.Add(result);
            node.Add(Pow());
            result = node;
        }
        return result;
    }

    public Node Pow() {
        var result = Fact();
        if (Current == TokenCategory.POW) {
            var node = new Pow() {
                AnchorToken = Expect(TokenCategory.POW)
            };
            node.Add(result);
            node.Add(Pow());
            result = node;
        }
        return result;
    }

    public Node Fact() {
        switch(Current) {

        case TokenCategory.INT:
            return new Int() {
                AnchorToken = Expect(TokenCategory.INT)
            };

        case TokenCategory.OPEN_PAR:
            Expect(TokenCategory.OPEN_PAR);
            var result = Exp();
            Expect(TokenCategory.CLOSE_PAR);
            return result;

        default:
            throw new SyntaxError();
        }
    }
}

public class Node: IEnumerable<Node> {

    IList<Node> children = new List<Node>();

    public Node this[int index] {
        get {
            return children[index];
        }
    }

    public Token AnchorToken { get; set; }

    public void Add(Node node) {
        children.Add(node);
    }

    public IEnumerator<Node> GetEnumerator() {
        return children.GetEnumerator();
    }

    System.Collections.IEnumerator
    System.Collections.IEnumerable.GetEnumerator() {
        throw new NotImplementedException();
    }

    public override string ToString() {
        return $"{GetType().Name} {AnchorToken}";
    }

    public string ToStringTree() {
        var sb = new StringBuilder();
        TreeTraversal(this, "", sb);
        return sb.ToString();
    }

    static void TreeTraversal(Node node, string indent, StringBuilder sb) {
        sb.Append(indent);
        sb.Append(node);
        sb.Append('\n');
        foreach (var child in node.children) {
            TreeTraversal(child, indent + "  ", sb);
        }
    }
}

public class Prog:  Node { }
public class Plus:  Node { }
public class Times: Node { }
public class Pow:   Node { }
public class Int:   Node { }

public class EvalVisitor {
    public int Visit(Prog node) {
        return Visit((dynamic) node[0]);
    }

    public int Visit(Plus node) {
        return Visit((dynamic) node[0])
            + Visit((dynamic) node[1]);
    }

    public int Visit(Times node) {
        return Visit((dynamic) node[0])
            * Visit((dynamic) node[1]);
    }

    public int Visit(Pow node) {
        return (int) Math.Pow(
            Visit((dynamic) node[0]),
            Visit((dynamic) node[1]));
    }

    public int Visit(Int node) {
        return Int32.Parse(node.AnchorToken.Lexeme);
    }
}

public class LispVisitor {
    public String Visit(Prog node) {
        return Visit((dynamic) node[0]);
    }

    public String Visit(Plus node) {
        return "(+ "
            + Visit((dynamic) node[0])
            + " "
            + Visit((dynamic) node[1])
            + ")";
    }

    public String Visit(Times node) {
        return "(* "
            + Visit((dynamic) node[0])
            + " "
            + Visit((dynamic) node[1])
            + ")";
    }

    public String Visit(Pow node) {
        return "(expt "
            + Visit((dynamic) node[0])
            + " "
            + Visit((dynamic) node[1])
            + ")";
    }

    public String Visit(Int node) {
        return node.AnchorToken.Lexeme;
    }
}

public class CLangVisitor {
    public String Visit(Prog node) {
        return @"
#include <stdio.h>
#include <math.h>

int main(void) {
    printf(""%d\n"","
        + Visit((dynamic) node[0])
        + @");
    return 0;
}
";
    }

    public String Visit(Plus node) {
        return "("
            + Visit((dynamic) node[0])
            + "+"
            + Visit((dynamic) node[1])
            + ")";
    }

    public String Visit(Times node) {
        return "("
            + Visit((dynamic) node[0])
            + "*"
            + Visit((dynamic) node[1])
            + ")";
    }

    public String Visit(Pow node) {
        return "(int) pow("
            + Visit((dynamic) node[0])
            + ", "
            + Visit((dynamic) node[1])
            + ")";
    }

    public String Visit(Int node) {
        return node.AnchorToken.Lexeme;
    }
}

public class SemanticVisitor {

    public void Visit(Prog node) {
        Visit((dynamic) node[0]);
    }

    public void Visit(Plus node) {
        Visit((dynamic) node[0]);
        Visit((dynamic) node[1]);
    }

    public void Visit(Times node) {
        Visit((dynamic) node[0]);
        Visit((dynamic) node[1]);
    }

    public void Visit(Pow node) {
        Visit((dynamic) node[0]);
        Visit((dynamic) node[1]);
    }

    public void Visit(Int node) {
        int value;
        if (!Int32.TryParse(node.AnchorToken.Lexeme, out value)) {
            throw new SemanticError(
                $"Integer literal too large: {node.AnchorToken.Lexeme}");
        }
    }
}

public class WATVisitor {
    public String Visit(Prog node) {
        return
            "(module\n"
            + "  (import \"math\" \"pow\" (func $pow (param i32) (param i32) (result i32)))\n"
            + "  (func\n"
            + "    (export \"start\")\n"
            + "    (result i32)\n"
            + Visit((dynamic) node[0])
            + "  )\n"
            + ")\n";
    }
    public String Visit(Plus node) {
        return Visit((dynamic) node[0])
            + Visit((dynamic) node[1])
            + "    i32.add\n";
    }
    public String Visit(Times node) {
        return Visit((dynamic) node[0])
            + Visit((dynamic) node[1])
            + "    i32.mul\n";
    }
    public String Visit(Pow node) {
        return Visit((dynamic) node[0])
            + Visit((dynamic) node[1])
            + "    call $pow\n";
    }
    public String Visit(Int node) {
        return $"    i32.const {node.AnchorToken.Lexeme}\n";
    }
}

public class Driver {
    public static void Main() {
        Console.Write("> ");
        var line = Console.ReadLine();
        var parser = new Parser(new Scanner(line).Start().GetEnumerator());
        try {
            var result = parser.Prog();
            // Console.WriteLine(result.ToStringTree());
            // Console.WriteLine(new EvalVisitor().Visit((dynamic) result));
            // Console.WriteLine(new LispVisitor().Visit((dynamic) result));
            // Console.WriteLine(new CLangVisitor().Visit((dynamic) result));
            new SemanticVisitor().Visit((dynamic) result);
            // Console.WriteLine("Semantics OK!");
            Console.WriteLine(new WATVisitor().Visit((dynamic) result));

        } catch (SyntaxError) {
            Console.WriteLine("Bad Syntax!");

        } catch (SemanticError e) {
            Console.WriteLine(e.Message);
        }
    }
}
