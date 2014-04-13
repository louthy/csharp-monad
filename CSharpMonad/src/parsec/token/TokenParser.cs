using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token
{
    public class GenTokenParser<A>
        where A : Token
    {
        public readonly Parser<IEnumerable<IdentifierToken>> Identifier;
        public readonly IReadOnlyDictionary<string, Parser<IEnumerable<ReservedToken>>> Reserved;
        public readonly Parser<IEnumerable<OperatorToken>> Operator;
        public readonly IReadOnlyDictionary<string, Parser<IEnumerable<ReservedOpToken>>> ReservedOp;
        public readonly Parser<CharLiteralToken> CharLiteral;
        public readonly Parser<StringLiteralToken> StringLiteral; //TODO
        public readonly Parser<IntegerToken> Natural;
        public readonly Parser<IntegerToken> Integer;
        public readonly Parser<FloatToken> Float;
        public readonly Parser<NaturalOrFloatToken> NaturalOrFloat;
        public readonly Parser<IntegerToken> Decimal;
        public readonly Parser<IntegerToken> Hexadecimal;
        public readonly Parser<IntegerToken> Octal;
        public readonly Func<string, Parser<SymbolToken>> Symbol;
        public readonly Func<Parser<A>, Parser<A>> Lexeme;
        public readonly Parser<IEnumerable<ParserChar>> WhiteSpace;
        public readonly Func<Parser<IEnumerable<A>>, Parser<IEnumerable<A>>> Parens;
        public readonly Func<Parser<IEnumerable<A>>, Parser<IEnumerable<A>>> Braces;
        public readonly Func<Parser<IEnumerable<A>>, Parser<IEnumerable<A>>> Angles;
        public readonly Func<Parser<IEnumerable<A>>, Parser<IEnumerable<A>>> Brackets;
        public readonly Symbol Semi;
        public readonly Symbol Comma;
        public readonly Symbol Colon;
        public readonly Symbol Dot;
        public readonly Func<Parser<A>, Parser<IEnumerable<A>>> SemiSep;
        public readonly Func<Parser<A>, Parser<IEnumerable<A>>> SemiSep1;
        public readonly Func<Parser<A>, Parser<IEnumerable<A>>> CommaSep;
        public readonly Func<Parser<A>, Parser<IEnumerable<A>>> CommaSep1;

        public GenTokenParser(GeneralLanguageDef def)
        {
            Identifier = Tok.Id.Identifier(def);
            Reserved = def.ReservedNames.ToDictionary(name => name, name => Tok.Id.Reserved(name, def) as Parser<IEnumerable<ReservedToken>>);

            Operator = Tok.Ops.Operator(def);
            ReservedOp = def.ReservedOpNames.ToDictionary(name => name, name => Tok.Ops.ReservedOp(name, def) as Parser<IEnumerable<ReservedOpToken>>);

            CharLiteral = Tok.Chars.CharLiteral();

            // stringLiteral = Tok.Strings.StringLiteral() TODO

            Natural = Tok.Numbers.Natural();
            Integer = Tok.Numbers.Integer();
            
            // floating = Tok.Numbers.Floating(); TODO
            // naturalOrFloat = Tok.Numbers.NaturalOrFloating(); TODO

            // TODO - Use the proper whiteSpace code from Text-Parsec-Token
            WhiteSpace = New.WhiteSpace() as Parser<IEnumerable<ParserChar>>;  

            Decimal = Tok.Numbers.Decimal();
            Hexadecimal = Tok.Numbers.Hexadecimal();
            Octal = Tok.Numbers.Octal();
            Symbol = (string name) => Tok.Symbol(name);
            Lexeme = (Parser<A> p) => Tok.Lexeme(p);
            Parens = (Parser<IEnumerable<A>> p) => Tok.Bracketing.Parens(p);
            Braces = (Parser<IEnumerable<A>> p) => Tok.Bracketing.Braces(p);
            Angles = (Parser<IEnumerable<A>> p) => Tok.Bracketing.Angles(p);
            Brackets = (Parser<IEnumerable<A>> p) => Tok.Bracketing.Brackets(p);
            Semi = Tok.Symbol(";");
            Comma = Tok.Symbol(",");
            Colon = Tok.Symbol(":");
            Dot = Tok.Symbol(".");
            CommaSep = (Parser<A> p) => New.SepBy(p, Comma);
            SemiSep = (Parser<A> p) => New.SepBy(p, Semi);
            CommaSep1 = (Parser<A> p) => New.SepBy1(p, Comma);
            SemiSep1 = (Parser<A> p) => New.SepBy1(p, Semi);
        }
    }

    /// <summary>
    /// This is ugly as hell
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class TokenParser : Parser<IEnumerable<Token>>
    {
        public TokenParser(GeneralLanguageDef def)
            :
            base(
                inp =>
                {
                    List<Token> tokens = new List<Token>();

                    var current = inp;
                    var lexer = new GenTokenParser<Token>(def);

                    start:

                    while (current.Count() > 0)
                    {
                        {
                            // Identifier
                            var ids = lexer.Identifier.Parse(current);
                            if (!ids.IsFaulted && ids.Value.Count() > 0)
                            {
                                var fst = ids.Value.First();
                                if (fst.Item1.Count() > 0)
                                {
                                    tokens.Add(fst.Item1.First());
                                    current = fst.Item2;
                                    goto start;
                                }
                            }
                        }

                        {
                            // Reserved
                            foreach (var reserved in lexer.Reserved.Values)
                            {
                                var res = reserved.Parse(current);
                                if (!res.IsFaulted && res.Value.Count() > 0)
                                {
                                    var fst = res.Value.First();
                                    if (fst.Item1.Count() > 0)
                                    {
                                        tokens.Add(fst.Item1.First());
                                        current = fst.Item2;
                                        goto start;
                                    }
                                }
                            }
                        }

                        {
                            // Operator
                            var ops = lexer.Operator.Parse(current);
                            if (!ops.IsFaulted && ops.Value.Count() > 0)
                            {
                                var fst = ops.Value.First();
                                if (fst.Item1.Count() > 0)
                                {
                                    tokens.Add(fst.Item1.First());
                                    current = fst.Item2;
                                    goto start;
                                }
                            }
                        }

                        {
                            // ReservedOps
                            foreach (var reservedOp in lexer.ReservedOp.Values)
                            {
                                var resOp = reservedOp.Parse(current);
                                if (!resOp.IsFaulted && resOp.Value.Count() > 0)
                                {
                                    var fst = resOp.Value.First();
                                    if (fst.Item1.Count() > 0)
                                    {
                                        tokens.Add(fst.Item1.First());
                                        current = fst.Item2;
                                        goto start;
                                    }
                                }
                            }
                        }

                        {
                            // CharLiteral
                            var cl = lexer.CharLiteral.Parse(current);
                            if (!cl.IsFaulted && cl.Value.Count() > 0)
                            {
                                var fst = cl.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            /*
                            // StringLiteral
                            var cl = lexer.StringLiteral.Parse(current);
                            if (!cl.IsFaulted && cl.Value.Count() > 0)
                            {
                                var fst = cl.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }*/
                        }

                        {
                            /*
                            // Float
                            var res = lexer.Float.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }*/
                        }

                        {
                            // Integer
                            var res = lexer.Integer.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Natural
                            var res = lexer.Natural.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Hexadecimal
                            var res = lexer.Hexadecimal.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Octal
                            var res = lexer.Octal.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Decimal
                            var res = lexer.Decimal.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Parens
                            var res = lexer.Parens(new TokenParser(def)).Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.AddRange(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Parens
                            var res = lexer.Parens(new TokenParser(def)).Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(new ParensToken(fst.Item1, fst.Item1.First().Location));
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Braces
                            var res = lexer.Braces(new TokenParser(def)).Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(new BracesToken(fst.Item1, fst.Item1.First().Location));
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // What if they're used in operators?
                            // Brackets
                            var res = lexer.Brackets(new TokenParser(def)).Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(new BracketsToken(fst.Item1, fst.Item1.First().Location));
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // What if they're used in operators?
                            // Angles
                            var res = lexer.Angles(new TokenParser(def)).Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(new AnglesToken(fst.Item1, fst.Item1.First().Location));
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Semi
                            var res = lexer.Semi.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Dot
                            var res = lexer.Dot.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Comma
                            var res = lexer.Comma.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }
                        }

                        {
                            // Colon
                            var res = lexer.Comma.Parse(current);
                            if (!res.IsFaulted && res.Value.Count() > 0)
                            {
                                var fst = res.Value.First();
                                tokens.Add(fst.Item1);
                                current = fst.Item2;
                                goto start;
                            }

                            if (res.IsFaulted)
                            {
                                return ParserResult.Fail<IEnumerable<Token>>(ParserError.Create("unexpected", current));
                            }
                        }
                    }
                    return new ParserResult<IEnumerable<Token>>(
                        Tuple.Create<IEnumerable<Token>, IEnumerable<ParserChar>>(
                            tokens,
                            new ParserChar[0]).Cons()
                        );
                }
            )
        {
        }
    }

}
