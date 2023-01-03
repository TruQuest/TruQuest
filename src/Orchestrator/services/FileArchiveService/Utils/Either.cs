namespace Utils;

internal struct Either<TError, TData>
    where TError : class
    where TData : class
{
    public TError? Error { get; }
    public TData? Data { get; }
    public bool IsError => Error != null;

    private Either(TError? error = null, TData? data = null)
    {
        Error = error;
        Data = data;
    }

    public static implicit operator Either<TError, TData>(TError error)
        => new Either<TError, TData>(error: error);

    public static implicit operator Either<TError, TData>(TData data)
        => new Either<TError, TData>(data: data);
}