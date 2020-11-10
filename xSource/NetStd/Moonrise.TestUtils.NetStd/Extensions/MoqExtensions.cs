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
using System.Reflection;
using Moq.Language;
using Moq.Language.Flow;

namespace Moonrise.Utils.Test.Extensions
{
    /// <summary>
    ///     Extensions that can be used with Moq library. Taken from
    ///     http://stackoverflow.com/questions/1068095/assigning-out-ref-parameters-in-moq but further overloaded for up to 3
    ///     ins and 3 outs.
    /// </summary>
    /// <remarks>
    ///     I started looking at this because I wasn't sure how to return the out value and my googling led to the above link.
    ///     However it turns out, and I don't know if it's because of a later change to Moq, that IF all you're after is
    ///     setting the out parameters there's a much simpler way than having written all of these overloaded extension
    ///     methods. Take a look at the MoqExtension tests in the MoonriseTestUtilsTests project. There is still a relevance
    ///     for these extension methods though and that is for when you might want to DO something in the callback such as
    ///     compute the out value. Then you do need these methods. But if it's just getting fixed values out, set them up in
    ///     the setup - kinda as normal!
    /// </remarks>
    public static class MoqExtensions
    {
        /// <summary>
        /// No inputs, 1 output
        /// </summary>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="outVal">The out value.</param>
        public delegate void OutAction1<TOut>(out TOut outVal);

        /// <summary>
        /// One input, 1 output
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="outVal">The out value.</param>
        public delegate void OutAction1_1<in T1, TOut>(T1 arg1, out TOut outVal);

        /// <summary>
        /// One input, 2 outputs
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="TOut1">The type of the out1.</typeparam>
        /// <typeparam name="TOut2">The type of the out2.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="outVal1">The out val1.</param>
        /// <param name="outVal2">The out val2.</param>
        public delegate void OutAction1_2<in T1, TOut1, TOut2>(T1 arg1, out TOut1 outVal1, out TOut2 outVal2);

        /// <summary>
        /// One input, 3 outputs
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="TOut1">The type of the out1.</typeparam>
        /// <typeparam name="TOut2">The type of the out2.</typeparam>
        /// <typeparam name="TOut3">The type of the out3.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="outVal1">The out val1.</param>
        /// <param name="outVal2">The out val2.</param>
        /// <param name="outVal3">The out val3.</param>
        public delegate void OutAction1_3<in T1, TOut1, TOut2, TOut3>(T1 arg1, out TOut1 outVal1, out TOut2 outVal2, out TOut3 outVal3);

        /// <summary>
        /// 0 inputs, 2 outputs
        /// </summary>
        /// <typeparam name="TOut1">The type of the out1.</typeparam>
        /// <typeparam name="TOut2">The type of the out2.</typeparam>
        /// <param name="outVal1">The out val1.</param>
        /// <param name="outVal2">The out val2.</param>
        public delegate void OutAction2<TOut1, TOut2>(out TOut1 outVal1, out TOut2 outVal2);

        /// <summary>
        /// Two inputs, 1 output
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="outVal">The out value.</param>
        public delegate void OutAction2_1<in T1, in T2, TOut>(T1 arg1, T2 arg2, out TOut outVal);

        /// <summary>
        /// Two inputs, 2 outputs
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="TOut1">The type of the out1.</typeparam>
        /// <typeparam name="TOut2">The type of the out2.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="outVal1">The out val1.</param>
        /// <param name="outVal2">The out val2.</param>
        public delegate void OutAction2_2<in T1, in T2, TOut1, TOut2>(T1 arg1, T2 arg2, out TOut1 outVal1, out TOut2 outVal2);

        /// <summary>
        /// Two inputs, 3 outputs
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="TOut1">The type of the out1.</typeparam>
        /// <typeparam name="TOut2">The type of the out2.</typeparam>
        /// <typeparam name="TOut3">The type of the out3.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="outVal1">The out val1.</param>
        /// <param name="outVal2">The out val2.</param>
        /// <param name="outVal3">The out val3.</param>
        public delegate void OutAction2_3<in T1, in T2, TOut1, TOut2, TOut3>(T1 arg1,
                                                                             T2 arg2,
                                                                             out TOut1 outVal1,
                                                                             out TOut2 outVal2,
                                                                             out TOut3 outVal3
        );

