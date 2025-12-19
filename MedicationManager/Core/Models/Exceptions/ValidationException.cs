namespace MedicationManager.Core.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// List of validation errors
        /// </summary>
        public IEnumerable<string> Errors { get; }

        public ValidationException() : base("One or more validation errors occurred")
        {
            Errors = [];
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public ValidationException(IEnumerable<string> errors)
            : base("One or more validation errors occurred")
        {
            Errors = errors;
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Errors = [message];
        }

        /// <summary>
        /// Gets all error messages as a single formatted string
        /// </summary>
        public string GetAllErrors() => string.Join(Environment.NewLine, Errors);
    }
}