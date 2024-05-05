namespace FailableLib;

public static class AsyncFailableExtensions
{
    public static async ValueTask<Exception?> FailureOrDefaultAsync<T>(this ValueTask<Failable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).FailureOrDefault();

    public static async Task<Exception?> FailureOrDefaultAsync<T>(this Task<Failable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).FailureOrDefault();


    public static async ValueTask<T> IfFailureAsync<T>(this ValueTask<Failable<T>> failableValueTask, Func<Exception, T> failure)
        => (await failableValueTask.ConfigureAwait(false)).IfFailure(failure);

    public static async Task<T> IfFailureAsync<T>(this Task<Failable<T>> failableTask, Func<Exception, T> failure)
        => (await failableTask.ConfigureAwait(false)).IfFailure(failure);

    public static async ValueTask<T> IfFailureAwaitAsync<T>(this ValueTask<Failable<T>> failableValueTask, Func<Exception, ValueTask<T>> asyncFailure)
        => await (await failableValueTask.ConfigureAwait(false)).IfFailureAsync(asyncFailure).ConfigureAwait(false);

    public static async Task<T> IfFailureAwaitTaskAsync<T>(this ValueTask<Failable<T>> failableValueTask, Func<Exception, Task<T>> asyncFailure)
        => await (await failableValueTask.ConfigureAwait(false)).IfFailureTaskAsync(asyncFailure).ConfigureAwait(false);

    public static async Task<T> IfFailureAwaitAsync<T>(this Task<Failable<T>> failableTask, Func<Exception, ValueTask<T>> asyncFailure)
    => await (await failableTask.ConfigureAwait(false)).IfFailureAsync(asyncFailure).ConfigureAwait(false);

    public static async Task<T> IfFailureAwaitTaskAsync<T>(this Task<Failable<T>> failableTask, Func<Exception, Task<T>> asyncFailure)
        => await (await failableTask.ConfigureAwait(false)).IfFailureTaskAsync(asyncFailure).ConfigureAwait(false);