        /// <summary>
        /// 0 inputs, 3 outputs
        /// </summary>
        /// <typeparam name="TOut1">The type of the out1.</typeparam>
        /// <typeparam name="TOut2">The type of the out2.</typeparam>
        /// <typeparam name="TOut3">The type of the out3.</typeparam>
        /// <param name="outVal1">The out val1.</param>
        /// <param name="outVal2">The out val2.</param>
        /// <param name="outVal3">The out val3.</param>
        public delegate void OutAction3<TOut1, TOut2, TOut3>(out TOut1 outVal1, out TOut2 outVal2, out TOut3 outVal3);

        /// <summary>
        /// 3 inputs, 1 output
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="outVal">The out value.</param>
        public delegate void OutAction3_1<in T1, in T2, in T3, TOut>(T1 arg1, T2 arg2, T3 arg3, out TOut outVal);

        /// <summary>
        /// 3 inputs, 2 outputs
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="TOut1">The type of the out1.</typeparam>
        /// <typeparam name="TOut2">The type of the out2.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="outVal1">The out val1.</param>
        /// <param name="outVal2">The out val2.</param>
        public delegate void OutAction3_2<in T1, in T2, in T3, TOut1, TOut2>(T1 arg1, T2 arg2, T3 arg3, out TOut1 outVal1, out TOut2 outVal2);

        /// <summary>
        /// 3 inputs, 3 outputs
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="TOut1">The type of the out1.</typeparam>
        /// <typeparam name="TOut2">The type of the out2.</typeparam>
        /// <typeparam name="TOut3">The type of the out3.</typeparam>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="outVal1">The out val1.</param>
        /// <param name="outVal2">The out val2.</param>
        /// <param name="outVal3">The out val3.</param>
        public delegate void OutAction3_3<in T1, in T2, in T3, TOut1, TOut2, TOut3>(
            T1 arg1,
            T2 arg2,
            T3 arg3,
            out TOut1 outVal1,
            out TOut2 outVal2,
            out TOut3 outVal3);

