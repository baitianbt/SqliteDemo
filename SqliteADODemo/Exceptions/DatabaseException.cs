using System.Runtime.Serialization;

namespace SqliteADODemo.Exceptions
{
    [Serializable]
    public class DatabaseException : Exception
    {
        public string Operation { get; }
        public string Details { get; }
        public string SqlStatement { get; }

        public DatabaseException(string message) 
            : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public DatabaseException(string operation, string message, Exception innerException = null) 
            : base(message, innerException)
        {
            Operation = operation;
            Details = innerException?.Message;
        }

        public DatabaseException(string operation, string message, string sqlStatement, Exception innerException = null) 
            : base(message, innerException)
        {
            Operation = operation;
            Details = innerException?.Message;
            SqlStatement = sqlStatement;
        }

        protected DatabaseException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            Operation = info.GetString(nameof(Operation));
            Details = info.GetString(nameof(Details));
            SqlStatement = info.GetString(nameof(SqlStatement));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Operation), Operation);
            info.AddValue(nameof(Details), Details);
            info.AddValue(nameof(SqlStatement), SqlStatement);
        }

        public override string ToString()
        {
            var result = $"{GetType().Name}: {Message}";
            if (!string.IsNullOrEmpty(Operation))
                result += $"\nOperation: {Operation}";
            if (!string.IsNullOrEmpty(Details))
                result += $"\nDetails: {Details}";
            if (!string.IsNullOrEmpty(SqlStatement))
                result += $"\nSQL: {SqlStatement}";
            if (InnerException != null)
                result += $"\n\nInner Exception:\n{InnerException}";
            return result;
        }
    }
} 