using System;

namespace NDesk.Options.Extensions
{
    /// <summary>
    /// OptionItemBase class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class OptionItemBase<T>
    {
        /// <summary>
        /// Casts the Value string to the Type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>Note that this is a hard cast.</remarks>
        protected internal static T CastString(string value)
        {
            //TODO: Might provide a hook for more explicit-user-provided string conversions.
            return (T) Convert.ChangeType(value, typeof (T));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prototype"></param>
        protected OptionItemBase(string prototype)
        {
            Prototype = prototype;
        }

        /// <summary>
        /// Gets the Prototype.
        /// </summary>
        protected string Prototype { get; private set; }

        /// <summary>
        /// Throws an OptionException with the Message and Prototype.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected OptionException ThrowOptionException(string message)
        {
            return new OptionException(message, Prototype);
        }
    }
}
