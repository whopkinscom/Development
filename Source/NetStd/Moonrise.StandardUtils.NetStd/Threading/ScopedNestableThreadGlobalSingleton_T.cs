#region Apache-v2.0

//    Copyright 2017 Will Hopkins - Moonrise Media Ltd.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

using System;
using System.Reflection;
using System.Threading;

namespace Moonrise.Utils.Standard.Threading
{
    /// <summary>
    ///     Provides scoped, nestable, thread global values.
    ///     <para>
    ///         Scoped because any call to get the value (via a static) that occurs somewhere INSIDE the using scope will get
    ///         that value.
    ///     </para>
    ///     <para>
    ///         Nestable because if you open another scope (through an interior/nested using) then THAT becomes the value
    ///         anything inside of THAT scope will receive whereas once outside of THAT using scope the value for the PREVIOUS
    ///         scope is the static value.
    ///     </para>
    ///     <para>
    ///         Thread because a <see cref="ThreadLocal{T}" /> is used as the backing store and so each scopes within different
    ///         threads are just for that thread.
    ///     </para>
    ///     <para>
    ///         Global because it's sort of acting like a global variable!
    ///     </para>
    ///     <para>
    ///         Another way of thinking about this class is that it is a smuggler. It can smuggle values way down into call
    ///         heirarchies without you needing to retrofit paramters to pass to each call. You know the way you can use class
    ///         variables for temporary working purposes without them being true properties/attributes of that class (from the
    ///         design rather than language persepective here)? Well, a <see cref="ScopedNestableThreadGlobalSingleton{T}" />
    ///         is really the same thing, but for a thread. Kinda!
    ///     </para>
    ///     <remarks>
    ///         <para>
    ///             Usage:
    ///             <para>
    ///                 public class SUT : NestableThreadGlobalSingleton&lt;string&gt;{public SUT(string value) :
    ///                 base(value){}
    ///             </para>
    ///         </para>
    ///     </remarks>
    ///     <example>
    ///         Wrap any significant "outer code" with
    ///         <para>
    ///             using (new SUT("value")) { YOUR CODE }
    ///         </para>
    ///         Then anywhere, even deep, within YOUR CODE you can get the current nested, threaded global value via
    ///         SUT.CurrentValue()
    ///     </example>
    /// </summary>
    /// <typeparam name="T">The type of the singelton</typeparam>
    /// <seealso cref="System.IDisposable" />
    public abstract class ScopedNestableThreadGlobalSingleton<T> : IDisposable
    {
        /// <summary>
        ///     The current "thing". This is stored on a per thread basis.
        /// </summary>
        private static readonly ThreadLocal<ScopedNestableThreadGlobalSingleton<T>> ThreadedCurrentGlobal =
            new ThreadLocal<ScopedNestableThreadGlobalSingleton<T>>(() => null);

        /// <summary>
        ///     Gets the current Nestable Thread Global Singleton value. If not already set this will be the default for generic
        ///     type.
        /// </summary>
        public static T CurrentValue => Current != null ? Current.Value : default(T);

        /// <summary>
        ///     Gets the current <see cref="ScopedNestableThreadGlobalSingleton{T}" />
        /// </summary>
        protected static ScopedNestableThreadGlobalSingleton<T> Current
        {
            get => ThreadedCurrentGlobal.Value;

            set => ThreadedCurrentGlobal.Value = value;
        }

        /// <summary>
        ///     The previous NestableThreadGlobalSingleton. This allows us to nest scopes, should we so desire.
        /// </summary>
        protected ScopedNestableThreadGlobalSingleton<T> Previous { get; }

        /// <summary>
        ///     The nested global threaded value
        /// </summary>
        protected T Value { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScopedNestableThreadGlobalSingleton{T}" /> class.
        /// </summary>
        /// <param name="value">The value which will be the current NestedThreadGlobal value.</param>
        protected ScopedNestableThreadGlobalSingleton(T value)
        {
            Previous = Current;
            Current = this;
            Value = value;
        }

        /// <summary>
        ///     Prevents a default instance of the <see cref="ScopedNestableThreadGlobalSingleton{T}" /> class from being created.
        ///     However we do need to be create one to initially populate the <see cref="ThreadLocal{T}" />
        /// </summary>
        private ScopedNestableThreadGlobalSingleton() { }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Inform any child implementations that we are disposing
            Disposing();

            if (ThreadedCurrentGlobal.Value != this)
            {
                throw new AmbiguousMatchException(
                    "The NestableThreadGlobalSingleton<T> being disposed SHOULD be the current thread static one, but for some reason isn't!");
            }

            ThreadedCurrentGlobal.Value = Previous;
        }

        /// <summary>
        ///     Indicates that the current <see cref="ScopedNestableThreadGlobalSingleton{T}" /> is being disposed. Override this
        ///     to take
        ///     additional actions.
        /// </summary>
        protected virtual void Disposing() { }

        // <summary>
        //     Here to remind you you need to implement a public new static T Value() method to make it globally available.
        // </summary>
        // <returns></returns>
//        protected abstract T ImplementStaticCurrentValue();
    }
}