    public static async ValueTask<Failable<TNew>> IfOkAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, Failable<TNew>> ok)
        => (await failableValueTask.ConfigureAwait(false)).IfOk(ok);

    public static async Task<Failable<TNew>> IfOkAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, Failable<TNew>> ok)
        => (await failableTask.ConfigureAwait(false)).IfOk(ok);

    public static async ValueTask<Failable<TNew>> IfOkAwaitAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, ValueTask<Failable<TNew>>> asyncOk)
        => await (await failableValueTask.ConfigureAwait(false)).IfOkAsync(asyncOk);

    public static async Task<Failable<TNew>> IfOkAwaitAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, ValueTask<Failable<TNew>>> asyncOk)
        => await (await failableTask.ConfigureAwait(false)).IfOkAsync(asyncOk);

    public static async Task<Failable<TNew>> IfOkAwaitTaskAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, Task<Failable<TNew>>> asyncOk)
    => await (await failableValueTask.ConfigureAwait(false)).IfOkTaskAsync(asyncOk);

    public static async Task<Failable<TNew>> IfOkAwaitTaskAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, Task<Failable<TNew>>> asyncOk)
        => await (await failableTask.ConfigureAwait(false)).IfOkTaskAsync(asyncOk);

    public static async ValueTask<Failable<TNew>> IfOkSimpleAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, TNew> okSimple)
        => (await failableValueTask.ConfigureAwait(false)).IfOkSimple(okSimple);

    public static async Task<Failable<TNew>> IfOkSimpleAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, TNew> okSimple)
        => (await failableTask.ConfigureAwait(false)).IfOkSimple(okSimple);

    public static async ValueTask<Failable<TNew>> IfOkSimpleAwaitAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, ValueTask<TNew>> asyncOkSimple)
        => await (await failableValueTask.ConfigureAwait(false)).IfOkSimpleAsync(asyncOkSimple);

    public static async Task<Failable<TNew>> IfOkSimpleAwaitAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, ValueTask<TNew>> asyncOkSimple)
        => await (await failableTask.ConfigureAwait(false)).IfOkSimpleAsync(asyncOkSimple);

    public static async Task<Failable<TNew>> IfOkSimpleAwaitTaskAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, Task<TNew>> asyncOkSimple)
        => await (await failableValueTask.ConfigureAwait(false)).IfOkSimpleTaskAsync(asyncOkSimple);

    public static async Task<Failable<TNew>> IfOkSimpleAwaitTaskAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, Task<TNew>> asyncOkSimple)
        => await (await failableTask.ConfigureAwait(false)).IfOkSimpleTaskAsync(asyncOkSimple);

    public static async ValueTask<Failable<TNew>> IfOkTryAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, TNew> ok)
        => (await failableValueTask.ConfigureAwait(false)).IfOkTry(ok);

    public static async Task<Failable<TNew>> IfOkTryAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, TNew> ok)
        => (await failableTask.ConfigureAwait(false)).IfOkTry(ok);

    public static async ValueTask<Failable<TNew>> IfOkTryAwaitAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, ValueTask<TNew>> asyncOk)
        => await (await failableValueTask.ConfigureAwait(false)).IfOkTryAsync(asyncOk);

    public static async Task<Failable<TNew>> IfOkTryAwaitAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, ValueTask<TNew>> asyncOk)
        => await (await failableTask.ConfigureAwait(false)).IfOkTryAsync(asyncOk);

    public static async Task<Failable<TNew>> IfOkTryAwaitTaskAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, Task<TNew>> asyncOk)
        => await (await failableValueTask.ConfigureAwait(false)).IfOkTryTaskAsync(asyncOk);

    public static async Task<Failable<TNew>> IfOkTryAwaitTaskAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, Task<TNew>> asyncOk)
        => await (await failableTask.ConfigureAwait(false)).IfOkTryTaskAsync(asyncOk);


    public static async ValueTask<bool> IsFailureAsync<T>(this ValueTask<Failable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).IsFailure;

    public static async Task<bool> IsFailureAsync<T>(this Task<Failable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).IsFailure;

    public static async ValueTask<bool> IsOkAsync<T>(this ValueTask<Failable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).IsOk;

    public static async Task<bool> IsOkAsync<T>(this Task<Failable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).IsOk;


    public static async ValueTask<TNew> MatchAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, TNew> ok, Func<Exception, TNew> failure)
        => (await failableValueTask.ConfigureAwait(false)).Match(ok, failure);

    public static async Task<TNew> MatchAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, TNew> ok, Func<Exception, TNew> failure)
        => (await failableTask.ConfigureAwait(false)).Match(ok, failure);

    public static async ValueTask<TNew> MatchAwaitAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, ValueTask<TNew>> asyncOk, Func<Exception, ValueTask<TNew>> asyncFailure)
        => await (await failableValueTask.ConfigureAwait(false)).Match(asyncOk, asyncFailure).ConfigureAwait(false);

    public static async Task<TNew> MatchAwaitAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, ValueTask<TNew>> asyncOk, Func<Exception, ValueTask<TNew>> asyncFailure)
        => await (await failableTask.ConfigureAwait(false)).Match(asyncOk, asyncFailure).ConfigureAwait(false);

    public static async Task<TNew> MatchAwaitTaskAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, Task<TNew>> asyncOk, Func<Exception, Task<TNew>> asyncFailure)
        => await (await failableValueTask.ConfigureAwait(false)).Match(asyncOk, asyncFailure).ConfigureAwait(false);

    public static async Task<TNew> MatchAwaitTaskAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, Task<TNew>> asyncOk, Func<Exception, Task<TNew>> asyncFailure)
        => await (await failableTask.ConfigureAwait(false)).Match(asyncOk, asyncFailure).ConfigureAwait(false);


    public static async ValueTask<T> OkAsync<T>(this ValueTask<Failable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).Ok;

    public static async Task<T> OkAsync<T>(this Task<Failable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).Ok;

    public static async ValueTask<T?> OkOrDefaultAsync<T>(this ValueTask<Failable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).OkOrDefault();

    public static async Task<T?> OkOrDefaultAsync<T>(this Task<Failable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).OkOrDefault();


    public static async ValueTask SwitchAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Action<T> ok, Action<Exception> failure)
        => (await failableValueTask.ConfigureAwait(false)).Switch(ok, failure);

    public static async Task SwitchAsync<T, TNew>(this Task<Failable<T>> failableTask, Action<T> ok, Action<Exception> failure)
        => (await failableTask.ConfigureAwait(false)).Switch(ok, failure);

    public static async ValueTask SwitchAwaitAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, ValueTask> ok, Func<Exception, ValueTask> failure)
        => await (await failableValueTask.ConfigureAwait(false)).Match(ok, failure).ConfigureAwait(false);

    public static async Task SwitchAwaitAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, ValueTask> ok, Func<Exception, ValueTask> failure)
        => await (await failableTask.ConfigureAwait(false)).Match(ok, failure).ConfigureAwait(false);

    public static async Task SwitchAwaitTaskAsync<T, TNew>(this ValueTask<Failable<T>> failableValueTask, Func<T, Task> ok, Func<Exception, Task> failure)
        => await (await failableValueTask.ConfigureAwait(false)).Match(ok, failure).ConfigureAwait(false);

    public static async Task SwitchAwaitTaskAsync<T, TNew>(this Task<Failable<T>> failableTask, Func<T, Task> ok, Func<Exception, Task> failure)
        => await (await failableTask.ConfigureAwait(false)).Match(ok, failure).ConfigureAwait(false);


    public static async ValueTask ThrowIfFailureAsync<T>(this ValueTask<Failable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).ThrowIfFailure();

    public static async Task ThrowIfFailureAsync<T>(this Task<Failable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).ThrowIfFailure();

    public static T Unwrap<T>(this ValueTask<Failable<T>> failableValueTask)
        => failableValueTask.Result.Ok;

    public static T Unwrap<T>(this Task<Failable<T>> failableValueTask)
        => failableValueTask.Result.Ok;
}


