using System.Collections;
using System.Collections.Generic;

namespace NDesk.Options.Extensions
{
    /// <summary>
    /// VariableList OptionItemBase class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VariableList<T> : OptionItemBase<T>, IEnumerable<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prototype"></param>
        public VariableList(string prototype)
            : base(prototype)
        {
        }

        //TODO: Might could transform this into an observable collection.
        /// <summary>
        /// Values backing field.
        /// </summary>
        private readonly List<T> _values = new List<T>();

        /// <summary>
        /// Gets the ValuesList for internal consumption.
        /// </summary>
        internal List<T> ValuesList
        {
            get { return _values; }
        }

        /// <summary>
        /// Gets the Values.
        /// </summary>
        public IEnumerable<T> Values
        {
            get { return _values; }
        }

        #region Enumerable Members

        public IEnumerator<T> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
