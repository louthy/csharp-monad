csharp-monad
============

Library of monads and a full set of parser combinators based on the Haskell Parsec library.

* `Either<R,L>`
* `EitherStrict<R,L>`
* `IO<T>`
* `Option<T>`
* `OptionStrict<T>`
* `Parser<T>`
* `Reader<E,T>`
* `State<S,T>`
* `Try<T>`
* `Writer<W,T>`


The library is stable, functional and pretty well tested.


### A note about laziness

All of the monads in this library (except for those ending in `Strict`) are either delegates or they are wrappers for delegates (as in the case of the `Parser<T>`).  They all require invoking in one way or another to get to the underlying value.  This could cause performance problems if you're not careful.  For example, the `Option<T>` monad has `Value()` and `HasValue()` extension methods:

```C#
        Option<T> option = from x in DoSomething()
                           from y in DoSomethingElse()
                           select x + y;
        
        if( option.HasValue() )
        {
                return option.Value();
        }
```
`HasValue()` and `Value()` will both cause the LINQ expression above to be invoked.  Therefore you end up doing the same computation twice.  You can mitigate this by invoking the result once:
```C#
        Option<T> option = from x in DoSomething()
                           from y in DoSomethingElse()
                           select x + y;
                           
        OptionResult<T> result = option();          // This invokes the bind function
        
        if( result.HasValue )
        {
            return result.Value;        
        }
```
Or by using the `Memo()` memoization extension method available on all of the non-strict monad types:
```C#
        Func<OptionResult<T>> result = (from x in DoSomething()
                                        from y in DoSomethingElse()
                                        select x + y)
                                       .Memo();
        
        if( result().HasValue )
        {
            return result().Value;        
        }
```
Or by either using the `Match` methods on each monad (see the documentation after this section):
```C#
        Func<int> res = (from x in DoSomething()
                         from y in DoSomethingElse()
                         select x + y)
                        .Match(
                             Just: v => v * 10,
                             Nothing: 0
                        );
```
Note that even `Match` uses laziness, but the testing for valid values is now encapsulated into a single expression.  You would still need to be careful when using the result `res`. 

All of these are valid methods, they're designed to fit the various scenarios in which you may need them.  You may wonder why do this at all?  The primary benefit of using laziness is that you can avoid doing calculations that aren't required, this allows you to build a more expression oriented system rather than the standard if-then-thatness of imperative programming.

You can always collapse the laziness by invoking the monad delegate, so you can have the best of both worlds. 


## Either monad

The `Either` monad represents values with two possibilities: a value of `Left` or `Right`.
`Either` is sometimes used to represent a value which is either correct or an error, by convention, `Left` is used to hold an error value `Right` is used to hold a correct value.

`Either` has a very close relationship to the `Try` monad (`Left` is `Exception` on the `Try` monad) and the `Option` monad (`Left` is `Nothing`).  However, the `Either` monad won't capture exceptions and can represent a concrete value as an alternative.  `Either` would primarily be used for known errors  rather than exceptional ones.

Once the `Either` monad is in the `Left` state it cancels the monad bind function and returns immediately.

__Example__

First we set up some methods that return either a `Left` or a `Right`.  In this case `Two()` returns a `Right`, and `Error()` returns a `Left`.

```C#    
        public Either<int, string> Two()
        {
            return () => 2;
        }
    
        public Either<int, string> Error()
        {
            return () => "Error!!";
        }
```

Below are some examples of using `Either<R,L>`.  Note, whenever a `Left` is returned it cancels the entire bind operation, so any functions after the `Left` will not be processed.
    
```C#            
        var r =
            from lhs in Two()
            from rhs in Two()
            select lhs+rhs;
    
        Assert.IsTrue(r.IsRight() && r.Right() == 4);

        var r =
            from lhs in Two()
            from mid in Error()
            from rhs in Two()
            select lhs+mid+rhs;
            
        Assert.IsTrue(r.IsLeft() && r.Left() == "Error!!");
```

You can also use the pattern matching methods to project the either value or to delegate to handlers.

__Example__

```C#
        // Delegate with named properties
        var unit =
            (from lhs in Two()
             from rhs in Two()
             select lhs + rhs)
            .Match(
                Right: r => Assert.IsTrue(r == 4),
                Left: l => Assert.IsFalse(true)
            );
            
        // Delegate without named properties
        var unit =
            (from lhs in Two()
             from rhs in Two()
             select lhs + rhs)
            .Match(
                right => Assert.IsTrue(right == 4),
                left => Assert.IsFalse(true)
            );        

        // Projection with named properties
        var result =
            (from lhs in Two()
             from rhs in Two()
             select lhs + rhs)
            .Match(
                Right: r => r * 2,
                Left: l => 0
            );
            
        Assert.IsTrue(result == 8);
        
        // Projection without named properties
        var result =
            (from lhs in Two()
             from rhs in Two()
             select lhs + rhs)
            .Match(
                r => r * 2,
                l => 0
            );
            
        Assert.IsTrue(result == 8);
```