public static class AsyncIFailableExtensions
{
    public static async ValueTask<Exception?> FailureOrDefaultAsync<T>(this ValueTask<IFailable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).FailureOrDefault();

    public static async Task<Exception?> FailureOrDefaultAsync<T>(this Task<IFailable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).FailureOrDefault();


    public static async ValueTask<T> IfFailureAsync<T>(this ValueTask<IFailable<T>> failableValueTask, Func<Exception, T> failure)
        => (await failableValueTask.ConfigureAwait(false)).IfFailure(failure);

    public static async Task<T> IfFailureAsync<T>(this Task<IFailable<T>> failableTask, Func<Exception, T> failure)
        => (await failableTask.ConfigureAwait(false)).IfFailure(failure);

    public static async ValueTask<T> IfFailureAwaitAsync<T>(this ValueTask<IFailable<T>> failableValueTask, Func<Exception, ValueTask<T>> asyncFailure)
        => await (await failableValueTask.ConfigureAwait(false)).IfFailureAsync(asyncFailure).ConfigureAwait(false);

    public static async Task<T> IfFailureAwaitTaskAsync<T>(this ValueTask<IFailable<T>> failableValueTask, Func<Exception, Task<T>> asyncFailure)
        => await (await failableValueTask.ConfigureAwait(false)).IfFailureTaskAsync(asyncFailure).ConfigureAwait(false);

    public static async Task<T> IfFailureAwaitAsync<T>(this Task<IFailable<T>> failableTask, Func<Exception, ValueTask<T>> asyncFailure)
    => await (await failableTask.ConfigureAwait(false)).IfFailureAsync(asyncFailure).ConfigureAwait(false);

    public static async Task<T> IfFailureAwaitTaskAsync<T>(this Task<IFailable<T>> failableTask, Func<Exception, Task<T>> asyncFailure)
        => await (await failableTask.ConfigureAwait(false)).IfFailureTaskAsync(asyncFailure).ConfigureAwait(false);


    public static async ValueTask<bool> IsFailureAsync<T>(this ValueTask<IFailable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).IsFailure;

    public static async Task<bool> IsFailureAsync<T>(this Task<IFailable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).IsFailure;

    public static async ValueTask<bool> IsOkAsync<T>(this ValueTask<IFailable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).IsOk;

    public static async Task<bool> IsOkAsync<T>(this Task<IFailable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).IsOk;


    public static async ValueTask<TNew> MatchAsync<T, TNew>(this ValueTask<IFailable<T>> failableValueTask, Func<T, TNew> ok, Func<Exception, TNew> failure)
        => (await failableValueTask.ConfigureAwait(false)).Match(ok, failure);

    public static async Task<TNew> MatchAsync<T, TNew>(this Task<IFailable<T>> failableTask, Func<T, TNew> ok, Func<Exception, TNew> failure)
        => (await failableTask.ConfigureAwait(false)).Match(ok, failure);

