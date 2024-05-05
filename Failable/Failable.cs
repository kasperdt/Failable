using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks.Sources;

namespace FailableLib;


public abstract class FailableException : Exception
{
    public FailableException(string message) : base(message)
    {
    }

    public FailableException(string message, Exception innerException) : base(message, innerException)
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


public class FailureException : FailableException
{
    public Type Type { get; }

    public FailureException(Type type, string message) : base(message)
    {
        Type = type;
    }
}

public class FailureException<T> : FailureException
{
    public FailureException(string message) : base(typeof(T), message)
    {
    }
}

public class UnwrapFailableException : FailableException
{
    public Type Type { get; }

    public UnwrapFailableException(Type type, Exception innerException) : base($"Failed to unrwap failable of type {type}.", innerException)
    {
        Type = type;
    }
}

public class UnwrapFailableException<T> : UnwrapFailableException
{
    public UnwrapFailableException(Exception innerException) : base(typeof(T), innerException)
    {
    }
}

//public delegate IMonad<TType, TNew> MonadicDelegate<T, TType, TNew>(T value);

//public interface IMonad<TType, T>
//{
//    static abstract IMonad<TType, T> Unit(T value); // Monadic return
//    IMonad<TType, TNew> Bind<TNew>(MonadicDelegate<T, TType, TNew> bindOperation);
//}


//public record FailableRecord;
//public record Ok<T>(T Value) : 


//public interface IFailable
//{
//}


//public delegate IFailable<TNew> IfOkAsyncDelegate<T, TNew>(T ok);
public interface IFailable<T> : IEquatable<T>, IEquatable<IFailable<T>>,
    IComparable<T>, IComparable<IFailable<T>>, IComparable
{
    bool Equals(Exception? failure) => TryGetFailure(out var thisFailure) && thisFailure == failure;
    bool IsFailure => TryGetFailure(out _);
    bool IsOk => !IsFailure;
    T? OkOrDefault() => IsOk ? Ok : default;
    Exception? FailureOrDefault() => TryGetFailure(out var failure) ? failure : null;
    bool TryGetOk(out T? ok)
    {
        ok = OkOrDefault();
        return IsOk;
    }

    void Deconstruct(out bool isOk, out T? okOrDefault, out Exception? failureOrDefault)
    {
        isOk = IsOk;
        okOrDefault = OkOrDefault();
        failureOrDefault = FailureOrDefault();
    }
    bool IEquatable<T>.Equals(T? other) 
        => IsOk && Ok is { } ok ? ok.Equals(other) : other is null;


    bool IEquatable<IFailable<T>>.Equals(IFailable<T>? other)
        => other is { } &&
        (other.TryGetFailure(out var of) ? TryGetFailure(out var tf) && of == tf :
        IsOk && EqualityComparer<T>.Default.Equals(Ok, other.Ok));

    int CompareTo(Exception? failure) => 1;

    int IComparable<T>.CompareTo(T? other) => other is null ? 1 : -1;



    int IComparable<IFailable<T>>.CompareTo(IFailable<T>? other) => (this, other) switch
    {
        ({ }, null) => 1,
        ((false, _, _), (true, _, _)) => -1,
        ((true, _, _), (false, _, _)) => 1,
        ((false, _, { }), (false, _, null)) => 1,
        ((false, _, null), (false, _, { })) => -1,
        ((false, _, _), (false, _, _)) => 0,
        ((true, { }, _), (true, null, _)) => 1,
        ((true, null, _), (true, { }, _)) => -1,
        ((true, null, _), (true, null, _)) => 0, 
        ((true, IComparable<T> tOk, _), (true, { } ok, _)) => tOk.CompareTo(ok),
        ((true, IComparable tOk, _), (true, { } ok, _)) => tOk.CompareTo(ok),
        ((true, { } tOk, _), (true, { } ok, _)) => 0,
    };

    int IComparable.CompareTo(object? obj) => obj switch
    {
        null => 1,
        IFailable<T> failable => CompareTo(failable),
        T ok => CompareTo(ok),
        Exception failure => CompareTo(failure),
        var o => throw new ArgumentException($"An instance of IFailable of type {typeof(T)} cannot be compared to an instance of type {o.GetType()}.")
    };


    T Ok { get; }
    bool TryGetFailure(out Exception? failure);

    
    TNew Match<TNew>(Func<T, TNew> ok, Func<Exception, TNew> failure)
        => TryGetFailure(out var f) ? failure(f!) : ok(Ok);    
    void Switch(Action<T> ok, Action<Exception> failure)
    {
        if (TryGetFailure(out var f)) failure(f!);
        else ok(Ok);
    }

    T IfFailure(Func<Exception, T> failure)
        => TryGetFailure(out var f) ? failure(f!) : Ok;
    async Task<T> IfFailureTaskAsync(Func<Exception, Task<T>> failure)
        => TryGetFailure(out var f) ? await failure(f!).ConfigureAwait(false) : Ok;

    async ValueTask<T> IfFailureAsync(Func<Exception, ValueTask<T>> failure)
        => TryGetFailure(out var f) ? await failure(f!).ConfigureAwait(false) : Ok;

    void ThrowIfFailure()
    {
        _ = Ok;
        if (TryGetFailure(out var failure)) throw new UnwrapFailableException<T>(failure!);
    }
}


public static class Failable
{
    public static Failable<T> Ok<T>(T value) => value;
    public static Failable<T> Try<T>(Func<T> toTry)
    {
        try
        {
            return toTry();
        }
        catch(Exception ex)
        {
            return Failable<T>.Catch(ex);
        }
    }
    public static async ValueTask<Failable<T>> TryAsync<T>(Func<ValueTask<T>> toTryAsync)
    {
        try
        {
            return await toTryAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Failable<T>.Catch(ex);
        }
    }

