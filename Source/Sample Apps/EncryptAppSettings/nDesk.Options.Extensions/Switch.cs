namespace NDesk.Options.Extensions
{
    /// <summary>
    /// Represents a Command-Line Switch.
    /// </summary>
    public class Switch
    {
        /// <summary>
        /// Gets whether the Switch is Enabled.
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Implicitly converts the Switch to its bool value.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static implicit operator bool(Switch input)
        {
            return input.Enabled;
        }
    }
}
