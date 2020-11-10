using System;
using System.Collections;
using Moonrise.Logging.Util;

namespace Moonrise.Logging
{
    /// <summary>
    ///     WORK IN PROGRESS!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ScopedNestableThreadGlobalSingleton{T}" />
    public class MethodTrace<T> : ScopedNestableThreadGlobalSingleton<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodTrace{T}"/> class.
        /// </summary>
        /// <param name="value">The value which will be the current NestedThreadGlobal value.</param>
        public MethodTrace(T value) : base(value)
        {
        }

        /// <summary>
        /// Determines if the type is currently traceable
        /// </summary>
        /// <param name="methodOwner">The method owner.</param>
        /// <returns>An indication if the owner of a called method has been marked as traceable</returns>
        public static bool Traceable(Type methodOwner)
        {
            bool retVal = false;

            if (CurrentValue.GetType() == typeof(string))
            {
                retVal = ((string)(object)CurrentValue == "*") || ((string)(object)CurrentValue == methodOwner.Name);
            }
            else if (CurrentValue.GetType() == methodOwner)
            {
                retVal = true;
            }
            else if (CurrentValue.GetType().IsAssignableFrom(typeof(IEnumerable)))
            {
                // Iterate through the "list" of types and check if the tracetype is in that list
            }

            return retVal;
        }
    }
}