    public static async Task<Failable<T>> TryTaskAsync<T>(Func<Task<T>> toTryAsync)
    {
        try
        {
            return await toTryAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Failable<T>.Catch(ex);
        }
    }



    public static Failable<T> Ternary<T>(bool condition, Func<T> ok, Func<Exception> failure)
    {
        try
        {
            if (condition) return ok();
            return Failable<T>.Fail(failure());
        }
        catch(Exception ex)
        {
            return Failable<T>.Catch(ex);
        }
    }

    public static async ValueTask<Failable<T>> TernaryAsync<T>(bool condition, Func<ValueTask<T>> ok, Func<Exception> failure)
    {
        try
        {
            if (condition) return await ok().ConfigureAwait(false);
            return Failable<T>.Fail(failure());
        }
        catch (Exception ex)
        {
            return Failable<T>.Catch(ex);
        }
    }

    public static async Task<Failable<T>> TernaryTaskAsync<T>(bool condition, Func<Task<T>> ok, Func<Exception> failure)
    {
        try
        {
            if (condition) return await ok().ConfigureAwait(false);
            return Failable<T>.Fail(failure());
        }
        catch (Exception ex)
        {
            return Failable<T>.Catch(ex);
        }
    }




    public static Failable<T> Ternary<T>(bool condition, Func<T> ok, Func<string> failure) 
        => Ternary(condition, ok, () => new FailureException<T>(failure()));

    public static async ValueTask<Failable<T>> TernaryAsync<T>(bool condition, Func<ValueTask<T>> ok, Func<string> failure)
        => await TernaryAsync(condition, async () => await ok().ConfigureAwait(false), () => new FailureException<T>(failure()));