## IO monad

The IO monad may be seen as unnecessary in C# where everything has side-effects, but it can be useful for chaining IO calls and lazy-loading, however I think its main benefit is as a programmer warning of the potential non-repeatable nature of a method.

__Example__

```C#
        private static IO<Unit> DeleteFile(string tmpFileName)
        {
            return () => Unit.Return( () => File.Delete(tmpFileName) );  // Unit.Return is used to wrap the 'void' return
        }

        private static IO<string> ReadFile(string tmpFileName)
        {
            return () => File.ReadAllText(tmpFileName);
        }

        private static IO<Unit> WriteFile(string tmpFileName, string data)
        {
            return () => Unit.Return( () => File.WriteAllText(tmpFileName, data) );
        }

        private static IO<string> GetTempFileName()
        {
            return () => Path.GetTempFileName();
        }
        
        string data = "Testing 123";

        var result = from tmpFileName   in GetTempFileName()
                     from _             in WriteFile(tmpFileName, data)
                     from dataFromFile  in ReadFile(tmpFileName)
                     from __            in DeleteFile(tmpFileName)
                     select dataFromFile;

        Assert.IsTrue(result.Invoke() == "Testing 123");
```
## Option monad 

If you're thinking of returning `null`, don't.  Use `Option<T>`.  It works a bit like `Nullable<T>` but it works with reference types too and implements the monad bind function.  The bind is cancelled as soon as `Option<T>.Nothing` is returned by any method.  `Option` is also known as the `Maybe` monad.
```C#
        result = from o in MaybeGetAnInt()
                 from o2 in Option<int>.Nothing
                 select o2;
                 

        public Option<int> MaybeGetAnInt()
        {
            var rnd = new Random(); 
            
            return () =>
                 Math.Abs(rnd.Next() % 10) > 5
                        ? rnd.ToOption()
                        : Option<int>.Nothing;
        }
```
You can check the result by looking at the HasValue() property, however each access to `HasValue()`, `Value()`, etc will re-invoke the option function, so it's best to match on the result, or call `GetValueOrDefault`.

```C#
        var result = MaybeGetAnInt().Match(
                        Just: v => v * 10,
                        Nothing: 0
                     );
```

## Parsec

Based on the Haskell Parsec library, this monad allows composition of parsers.  There is a whole library of parsers from reading a single character up to processing expressions and operator associativty.  The library is very stable.  

