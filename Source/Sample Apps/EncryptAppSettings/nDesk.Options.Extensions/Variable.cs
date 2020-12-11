namespace NDesk.Options.Extensions
{
    /* TODO: Consider what to do with Variable, add RequiredVariable,
     * or does that mix with the Requirement class? */
    /// <summary>
    /// Variable OptionItemBase class.
    /// The last known Variable Value is recalled.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Variable<T> : OptionItemBase<T>
    {
        /// <summary>
        /// Implicitly converts the Variable value.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static implicit operator T(Variable<T> variable)
        {
            return variable.Value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prototype">The name of the Variable.</param>
        public Variable(string prototype)
            : base(prototype)
        {
            Value = default(T);
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        public T Value { get; internal set; }
    }
}