    public static async Task<Failable<T>> TernaryTaskAsync<T>(bool condition, Func<Task<T>> ok, Func<string> failure)
        => await TernaryAsync(condition, async () => await ok().ConfigureAwait(false), () => new FailureException<T>(failure()));
}




public readonly struct Failable<T> : IFailable<T>, 
    IEquatable<Failable<T>>, 
    IComparable<Failable<T>>, 
    IEqualityOperators<Failable<T>, Failable<T>, bool>,
    IEqualityOperators<Failable<T>, IFailable<T>, bool>
{








    T? MaybeOk { get; }

    public T? OkOrDefault() => MaybeOk;

    public T Ok 
    { 
        get 
        { 
            if (IsFailure) throw new UnwrapFailableException<T>(MaybeFailure!); 
            return MaybeOk!; 
        } 
    }

    Exception? MaybeFailure { get; }
    Exception Failure => IsFailure ? MaybeFailure! : throw new ExpectedFailureException<T>(MaybeOk);
    public Exception? FailureOrDefault() => MaybeFailure;



    public bool IsFailure { get; }
    public bool IsOk => !IsFailure;
    private Failable(Exception failure)
    {
        MaybeFailure = failure;
        IsFailure = true;
    }

    public Failable(T ok)
    {
        MaybeOk = ok;
    }


    public static implicit operator Failable<T>(T ok) => new(ok);
    public static implicit operator T(Failable<T> failable) => failable.Ok;
    public static implicit operator Failable<T>(Exception failure) => Fail(failure);

    public static bool operator ==(Failable<T> left, Failable<T> right) => left.Equals(right);

    public static bool operator !=(Failable<T> left, Failable<T> right) => !(left == right);

    public static bool operator ==(Failable<T> left, Exception? right) => left.Equals(right);

    public static bool operator !=(Failable<T> left, Exception? right) => !(left == right);

    public static bool operator ==(Failable<T> left, IFailable<T>? right) => left.Equals(right);

    public static bool operator !=(Failable<T> left, IFailable<T>? right) => !(left == right);


    public static bool operator ==(Failable<T> left, T? right) => left.Equals(right);

    public static bool operator !=(Failable<T> left, T? right) => !(left == right);

    public static bool operator ==(IFailable<T>? left, Failable<T> right) => right == left;

    public static bool operator !=(IFailable<T>? left, Failable<T> right) => !(left == right);

    public static bool operator ==(T? left, Failable<T> right) => (right == left);

    public static bool operator !=(T? left, Failable<T> right) => !(left == right);

    public static bool operator ==(Exception? left, Failable<T> right) => (right == left);

    public static bool operator !=(Exception? left, Failable<T> right) => !(left == right);


    public static Failable<T> Catch(Exception exception) => new(exception);
    public static Failable<T> Fail(Exception failure)
    {
        try
        {
            throw failure;
        }
        catch(Exception e)
        {
            return new(e);
        }
    }

    public static Failable<T> Fail(string failureMessage) => Fail(new FailureException<T>(failureMessage));

    public static Failable<T> Accept(T ok) => new(ok);


    //static IMonad<IFailable, T> IMonad<IFailable, T>.Unit(T value) => (Failable<T>)value;

    //T IFailable<T>.Ok => this;


    //IMonad<IFailable, TNew> IMonad<IFailable, T>.Bind<TNew>(MonadicDelegate<T, IFailable, TNew> bindOperation)
    //{
    //    if (IsOk) return bindOperation(this);
    //    return Failable<TNew>.Catch(Failure);
    //}


    public void Deconstruct(out bool isOk, out T? okOrDefault, out Exception? failureOrDefault)
    {
        isOk = IsOk;
        okOrDefault = OkOrDefault();
        failureOrDefault = FailureOrDefault();
    }

    public TNew Match<TNew>(Func<T, TNew> ok, Func<Exception, TNew> failure)
        => IsOk ? ok(this) : failure(Failure);


    public void Switch(Action<T> ok, Action<Exception> failure)
    {
        if (IsOk) ok(this);
        else failure(Failure);
    }

    //public async Task SwitchAsync(Func<T, Task> ok, Func<Exception, Task> failure)
    //{
    //    if (IsOk) await ok(this);
    //    else await failure(Failure);
    //}


    public void ThrowIfFailure() => _ = Ok;

    public Failable<TNew> IfOkSimple<TNew>(Func<T, TNew> ok)
        => IsOk ? ok(this) : Failable<TNew>.Catch(Failure);

    public async Task<Failable<TNew>> IfOkSimpleTaskAsync<TNew>(Func<T, Task<TNew>> okAsync)
        => IsOk ? await okAsync(this).ConfigureAwait(false) : Failable<TNew>.Catch(Failure);

    public async ValueTask<Failable<TNew>> IfOkSimpleAsync<TNew>(Func<T, ValueTask<TNew>> okAsync)
        => IsOk ? await okAsync(this).ConfigureAwait(false) : Failable<TNew>.Catch(Failure);


    public Failable<TNew> IfOk<TNew>(Func<T, Failable<TNew>> ok) //?
        => IsOk ? ok(this) : Failable<TNew>.Catch(Failure);

    public async Task<Failable<TNew>> IfOkTaskAsync<TNew>(Func<T, Task<Failable<TNew>>> okAsync) 
        => IsOk ? await okAsync(this).ConfigureAwait(false) : Failable<TNew>.Catch(Failure);

    public async ValueTask<Failable<TNew>> IfOkAsync<TNew>(Func<T, ValueTask<Failable<TNew>>> okAsync)
        => IsOk ? await okAsync(this).ConfigureAwait(false) : Failable<TNew>.Catch(Failure);


    public Failable<TNew> IfOkTry<TNew>(Func<T, TNew> ok)
    {
        var t = this;
        return IsOk ? Failable.Try(() => ok(t)) : Failable<TNew>.Catch(Failure);
    }

    public async Task<Failable<TNew>> IfOkTryTaskAsync<TNew>(Func<T, Task<TNew>> okAsync)
    {
        var t = this;
        return IsOk ? await Failable.TryTaskAsync(async () => await okAsync(t).ConfigureAwait(false)) : Failable<TNew>.Catch(Failure);
    }

    public async ValueTask<Failable<TNew>> IfOkTryAsync<TNew>(Func<T, ValueTask<TNew>> okAsync)
    {
        var t = this;
        return IsOk ? await Failable.TryAsync(async () => await okAsync(t).ConfigureAwait(false)) : Failable<TNew>.Catch(Failure);
    }




    //public void IfOkDo(Action<T> ok)
    //{
    //    if (IsOk) ok(Ok);
    //}
    //public async Task IfOkDoAsync(Func<T, Task> ok)
    //{
    //    if (IsOk) await ok(Ok);
    //}



    public bool TryGetFailure(out Exception? failure)
    {
        failure = MaybeFailure;
        return IsFailure;
    }

    public bool TryGetOk(out T? ok)
    {
        ok = MaybeOk;
        return IsOk;
    }

    //internal Failable<T> IfFailue<TEx>(Func<TEx, T> failure) where TEx : Exception 
    //    => IsFailure && Failure is TEx e ? failure(e) : this;

    //internal async Task<Failable<T>> IfFailueAsync<TEx>(Func<TEx, Task<T>> failure) where TEx : Exception 
    //    => IsFailure && Failure is TEx e ? await failure(e) : this;


    //internal Failable<T> IfFailue<TEx>(Func<TEx, bool> predicate, Func<TEx, T> failure) where TEx : Exception
    // => IsFailure && Failure is TEx e && predicate(e) ? failure(e) : this;

    //internal async Task<Failable<T>> IfFailueAsync<TEx>(Func<TEx, bool> predicate, Func<TEx, Task<T>> failure) where TEx : Exception
    //    => IsFailure && Failure is TEx e && predicate(e) ? await failure(e) : this;



    public T IfFailure(Func<Exception, T> failure)
    => IsOk ? (T)this : failure(Failure);

    public async Task<T> IfFailureTaskAsync(Func<Exception, Task<T>> failure)
        => IsOk ? (T)this : await failure(Failure).ConfigureAwait(false);

    public async ValueTask<T> IfFailureAsync(Func<Exception, ValueTask<T>> failure)
        => IsOk ? (T)this : await failure(Failure).ConfigureAwait(false);


    public bool Equals(T? other)
        => IsOk && EqualityComparer<T>.Default.Equals(Ok, other);

    public bool Equals(IFailable<T>? other)
        => other is { } && (other.TryGetFailure(out var of) ? Equals(of) : Equals(other.Ok));

    public bool Equals(Exception? other) => TryGetFailure(out var thisFailure) && thisFailure == other;

    public bool Equals(Failable<T> other)
        => other.TryGetFailure(out var of) ? Equals(of) : Equals(other.Ok);


    public override bool Equals([NotNullWhen(true)] object? obj) => obj switch
    {
        null => false,
        Failable<T> failable => Equals(failable),
        IFailable<T> iFailable => Equals(iFailable),
        T ok => Equals(ok),
        Exception failure => Equals(failure),
        var o => throw new ArgumentException($"An instance of IFailable of type {typeof(T)} cannot be compared to an instance of type {o.GetType()} and value equality-wise.")
    };

    public override int GetHashCode() => HashCode.Combine(typeof(T), IsOk, OkOrDefault(), FailureOrDefault());

    public override string ToString() => Match(
        ok => $"Ok {{ {ToStr(ok)} }}", 
        failure => $"F<{failure?.GetType()?.ToString() ?? "??"}> {{ {ToStr(failure!)} }}");

    static string ToStr(Exception e) => (e, e?.Message) is (not null, { } m) ? $"'{m}'" : "<null>"; 
    static string ToStr(T ok) => ok switch
    {
        null => "<null>",
        string => $"'obj'",
        T => ok.ToString() ?? "<null>"
    };


    int IComparable<Failable<T>>.CompareTo(Failable<T> other)
    {
        return CompareTo<Failable<T>, T>(this, other);
    }

    int CompareTo<TF, TV>(TF value, TF other) where TF : IFailable<TV>
    {
        return value.CompareTo(other);
    }
}




public readonly struct Nil
{
    readonly static Nil _value = new();
    public static ref readonly Nil Value => ref _value;
}



static class AsyncExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<Task<T>> tasks)
    {
        HashSet<Task<T>> activeTasks = new(tasks);

        while (activeTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(activeTasks).ConfigureAwait(false);
            activeTasks.Remove(completedTask);
            yield return await completedTask.ConfigureAwait(false);
        }
    }

    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<ValueTask<T>> valueTasks)
    {
        Queue<Task<T>> tasks = new();
        foreach (var valueTask in valueTasks)
        {
            if (valueTask.IsCompleted) yield return await valueTask.ConfigureAwait(false);
            else tasks.Enqueue(valueTask.AsTask());
        }
        await foreach (var value in ToAsyncEnumerable(tasks).ConfigureAwait(false)) yield return value;
    }

    public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> asyncCollection, Func<T, bool> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach(var item in asyncCollection.ConfigureAwait(false).WithCancellation(cancellationToken)) if (predicate(item)) yield return item;
    }

    public static async IAsyncEnumerable<TNew> Select<T, TNew>(this IAsyncEnumerable<T> asyncCollection, Func<T, TNew> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in asyncCollection.ConfigureAwait(false).WithCancellation(cancellationToken)) yield return selector(item);
    }
}

