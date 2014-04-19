csharp-monad
============

Library of monads for C#:

* `Either<R,L>`
* `EitherStrict<R,L>`
* `IO<T>`
* `Option<T>`
* `OptionStrict<T>`
* `Parser<T>`
* `Reader<E,T>`
* `State<S,T>`
* `Try<T>`


The library is functional and pretty well tested, but it's in active development, so as you can see documentation is pretty sparse right now and everything is liable to change.

The Token section of the parser components currently don't have enough unit-tests to be confident enough in its robustness.  So be cautious and expect bugs.

### A note about laziness

All of the monads in this library (except for those ending in `Strict`) are either delegates or (as in the case of the `Parser<T>`), they are wrappers for delegates.  They all require invoking in one way or another to get to the underlying value.  This could cause performance problems if you're not careful.  For example, the `Option<T>` monad has `Value()` and `HasValue()` extension methods:

```C#
        Option<T> option = from x in DoSomething()
                           from y in DoSomethingElse()
                           select x + y;
        
        if( option.HasValue() )
        {
                return option.Value();
        }
```
`HasValue()` and `Value()` will both cause the LINQ expression above to be invoked.  Therefore you end up doing twice as much work for no reason.  You can mitigate this by invoking the result once:
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
Note that even `Match` uses laziness, but the testing for valid values is now encapsulated.  You would still need to be careful when using the `res`. 

All of them are valid methods, they're designed to fit the various scenarios that you may need them for.  You may wonder why do this at all?  The primary benefit of using laziness is that you can avoid doing calculations that aren't required, this allows you to build a more expression oriented system rather than the standard if-then-thatness of imperative programming.

You can always collapse the laziness by invoking the monad delegate, so you can have the best of both worlds.


## Either monad

The `Either` monad represents values with two possibilities: a value of `Left` or `Right`.
`Either` is sometimes used to represent a value which is either correct or an error, by convention, `Left` is used to hold an error value `Right` is used to hold a correct value.

So you can see that Either has a very close relationship to the `Try` monad.  However, the `Either` monad won't capture exceptions.  `Either` would primarily be used for known error values rather than exceptional ones.

Once the `Either` monad is in the `Left` state it cancels the monad bind function and returns immediately.

__Example__

First we set up some methods that return either a `Left` or a `Right`.  In this case `Two()` returns a `Right`, and `Error()` returns a `Left`.

```C#    
        public Either<int, string> Two()
        {
            return Either.Right<int, string>(2);
        }
    
        public Either<int, string> Error()
        {
            return Either.Left<int, string>("Error!!");
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
            (from one in Two()
             from two in Two()
             select one + two)
            .Match(
                Right: r => Assert.IsTrue(r == 4),
                Left: l => Assert.IsFalse(true)
            );
            
        // Delegate without named properties
        var unit =
            (from one in Two()
             from two in Two()
             select one + two)
            .Match(
                right => Assert.IsTrue(right == 4),
                left => Assert.IsFalse(true)
            );        

        // Projection with named properties
        var result =
            (from one in Two()
             from two in Two()
             select one + two)
            .Match(
                Right: r => r * 2,
                Left: l => 0
            );
            
        Assert.IsTrue(result == 8);
        
        // Projection without named properties
        var result =
            (from one in Two()
             from two in Two()
             select one + two)
            .Match(
                r => r * 2,
                l => 0
            );
            
        Assert.IsTrue(result == 8);
```


## IO monad

The IO monad may be seen as unnecessary in C# where everything has side-effects, but it can be useful for chaining IO calls and lazy-loading, however I think it's main benefit is as a programmer warning of the potential non-repeatable nature of a method.

__Example__

```C#
        private static IO<Unit> DeleteFile(string tmpFileName)
        {
            return () =>
                Unit.Return( () => File.Delete(tmpFileName) );
        }

        private static IO<string> ReadFile(string tmpFileName)
        {
            return () => File.ReadAllText(tmpFileName);
        }

        private static IO<Unit> WriteFile(string tmpFileName, string data)
        {
            return () =>
                Unit.Return( () => File.WriteAllText(tmpFileName, data) );
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

If you're thinking of returning null, don't.  Use `Option<T>`.  It works a bit like `Nullable<T>` but it works with reference types too and implements the monad bind function.  The bind is cancelled as soon as `Option<T>.Nothing` is returned by any method.  `Option` is known as the `Maybe` monad.
```C#
        result = from o in MaybeGetAnInt()
                 from o2 in Option<int>.Nothing
                 select o2;
                 

        public Option<int> MaybeGetAnInt()
        {
            var rnd = Math.Abs(new Random().Next());
        
            return (rnd % 10) > 5
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

This is all work in progress, but very stable and functional.  It's probably easiest to check the unit test code for examples of usage.  Here's a very simple expression parser:

```C#
    public class TestExpr
    {
        public void ExpressionTests()
        {
            var ten = Eval("2*3+4");

            Assert.IsTrue(ten == 10);

            var fourteen = Eval("2*(3+4)");

            Assert.IsTrue(fourteen == 14);
        }

        public int Eval(string expr)
        {
            var r = New.Expr().Parse(expr);
            if (r.Count() == 0)
            {
                throw new Exception("Invalid expression");
            }
            else
            {
                return r.First().Item1;
            }
        }
    }

    public class New
    {
        public static Expr Expr()
        {
            return new Expr();
        }
        public static Term Term()
        {
            return new Term();
        }
        public static Factor Factor()
        {
            return new Factor();
        }
    }

    public class Expr : Parser<int>
    {
        public Expr()
            :
            base(
                inp => (from t in New.Term()
                        from e in
                            (from plus in Gen.Character('+')
                             from expr in New.Expr()
                             select expr)
                             | Gen.Return<int>(0)
                        select t + e)
                       .Parse(inp)
            )
        { }
    }

    public class Term : Parser<int>
    {
        public Term()
            :
            base(
                inp => (from f in New.Factor()
                        from t in
                            (from mult in Gen.Character('*')
                             from term in New.Term()
                             select term)
                             | Gen.Return<int>(1)
                        select f * t)
                       .Parse(inp)
            )
        { }
    }

    public class Factor : Parser<int>
    {
        public Factor()
            :
            base(
                inp => (from choice in
                            (from d in Gen.Digit()
                             select Int32.Parse(d.Value.ToString()))
                             | from open in Gen.Character('(')
                               from expr in New.Expr()
                               from close in Gen.Character(')')
                               select expr
                        select choice)
                        .Parse(inp)

            )
        { }

    }
```

I will be updating this library with more parser components for language-parser building.  Watch this space :)


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

## State

__Documentation coming soon__
        

## Try monad

Used for computations which may fail or throw exceptions.  Failure records information about the cause/location of the failure. Failure values bypass the bound function.  Useful for building computations from sequences of functions that may fail or using exception handling to structure error handling.

Use `()` or '.Invoke()' at the end of an expression to invoke the bind function.  You can check if an exception was thrown by testing `IsFaulted` on the `ErrorResult<T>` returned from the invocation (or by using the `Match` methods), the Exception property will hold the thrown exception.

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




