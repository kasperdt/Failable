using System.Security.Cryptography.X509Certificates;

namespace FailableLib;


public abstract class FailableException : Exception
{
    public FailableException(string message) : base(message)
    {
    }
}

public class ExpectedFailureException : FailableException
{
    public Type Type { get; }
    public object? Value { get; }
    public ExpectedFailureException(Type type, object? value) : base($"Expected failure but got value {value ?? "null"} of type {type} instead.")
    {
        Type = type;
        Value = value;
    }
}

public class ExpectedFailureException<T> : ExpectedFailureException
{
    public ExpectedFailureException(object? value) : base(value?.GetType() ?? typeof(T), value)
    {
    }
}

public readonly struct Failable<T>
{
    public T? MaybeOk { get; }

    public T Ok => IsOk ? MaybeOk! : throw MaybeFailure!;

    public Exception? MaybeFailure { get; }
    public Exception Failure => IsFailure ? MaybeFailure! : throw new ExpectedFailureException<T>(MaybeOk);

    public bool IsFailure { get; }
    public bool IsOk => !IsFailure;
    public Failable(Exception failure)
    {
        MaybeFailure = failure;
        IsFailure = true;
    }

    public Failable(T ok)
    {
        MaybeOk = ok;
    }

    public static implicit operator Failable<T>(T ok) => new(ok);
    public static implicit operator Failable<T>(Exception failure) => new(failure);
    public static implicit operator T(Failable<T> failable) => failable.Ok;
    public static implicit operator Exception(Failable<T> failable) => failable.Failure; //?


    public void ThrowIfFailure()
    {
        if (IsFailure) throw MaybeFailure!;
    }


    public TNew Match<TNew>(Func<T, TNew> ok, Func<Exception, TNew> failure)
        => IsOk ? ok(Ok) : failure(Failure);

    public void Switch(Action<T> ok, Action<Exception> failure)
    {
        if (IsOk) ok(Ok);
        else failure(Failure);
    }



    public Failable<TNew> IfOk<TNew>(Func<T, Failable<TNew>> ok) //?
        => IsOk ? ok(Ok) : Failure;

    public async Task<Failable<TNew>> IfOkAsync<TNew>(Func<T, Task<Failable<TNew>>> okAsync) 
        => IsOk ? await okAsync(Ok) : Failure;

    public Failable<T> IfFailure(Func<Exception, Exception> failure)
        => IsOk ? Ok : failure(Failure);
}

public static class Failable
{

    public static Failable<T> Try<T>(Func<Failable<T>> toTry)
    {
        try
        {
            return toTry();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}

public static class FailableExtensions
{
    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<Task<T>> tasks)
    {
        HashSet<Task<T>> activeTasks = new(tasks);

        while (activeTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(activeTasks);
            activeTasks.Remove(completedTask);
            yield return await completedTask;
        }
    }

    public static void SwitchMany<T>(this IEnumerable<Failable<T>> failables, Action<T> oks, Action<Exception> failures)
    {
        foreach (var failable in failables)
            failable.Switch(oks, failures);
    }

    public static async Task SwitchManyAsync<T>(this IEnumerable<Task<Failable<T>>> failables, Action<T> oks, Action<Exception> failures)
    {
        await foreach (var failable in failables.ToAsyncEnumerable())
            failable.Switch(oks, failures);
    }


    public static (IEnumerable<T> Oks, IEnumerable<Exception> Failures) Split<T>(this IEnumerable<Failable<T>> failables)
    {
        List<T> oks = new();
        List<Exception> failures = new();
        failables.SwitchMany(oks.Add, failures.Add);
        return (oks, failures);
    }

    public static Failable<IEnumerable<T>> Compress<T>(this IEnumerable<Failable<T>> failables)
    {
        List<T> oks = new();
        foreach(var failable in failables)
        {
            if (failable.IsFailure) return failable.Failure;
            oks.Add(failable);
        }
        return oks;
    }

    

    public static async Task<Failable<IEnumerable<T>>> CompressAsync<T>(this IEnumerable<Task<Failable<T>>> failables)
    {
        List<T> oks = new();
        await foreach (var failable in failables.ToAsyncEnumerable())
        {
            if (failable.IsFailure) return failable.Failure;
            oks.Add(failable);
        }
        return oks;
    }
}