__Roadmap for this feature__:
* More unit tests
* Floating point number parsers
* Speed improvements (the example below, which is pretty damn complex, can parse 800 lines of source in 750ms - which isn't bad, but can be improved)
* Implement the rest of the usefel parsers from the Parsec lib (almost everything you'll need is in the package already - check the static `Prim`, `Tok` and `Ex` helpers - but it would be nice to have the full set)


__Example__


```C#
            // Inspired by http://www.stephendiehl.com/llvm/

            Parser<Term> exprlazy = null;
            Parser<Term> expr = Prim.Lazy<Term>(() => exprlazy);
            Func<Parser<Term>,Parser<ImmutableList<Term>>> many = Prim.Many;
            Func<Parser<Term>,Parser<Term>> @try = Prim.Try;

            var def = new Lang();
            var lexer = Tok.MakeTokenParser<Term>(def);
            var binops = BuildOperatorsTable<Term>(lexer);

            // Lexer
            var intlex = lexer.Integer;
            var floatlex = lexer.Float;
            var parens = lexer.Parens;
            var commaSep = lexer.CommaSep;
            var semiSep = lexer.SemiSep;
            var identifier = lexer.Identifier;
            var reserved = lexer.Reserved;
            var reservedOp = lexer.ReservedOp;
            var whiteSpace = lexer.WhiteSpace;

            // Parser
            var integer = from n in intlex
                          select new Integer(n) as Term;

            var variable = from v in identifier
                           select new Var(v) as Term;

            var manyargs = parens(from ts in many(variable)
                                  select new Arguments(ts) as Term);

            var commaSepExpr = parens(from cs in commaSep(expr)
                                      select new Exprs(cs) as Term);

            var function = from resv in reserved("def")
                           from name in identifier
                           from args in manyargs
                           from body in expr
                           select new Function(name, args, body) as Term;

            var externFn = from resv in reserved("extern")
                           from name in identifier
                           from args in manyargs
                           select new Extern(name, args) as Term;

            var call = from name in identifier
                       from args in commaSepExpr
                       select new Call(name, args as Exprs) as Term;

            var subexpr = (from p in parens(expr)
                           select new Expression(p) as Term);

            var factor = from f in @try(integer)
                         | @try(externFn)
                         | @try(function)
                         | @try(call)
                         | @try(variable)
                         | subexpr
                         select f;

            var defn = from f in @try(externFn)
                       | @try(function)
                       | @try(expr)
                       select f;

            var toplevel = from ts in many(
                               from fn in defn
                               from semi in reservedOp(";")
                               select fn
                           )
                           select ts;

            exprlazy = Ex.BuildExpressionParser<Term>(binops, factor);

            var text = @"def foo(x y) x+foo(y, 4);
                         def foo(x y) x+y*2;
                         def foo(x y) x+y;
                         extern sin(a);";

            var result = toplevel.Parse(text);
```

For the full version of this, including the definition of the operator table, see LexerTests.cs in the UnitTest project.

## Reader

The `Reader<E,T>` monad is for passing an initial 'environment' state through the bind function,  Each stage will recieve the same `E` environment reference (ideally you should make it immutable to be pure - it's not supposed to be a state monad).

__Example__

First let's set up a class that will hold our environment:

```C#
        class Person
        {
            public string Name;
            public string Surname;
        }
```
Now let's create a couple of methods that extract values from the reader monad.
```C#
        private static Reader<Person, string> Name()
        {
            return env => env.Name;
        }

        private static Reader<Person, string> Surname()
        {
            return env => env.Surname;
        }
```
Next see how we can use those methods and the environment class (Person) in a monadic bind function.

```C#
       var person = new Person { Name = "Joe", Surname = "Bloggs" };

       var reader = from n in Name()
                    from s in Surname()
                    select n + " " + s;

       Assert.IsTrue(reader(person) == "Joe Bloggs");
```

Note how the `person` is passed to the reader at the end.  That invokes the bind function using the environment.

Here's another example mixing both the underlying value `10` and the environment `Person`:

```C#
            var person = new Person { Name = "Joe", Surname = "Bloggs" };

            var initial = Reader.Return<Person,int>(10);

            var reader = from x in initial
                         from p in Reader.Ask<Person>()
                         let nl = p.Name.Length
                         let sl = p.Surname.Length
                         select nl * sl * x;

            Assert.IsTrue(reader(person) == 180);
```

## State

Pass in some initial state which can be 'mutated' through the bind function.  In reality the state isn't mutated, as each stage returns a new instance.  A `StateResult<S,T>` is used to facilitate the passing of state and the underlying monad value.  `Item1` is the state, `Item2` is the monadic value.  

If you take a look at the example below, you should see that both the underlying `int` value and the `string` state are being manipulated in the same expression.  

```C#
            var first  = State.Return<string,int>(10);
            var second = State.Return<string,int>(3);
            var third  = State.Return<string,int>(5);
            var fourth = State.Return<string,int>(100);

            var sm = from x in first
                     from t in State.Get<string>( s => s + "yyy" )
                     from y in second
                     from s in State.Put("Hello " + (x * y) + t)
                     from z in third
                     from w in fourth
                     from s1 in State.Get<string>()
                     from s2 in State.Put( s1 + " " + (z * w) )
                     select x * y * z * w;

            var res = sm(", World"); // Invoke with the initial state

            Assert.IsTrue(res.State == "Hello 30, Worldyyy 500");
            Assert.IsTrue(res.Value == 15000);

```
        

## Try monad

Used for computations which may fail or throw exceptions.  Failure records information about the cause/location of the failure (in the `Exception` property).  Failure values bypass the bound function.  Useful for building computations from sequences of functions that may fail or using exception handling to structure error handling.

You can check if an exception was thrown by testing `IsFaulted` on the `ErrorResult<T>` returned from the invocation (or by using the `Match` methods).

__Example__

```C#
        private Try<int> DoSomething(int value)
        {
            return () => value + 1;
        }

        private Try<int> DoSomethingError(int value)
        {
            return () =>
            {
                throw new Exception("Whoops");
            };
        }

        private Try<int> DoNotEverEnterThisFunction(int value)
        {
            return () => return 10000;
        }
        
        
        var monad = (from val1 in DoSomething(10)
                     from val2 in DoSomethingError(val1)
                     from val3 in DoNotEverEnterThisFunction(val2)
                     select val3);
                  
        var result = monad();

        Console.WriteLine(result.IsFaulted ? result.Exception.Message : "Success");
```

Note, if you're using the `Try<T>` monad outside of a LINQ expression then you will need to append .Try() to safely invoke the wrapped function.  i.e.

```C#
        var value = DoSomethingError().Try();
```

You can pattern match on the result to make it simpler:

```C#
        var value = DoSomethingError()
                        .Match( 
                                Success: v => v 
                                Fail: err => ...
                        );
```

## Writer monad

__Documentation coming soon__

*Here's a quick example*

```C#
        var res = (from a in LogNumber(3)
                   from b in LogNumber(5)
                   from _ in Writer.Tell("Gonna multiply these two")
                   select a * b)
                  .Memo();

        Assert.IsTrue(res().Value == 15 && res().Output.Count() == 3);
        Assert.IsTrue(res().Output.First() == "Got number: 3");
        Assert.IsTrue(res().Output.Skip(1).First() == "Got number: 5");
        Assert.IsTrue(res().Output.Skip(2).First() == "Gonna multiply these two");


        private static Writer<string,int> LogNumber(int num)
        {
            return () => Writer.Tell(num, "Got number: " + num);
        }
```
