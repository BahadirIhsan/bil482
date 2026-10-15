namespace IndustryApp.Application.Common;

/// <summary>Değer taşımayan işlem sonucu. Hata durumunda Error taşır.</summary>
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess == (error is not null))
            throw new ArgumentException("Başarılı sonuç hata taşıyamaz; başarısız sonuç hata taşımalıdır.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>Değer taşıyan işlem sonucu.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(true, null) => _value = value;

    private Result(Error error)
        : base(false, error)
    {
    }

    /// <summary>Başarılı sonucun değeri; başarısız sonuçta erişilirse hata fırlatır.</summary>
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Başarısız sonucun değeri okunamaz.");

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);
}
