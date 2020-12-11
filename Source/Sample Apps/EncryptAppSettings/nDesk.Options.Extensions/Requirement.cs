using System;

namespace NDesk.Options.Core
{
    /// <summary>
    /// Requirement class.
    /// </summary>
    public class Requirement
    {
        /// <summary>
        /// Gets or sets a function indicating whether IsRequirementSatisfied.
        /// </summary>
        public Func<bool> IsRequirementSatisfied { get; set; }

        /// <summary>
        /// Gets or sets the Text.
        /// </summary>
        public string[] Text { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Requirement()
        {
            //Such that we do not leave the function open.
            IsRequirementSatisfied = () => false;
        }
    }
}