    public static async ValueTask<TNew> MatchAwaitAsync<T, TNew>(this ValueTask<IFailable<T>> failableValueTask, Func<T, ValueTask<TNew>> asyncOk, Func<Exception, ValueTask<TNew>> asyncFailure)
        => await (await failableValueTask.ConfigureAwait(false)).Match(asyncOk, asyncFailure).ConfigureAwait(false);

    public static async Task<TNew> MatchAwaitAsync<T, TNew>(this Task<IFailable<T>> failableTask, Func<T, ValueTask<TNew>> asyncOk, Func<Exception, ValueTask<TNew>> asyncFailure)
        => await (await failableTask.ConfigureAwait(false)).Match(asyncOk, asyncFailure).ConfigureAwait(false);

    public static async Task<TNew> MatchAwaitTaskAsync<T, TNew>(this ValueTask<IFailable<T>> failableValueTask, Func<T, Task<TNew>> asyncOk, Func<Exception, Task<TNew>> asyncFailure)
        => await (await failableValueTask.ConfigureAwait(false)).Match(asyncOk, asyncFailure).ConfigureAwait(false);

    public static async Task<TNew> MatchAwaitTaskAsync<T, TNew>(this Task<IFailable<T>> failableTask, Func<T, Task<TNew>> asyncOk, Func<Exception, Task<TNew>> asyncFailure)
        => await (await failableTask.ConfigureAwait(false)).Match(asyncOk, asyncFailure).ConfigureAwait(false);


    public static async ValueTask<T> OkAsync<T>(this ValueTask<IFailable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).Ok;

    public static async Task<T> OkAsync<T>(this Task<IFailable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).Ok;

    public static async ValueTask<T?> OkOrDefaultAsync<T>(this ValueTask<IFailable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).OkOrDefault();

    public static async Task<T?> OkOrDefaultAsync<T>(this Task<IFailable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).OkOrDefault();


    public static async ValueTask SwitchAsync<T, TNew>(this ValueTask<IFailable<T>> failableValueTask, Action<T> ok, Action<Exception> failure)
        => (await failableValueTask.ConfigureAwait(false)).Switch(ok, failure);

    public static async Task SwitchAsync<T, TNew>(this Task<IFailable<T>> failableTask, Action<T> ok, Action<Exception> failure)
        => (await failableTask.ConfigureAwait(false)).Switch(ok, failure);

    public static async ValueTask SwitchAwaitAsync<T, TNew>(this ValueTask<IFailable<T>> failableValueTask, Func<T, ValueTask> ok, Func<Exception, ValueTask> failure)
        => await (await failableValueTask.ConfigureAwait(false)).Match(ok, failure).ConfigureAwait(false);

    public static async Task SwitchAwaitAsync<T, TNew>(this Task<IFailable<T>> failableTask, Func<T, ValueTask> ok, Func<Exception, ValueTask> failure)
        => await (await failableTask.ConfigureAwait(false)).Match(ok, failure).ConfigureAwait(false);

    public static async Task SwitchAwaitTaskAsync<T, TNew>(this ValueTask<IFailable<T>> failableValueTask, Func<T, Task> ok, Func<Exception, Task> failure)
        => await (await failableValueTask.ConfigureAwait(false)).Match(ok, failure).ConfigureAwait(false);

    public static async Task SwitchAwaitTaskAsync<T, TNew>(this Task<IFailable<T>> failableTask, Func<T, Task> ok, Func<Exception, Task> failure)
        => await (await failableTask.ConfigureAwait(false)).Match(ok, failure).ConfigureAwait(false);


    public static async ValueTask ThrowIfFailureAsync<T>(this ValueTask<IFailable<T>> failableValueTask)
        => (await failableValueTask.ConfigureAwait(false)).ThrowIfFailure();

    public static async Task ThrowIfFailureAsync<T>(this Task<IFailable<T>> failableTask)
        => (await failableTask.ConfigureAwait(false)).ThrowIfFailure();

    public static T Unwrap<T>(this ValueTask<IFailable<T>> failableValueTask)
        => failableValueTask.Result.Ok;

    public static T Unwrap<T>(this Task<IFailable<T>> failableValueTask)
        => failableValueTask.Result.Ok;
}


//TODO list:

//WebCallHandlers
//(IAsyncEnumerable, IAsyncEnumerable)
//Future<T>
