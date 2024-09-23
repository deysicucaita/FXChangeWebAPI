namespace FXChangeWebAPI.Domain.Common;

public sealed class FResult<TValue>
{
    public readonly TValue? Value;
    public readonly Error? Error;

    private bool _isSuccess;

    public bool IsSuccess { get { return _isSuccess; } }

    public FResult(TValue value)
    {
        _isSuccess = true;
        Value = value;
        Error = default;
    }

    public FResult(Error error)
    {
        _isSuccess = false;
        Value = default;
        Error = error;
    }

    // Happy path
    public static implicit operator FResult<TValue>(TValue value) => new FResult<TValue>(value);

    // Error path
    public static implicit operator FResult<TValue>(Error error) => new FResult<TValue>(error);

    public static FResult<TValue> Success(TValue value) => new FResult<TValue>(value);

    public static FResult<TValue> Failure(Error error) => new FResult<TValue>(error);
}

public sealed record Error(string Code, string? Message = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error ValidationError = new("01", "Validation Error");
    public static readonly Error NotFoundEntity = new("02", "Not Found Error");
    public static readonly Error UserOrPwdIncorrect = new("03", "User or Pwd incorrect.");
    public static readonly Error UserNotFound = new("04", "User Not Found.");
    public static readonly Error InvalidPwd = new("05", "Password invalid.");
    public static readonly Error DateNotFound = new("06", "Operated Date Not Found.");
    public static readonly Error PrevCashSheetNotFound = new("07", "Previous CashSheet Not Found.");
    public static readonly Error CounterAlreadyClosed = new("08", "CashSheet already executed at counter.");
    public static readonly Error CashSheetWithNegValue = new("09", "CashSheet with Negative Value.");
}
