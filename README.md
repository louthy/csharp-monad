csharp-monad
============

Library of monads for C#

The library is stable but it's still in development, so as you can see documentation is pretty sparse right now.


## Error monad

Used for computations which may fail or throw exceptions.  Failure records information about the cause/location of the failure. Failure values bypass the bound function.  Useful for building computations from sequences of functions that may fail or using exception handling to structure error handling.

Use IsFaulted at the end of an expression to check if an exception was thrown, the Exception property will hold the thrown exception.

__Example__

        private Error<int> DoSomething(int value)
        {
            return Error.Return(() =>
                value + 1
            );
        }

        private Error<int> DoSomethingElse(int value)
        {
            return Error.Return(() =>
                value + 10
            );
        }

        private Error<int> DoNotEverEnterThisFunction(int value)
        {
            return Error.Return<int>(() =>
            {
                return 10000;
            });
        }
        
        
        var result = from val1 in DoSomething(10)
                     from val2 in DoSomethingError(val1)
                     from val3 in DoNotEverEnterThisFunction(val2)
                     select val3;        


        Console.WriteLine(result.IsFaulted ? result.Exception.Message : "Success");


## IO monad

The IO monad may be seen as unnecessary in C# where everything has side-effects, but it can be useful for chaining IO calls and lazy-loading, however I think it's main benefit is as a programmer warning of the potential non-repeatable nature of a method.

__Example__

        private static IO<Unit> DeleteFile(string tmpFileName)
        {
            return () =>
                Unit.Return(
                    () => File.Delete(tmpFileName)
                );
        }

        private static IO<string> ReadFile(string tmpFileName)
        {
            return () => File.ReadAllText(tmpFileName);
        }

        private static IO<Unit> WriteFile(string tmpFileName, string data)
        {
            return () =>
                Unit.Return(
                    () => File.WriteAllText(tmpFileName, data)
                );
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

## Option monad 

If you're thinking of returning null, don't.  Use Option<T>.  It works a bit like Nullable<T> but it works with reference types too and implements the monad bind function.  The bind is cancelled as soon as Nothing is returned by any method.  Also known as the Maybe monad.

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

You can check the result by looking at the HasValue property, however an even even nicer way is to use pattern matching for a proper functional expression:

        var result = MaybeGetAnInt().Match(
                        Just: v => v * 10,
                        Nothing: 0
                     );



## Parsec monad

*documentation coming soon*

