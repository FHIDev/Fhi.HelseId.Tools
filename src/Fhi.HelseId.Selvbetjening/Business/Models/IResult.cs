namespace Fhi.HelseIdSelvbetjening.Business.Models
{
    public interface IResult<T, TError>
    {
        TResult HandleResponse<TResult>(
            Func<T, TResult> onSuccess,
            Func<TError, TResult> onError);
    }

    public record Success<T, TError>(T Value) : IResult<T, TError>
    {
        public TResult HandleResponse<TResult>(
            Func<T, TResult> onSuccess,
            Func<TError, TResult> onError)
            => onSuccess(Value);
    }
    public record Error<T, TError>(TError Value) : IResult<T, TError>
    {
        public TResult HandleResponse<TResult>(
            Func<T, TResult> onSuccess,
            Func<TError, TResult> onError)
            => onError(Value);
    }
}
