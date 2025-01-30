namespace Shared.Models.Common
{
    public enum DataResultType
    {
        Success,
        NotFound,
        AlreadyExists,
        Error
    }
    public class DataResult<T>
    {
        public T? Data { get; private set; }
        public DataResultType ResultType { get; private set; }

        private DataResult(DataResultType resultType, T? data)
        {
            ResultType = resultType;
            Data = data;
        }

        public static DataResult<T> Success(T data) => new DataResult<T>(DataResultType.Success, data);
        public static DataResult<T> NotFound() => new DataResult<T>(DataResultType.NotFound, default);
        public static DataResult<T> AlreadyExists() => new DataResult<T>(DataResultType.AlreadyExists, default);
        public static DataResult<T> Error() => new DataResult<T>(DataResultType.Error, default);

        public bool IsSuccess => ResultType == DataResultType.Success;
        public bool IsFailure => ResultType != DataResultType.Success;
    }
}