public static class FailableExtensions
{
    public static void AssertOk(this Failable<Nil> failable) => failable.ThrowIfFailure();
    public static Failable<TNew> IfOk<TNew>(this Failable<Nil> failableVoid, Func<Failable<TNew>> ok) => failableVoid.IfOk(_ => ok());
    public static async Task<Failable<TNew>> IfOkTaskAsync<TNew>(this Failable<Nil> failableVoid, Func<Task<Failable<TNew>>> ok) => await failableVoid.IfOkAsync(async _ => await ok().ConfigureAwait(false));
    public static async ValueTask<Failable<TNew>> IfOkAsync<TNew>(this Failable<Nil> failableVoid, Func<ValueTask<Failable<TNew>>> ok) => await failableVoid.IfOkAsync(async _ => await ok().ConfigureAwait(false));
}

public static class IFailableExtensions
{
    public static void AssertOk(this IFailable<Nil> failable) => failable.ThrowIfFailure();
}

public static class FailablesExtensions
{
    static void SwitchMany<T>(this IEnumerable<Failable<T>> failables, Action<T> oks, Action<Exception> failures)
    {
        foreach (var failable in failables)
            failable.Switch(oks, failures);
    }

    public static (IEnumerable<T> Oks, IEnumerable<Exception> Failures) Split<T>(this IEnumerable<Failable<T>> failables)
    {
        List<T> oks = new();
        List<Exception> failures = new();
        failables.SwitchMany(oks.Add, failures.Add);
        return (oks, failures);
    }

