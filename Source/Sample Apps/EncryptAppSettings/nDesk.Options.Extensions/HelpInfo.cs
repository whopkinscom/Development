namespace NDesk.Options.Extensions
{
    /// <summary>
    /// HelpInfo is a decorator that handles a Boolean-Variable.
    /// </summary>
    public class HelpInfo
    {
        /// <summary>
        /// Help backing field.
        /// </summary>
        private readonly Switch _help;

        /// <summary>
        /// Gets the Help.
        /// </summary>
        public Switch Help
        {
            get { return _help; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="optionSet">An OptionSet.</param>
        /// <param name="prototype">The HelpPrototype.</param>
        /// <param name="description"></param>
        internal HelpInfo(OptionSet optionSet, string prototype, string description = null)
        {
            var variablePrototype = prototype;
            _help = optionSet.AddSwitch(variablePrototype, description);
        }
    }
}