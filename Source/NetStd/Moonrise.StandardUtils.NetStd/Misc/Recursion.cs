using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Moonrise.Utils.Standard.Misc
{
    /// <summary>
    /// Allows for guarded recursion
    /// </summary>
    public class Recursion : IDisposable
    {
        /// <summary>
        /// A one-per-thread recursion count
        /// </summary>
        static private ThreadLocal<int> currentDepth = new ThreadLocal<int>();

        /// <summary>
        /// Creating a <see cref="Recursion"/> increases the recursion count by one
        /// </summary>
        public Recursion()
        {
            currentDepth.Value++;
        }

        /// <summary>
        /// Disposing a <see cref="Recursion"/> decreases the recursion count by one
        /// </summary>
        /// <remarks>
        /// As long as the using pattern is deployed the count will always get reset to zero, either by an exception or returns falling down the stack.
        /// </remarks>
        public void Dispose()
        {
            currentDepth.Value--;
        }

        /// <summary>
        /// Guards against a recursive call eating up the stack.
        /// </summary>
        /// <remarks>
        /// Use the using pattern just prior to making a recursive call. Don't even use the brackets.
        /// <example>
        /// <code>
        /// void Doit() {<para>
        /// ...</para><para>
        /// using (Recursion.Guard())</para><para>
        /// Doit();</para><para>
        /// ...</para><para>
        /// }</para>
        /// </code></example>
        /// </remarks>
        /// <param name="maximumLevel">The maximum depth you do not expect to exceed - defaults to 50. It's pretty rare to go much deeper than this but you know your algorithm!</param>
        /// <param name="caller">The name of the method making the guarded call.</param>
        /// <returns>An <see cref="IDisposable"/> <see cref="Recursion"/> reference that will manage the recursion counter</returns>
        public static IDisposable Guard(int maximumLevel = 50, [CallerMemberName] string caller = null)
        {
            if (currentDepth.Value >= maximumLevel)
                throw new StackOverflowException($"The {caller} method has been called {maximumLevel} times!");

            IDisposable retVal = new Recursion();
            return retVal;
        }
    }
}