    public static (IAsyncEnumerable<T> Oks, IAsyncEnumerable<Exception> Failures) SplitAE<T>(this IAsyncEnumerable<Failable<T>> asyncFailables, CancellationToken cancellationToken = default)
    {
        var oks = asyncFailables.Where(f => f.IsOk, cancellationToken).Select(f => f.Ok, cancellationToken);
        var failures = asyncFailables.Where(f => f.IsFailure, cancellationToken).Select(f => f.FailureOrDefault()!, cancellationToken);
        return (oks, failures);
    }

    public static (IAsyncEnumerable<T> Oks, IAsyncEnumerable<Exception> Failures) SplitAE<T>(this IEnumerable<Task<Failable<T>>> failables, CancellationToken cancellationToken = default)
    {
        return failables.ToAsyncEnumerable().SplitAE(cancellationToken);
    }

    public static (IAsyncEnumerable<T> Oks, IAsyncEnumerable<Exception> Failures) SplitAE<T>(this IEnumerable<ValueTask<Failable<T>>> failables, CancellationToken cancellationToken = default)
    {
        return failables.ToAsyncEnumerable().SplitAE(cancellationToken);
    }

    public static Failable<IEnumerable<T>> Compress<T>(this IEnumerable<Failable<T>> failables)
    {
        Queue<T> oks = new();
        foreach(var failable in failables)
        {
            if (failable.TryGetFailure(out var failure)) return Failable<IEnumerable<T>>.Catch(failure!);
            oks.Enqueue(failable);
        }
        return oks;
    }



