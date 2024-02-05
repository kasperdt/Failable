using System.Security.Cryptography.X509Certificates;

namespace Failable
{

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

    public class Failure : FailableException, IEquatable<Failure>
    {
        public Failure(string message) : base(message)
        {

        }



        public void Deconstruct(out string message) => message = Message;

        public bool Equals(Failure? other) => other is { } value && Message == value.Message;
        public static bool operator ==(Failure a, Failure b) => a.Equals(b);
        public static bool operator !=(Failure a, Failure b) => !(a == b);

        public override bool Equals(object? obj) => obj is Failure f && f == this;

        public override int GetHashCode() => Message.GetHashCode();

        public override string ToString() => Message;

        public static implicit operator Failure(string message) => new(message);
        public static implicit operator string(Failure failure) => failure.Message;

        public static Failure WithMessage(string message) => new(message);

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

        public void IfOkDo(Action<T> ok)
        {
            if (IsOk) ok(Ok);
        }

        public void IfFailureDo(Action<Exception> failure)
        {
            if (IsFailure) failure(Failure);
        }

        public Failable<TNew> IfOk<TNew>(Func<T, TNew> ok)
            => IsOk ? ok(Ok) : Failure;

        public Failable<TNew> IfOk<TNew>(Func<T, Failable<TNew>> ok) //?
            => IsOk ? ok(Ok) : Failure;

        public T IfFailure(Func<Exception, T> failure)
            => IsOk ? Ok : failure(Failure);

        public Failable<T> IfFailure(Func<Exception, Failable<T>> failure) //?
            => IsOk ? Ok : failure(Failure);

        public T? IfFailureNullable(Func<Exception, T?> failure) //?
            => IsOk ? Ok : failure(Failure);


        //public void Deconstruct(out T? ok)
        //{
        //    ok = MaybeOk;
        //}
        //public void Deconstruct(out Exception? failure)
        //{
        //    failure = MaybeFailure;
        //}
    }

    public static class Failable
    {
        //public static Maybe<T> Error<T>(string message) => Maybe<T>.Error(message);



        //static Maybe<int> a() => Error("");

        public static Failure Failure(string message) => message;

        //public static Failable<T> Assert(o)

        public static Failable<T> Try<T>(Func<T> toTry)
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

}
