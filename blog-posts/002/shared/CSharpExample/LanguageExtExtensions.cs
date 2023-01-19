using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace GGCSharp;

public static class LanguageExtExtensions
{
    public static Result<Unit> ToUnitResult<TResult>(this Try<TResult> result)
    {
        return result.Match(r => new Result<Unit>(Unit.Default), e => new Result<Unit>(e));
    }

    public static Result<Unit> ToUnitResult<TResult>(this Eff<TResult> result)
    {
        return result.Run().Match(r => new Result<Unit>(Unit.Default), e => new Result<Unit>(e));
    }

    public static Task<Result<Unit>> ToUnitResult<TResult>(this TryAsync<TResult> result)
    {
        return result.Match(r => new Result<Unit>(Unit.Default), e => new Result<Unit>(e));
    }

    public static Result<T> Flatten<T>(this Result<Result<T>> result)
    {
        return result.Match(succ => succ.Match(a => a, b => new Result<T>(b)),
            fail => new Result<T>(fail));
    }
    
    public static Unit LogException<TResult>(this Result<TResult> result)
    {
        result.IfFail(e => Console.WriteLine(e.ToString()));

        return Unit.Default;
    }

    public static Either<Exception, TResult> LogException<TResult>(this Either<Exception, TResult> either)
    {
        either.IfLeft(e => Console.WriteLine(e.ToString()));

        return either;
    }

    public static EitherAsync<Exception, TResult> LogException<TResult>(
        this EitherAsync<Exception, TResult> eitherAsync)
    {
        eitherAsync.IfLeft(e => Console.WriteLine(e.ToString()));

        return eitherAsync;
    }

    public static Either<Error, TResult> LogException<TResult>(this Either<Error, TResult> either)
    {
        either.IfLeft(e => Console.WriteLine(e.ToString()));

        return either;
    }

    public static EitherAsync<Error, TResult> LogException<TResult>(this EitherAsync<Error, TResult> eitherAsync)
    {
        eitherAsync.IfLeft(e => Console.WriteLine(e.ToString()));

        return eitherAsync;
    }
}