        /// <summary>
        ///     Callback invoker for functions that take 0 in params and 1 out param
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, TOut>(this ICallback<TMock, TReturn> mock, OutAction1<TOut> action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 0 in params and 2 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, TOut1, TOut2>(this ICallback<TMock, TReturn> mock,
                                                                                               OutAction2<TOut1, TOut2> action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 0 in params and 3 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, TOut1, TOut2, TOut3>(this ICallback<TMock, TReturn> mock,
                                                                                                      OutAction3<TOut1, TOut2, TOut3> action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        // One input, 1-3 outputs
        /// <summary>
        ///     Callback invoker for functions that take 1 in param and 1 out param
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, TOut>(this ICallback<TMock, TReturn> mock,
                                                                                           OutAction1_1<T1, TOut> action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 1 in param and 2 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, TOut1, TOut2>(this ICallback<TMock, TReturn> mock,
                                                                                                   OutAction1_2<T1, TOut1, TOut2> action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 1 in param and 3 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, TOut1, TOut2, TOut3>(this ICallback<TMock, TReturn> mock,
                                                                                                          OutAction1_3<T1, TOut1, TOut2, TOut3>
                                                                                                              action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 2 in params and 1 out param
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, T2, TOut>(this ICallback<TMock, TReturn> mock,
                                                                                               OutAction2_1<T1, T2, TOut> action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 2 in params and 2 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, T2, TOut1, TOut2>(this ICallback<TMock, TReturn> mock,
                                                                                                       OutAction2_2<T1, T2, TOut1, TOut2> action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 2 in params and 3 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, T2, TOut1, TOut2, TOut3>(this ICallback<TMock, TReturn> mock,
                                                                                                              OutAction2_3
                                                                                                                  <T1, T2, TOut1, TOut2, TOut3>
                                                                                                                  action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 3 in params and 1 out param
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, T2, T3, TOut>(this ICallback<TMock, TReturn> mock,
                                                                                                   OutAction3_1<T1, T2, T3, TOut> action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 3 in params and 2 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, T2, T3, TOut1, TOut2>(this ICallback<TMock, TReturn> mock,
                                                                                                           OutAction3_2<T1, T2, T3, TOut1, TOut2>
                                                                                                               action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for functions that take 3 in params and 3 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static IReturnsThrows<TMock, TReturn> OutCallback<TMock, TReturn, T1, T2, T3, TOut1, TOut2, TOut3>(this ICallback<TMock, TReturn> mock,
                                                                                                                  OutAction3_3
                                                                                                                  <T1, T2, T3, TOut1, TOut2, TOut3
                                                                                                                  > action)
            where TMock : class
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take no in params and 1 out param
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<TOut>(this ICallback mock, OutAction1<TOut> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take no in params and 2 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<TOut1, TOut2>(this ICallback mock, OutAction2<TOut1, TOut2> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take no in params and 3 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<TOut1, TOut2, TOut3>(this ICallback mock, OutAction3<TOut1, TOut2, TOut3> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 1 in param and 1 out param
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, TOut>(this ICallback mock, OutAction1_1<T1, TOut> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 1 in param and 2 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, TOut1, TOut2>(this ICallback mock, OutAction1_2<T1, TOut1, TOut2> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 1 in param and 3 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, TOut1, TOut2, TOut3>(this ICallback mock, OutAction1_3<T1, TOut1, TOut2, TOut3> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 2 in params and 1 out param
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, T2, TOut>(this ICallback mock, OutAction2_1<T1, T2, TOut> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 2 in params and 2 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, T2, TOut1, TOut2>(this ICallback mock, OutAction2_2<T1, T2, TOut1, TOut2> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 2 in params and 3 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, T2, TOut1, TOut2, TOut3>(this ICallback mock, OutAction2_3<T1, T2, TOut1, TOut2, TOut3> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 3 in params and 1 out param
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, T2, T3, TOut>(this ICallback mock, OutAction3_1<T1, T2, T3, TOut> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 3 in params and 2 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, T2, T3, TOut1, TOut2>(this ICallback mock, OutAction3_2<T1, T2, T3, TOut1, TOut2> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Callback invoker for procedures that take 3 in params and 3 out params
        /// </summary>
        /// <returns>The passed mock - for fluent chaining</returns>
        public static ICallbackResult OutCallback<T1, T2, T3, TOut1, TOut2, TOut3>(this ICallback mock,
                                                                                   OutAction3_3<T1, T2, T3, TOut1, TOut2, TOut3> action)
        {
            return OutCallbackInternal(mock, action);
        }

        /// <summary>
        ///     Common callback invoker for functions (i.e. methods that return stuff)
        /// </summary>
        /// <typeparam name="TMock">The type of the mock.</typeparam>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="mock">The mock that this is an extension for.</param>
        /// <param name="action">The action with out params.</param>
        /// <returns>The passed mock - for fluent chaining</returns>
        private static IReturnsThrows<TMock, TReturn> OutCallbackInternal<TMock, TReturn>(ICallback<TMock, TReturn> mock, object action)
            where TMock : class
        {
            MethodInfo methodInfo =
                mock.GetType()
                    .GetTypeInfo()
                    .Assembly.GetType("Moq.MethodCall")
                    .GetTypeInfo()
                    .GetDeclaredMethod("SetCallbackWithArguments");
            methodInfo.Invoke(mock,
                              new[]
                              {
                                  action
                              });
            return mock as IReturnsThrows<TMock, TReturn>;
        }

        /// <summary>
        ///     Common callback invoker for procedures (i.e. methods that don't return stuff)
        /// </summary>
        /// <param name="mock">The mock that this is an extension for.</param>
        /// <param name="action">The action with out params.</param>
        /// <returns>The passed mock - for fluent chaining</returns>
        private static ICallbackResult OutCallbackInternal(ICallback mock, object action)
        {
            MethodInfo methodInfo =
                mock.GetType()
                    .GetTypeInfo()
                    .Assembly.GetType("Moq.MethodCall")
                    .GetTypeInfo()
                    .GetDeclaredMethod("SetCallbackWithArguments");
            methodInfo.Invoke(mock,
                              new[]
                              {
                                  action
                              });
            return (ICallbackResult)mock;
        }
    }
}