    public static async ValueTask<Failable<IEnumerable<T>>> CompressAsync<T>(this IAsyncEnumerable<Failable<T>> asyncFailables, CancellationToken cancellationToken = default)
    {
        Queue<T> oks = new();
        await foreach (var failable in asyncFailables.ConfigureAwait(false).WithCancellation(cancellationToken))
        {
            if (failable.TryGetFailure(out var failure)) return Failable<IEnumerable<T>>.Catch(failure!);
            oks.Enqueue(failable);
        }
        return oks;
    }


    public static async Task<Failable<IEnumerable<T>>> CompressAsync<T>(this IEnumerable<Task<Failable<T>>> failables, CancellationToken cancellationToken = default)
    {
        return await failables.ToAsyncEnumerable().CompressAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Failable<IEnumerable<T>>> CompressAsync<T>(this IEnumerable<ValueTask<Failable<T>>> failables, CancellationToken cancellationToken = default)
    {
        return await failables.ToAsyncEnumerable().CompressAsync(cancellationToken).ConfigureAwait(false);
    }
}

public static class IFailablesExtensions
{
    static void SwitchMany<T>(this IEnumerable<IFailable<T>> failables, Action<T> oks, Action<Exception> failures)
    {
        foreach (var failable in failables)
            failable.Switch(oks, failures);
    }

    public static (IEnumerable<T> Oks, IEnumerable<Exception> Failures) Split<T>(this IEnumerable<IFailable<T>> failables)
    {
        List<T> oks = new();
        List<Exception> failures = new();
        failables.SwitchMany(oks.Add, failures.Add);
        return (oks, failures);
    }

    public static (IAsyncEnumerable<T> Oks, IAsyncEnumerable<Exception> Failures) SplitAE<T>(this IAsyncEnumerable<IFailable<T>> asyncFailables, CancellationToken cancellationToken = default)
    {
        var oks = asyncFailables.Where(f => f.IsOk, cancellationToken).Select(f => f.Ok, cancellationToken);
        var failures = asyncFailables.Where(f => f.IsFailure, cancellationToken).Select(f => f.FailureOrDefault()!, cancellationToken);
        return (oks, failures);
    }

    public static (IAsyncEnumerable<T> Oks, IAsyncEnumerable<Exception> Failures) SplitAE<T>(this IEnumerable<Task<IFailable<T>>> failables, CancellationToken cancellationToken = default)
    {
        return failables.ToAsyncEnumerable().SplitAE(cancellationToken);
    }

    public static (IAsyncEnumerable<T> Oks, IAsyncEnumerable<Exception> Failures) SplitAE<T>(this IEnumerable<ValueTask<IFailable<T>>> failables, CancellationToken cancellationToken = default)
    {
        return failables.ToAsyncEnumerable().SplitAE(cancellationToken);
    }

    public static Failable<IEnumerable<T>> Compress<T>(this IEnumerable<IFailable<T>> failables)
    {
        Queue<T> oks = new();
        foreach (var failable in failables)
        {
            if (failable.TryGetFailure(out var failure)) return Failable<IEnumerable<T>>.Catch(failure!);
            oks.Enqueue(failable.Ok);
        }
        return oks;
    }



    public static async ValueTask<Failable<IEnumerable<T>>> CompressAsync<T>(this IAsyncEnumerable<IFailable<T>> asyncFailables, CancellationToken cancellationToken = default)
    {
        Queue<T> oks = new();
        await foreach (var failable in asyncFailables.ConfigureAwait(false).WithCancellation(cancellationToken))
        {
            if (failable.TryGetFailure(out var failure)) return Failable<IEnumerable<T>>.Catch(failure!);
            oks.Enqueue(failable.Ok);
        }
        return oks;
    }


    public static async Task<Failable<IEnumerable<T>>> CompressAsync<T>(this IEnumerable<Task<IFailable<T>>> failables, CancellationToken cancellationToken = default)
    {
        return await failables.ToAsyncEnumerable().CompressAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Failable<IEnumerable<T>>> CompressAsync<T>(this IEnumerable<ValueTask<IFailable<T>>> failables, CancellationToken cancellationToken = default)
    {
        return await failables.ToAsyncEnumerable().CompressAsync(cancellationToken).ConfigureAwait(false);
    }
}



//TODO list:

//WebCallHandlers
//(IAsyncEnumerable, IAsyncEnumerable)
//Future<T>
