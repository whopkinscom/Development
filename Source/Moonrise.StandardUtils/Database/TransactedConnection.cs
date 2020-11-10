//// <copyright file="TransactedConnection.cs" company="Moonrise Media Ltd.">
//// Originally written by WillH - with any acknowledgements as required. Once checked in to your version control you have full rights except for selling the source!
//// </copyright>

//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Threading;
//using System.Transactions;
//using IsolationLevel = System.Transactions.IsolationLevel;

//namespace Moonrise.Utils.Standard.Database
//{
//    /// <summary>
//    ///     Allows for nested transaction connections so that you never need to worry about nesting your connections. The
//    ///     outermost TransactedConnection will start a transaction and all connections with the same connection string will
//    ///     share the same connection within the overall transaction.
//    /// </summary>
//    /// <seealso cref="System.IDisposable" />
//    public class TransactedConnection : IDisposable
//    {
//        /// <summary>
//        /// Supplies a default connection that is, the usually expected, <see cref="SqlConnection"/>
//        /// </summary>
//        /// <seealso cref="Moonrise.Utils.Standard.Database.TransactedConnection.IConnection" />
//        public class DefaultConnection : IConnection
//        {
//            private readonly SqlConnection _actualConnection;

//            /// <summary>
//            /// Initializes a new instance of the <see cref="DefaultConnection"/> class.
//            /// </summary>
//            /// <param name="actualConnection">The actual connection.</param>
//            public DefaultConnection(SqlConnection actualConnection)
//            {
//                _actualConnection = actualConnection;
//            }

//            /// <summary>
//            /// Gets the actual connection state.
//            /// </summary>
//            public ConnectionState State
//            {
//                get
//                {
//                    return _actualConnection.State;
//                }
//            }

//            /// <summary>
//            ///     Gets the underlying <see cref="SqlConnection" />
//            /// </summary>
//            /// <returns>
//            ///     The actual connection
//            /// </returns>
//            public SqlConnection AsSqlConnection()
//            {
//                return _actualConnection;
//            }

//            /// <summary>
//            /// Closes this actual connection
//            /// </summary>
//            public void Close()
//            {
//                _actualConnection.Close();
//            }

//            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
//            public void Dispose()
//            {
//                _actualConnection.Dispose();
//            }

//            /// <summary>
//            /// Opens the actual connection
//            /// </summary>
//            public void Open()
//            {
//                _actualConnection.Open();
//            }
//        }

//        /// <summary>
//        /// Supplies a default transaction that is the usually expected <see cref="TransactionScope" />.
//        /// </summary>
//        /// <seealso cref="Moonrise.Utils.Standard.Database.TransactedConnection.ITransaction" />
//        public class DefaultTransaction : ITransaction
//        {
//            private readonly TransactionScope _transactionScope;

//            /// <summary>
//            /// Initializes a new instance of the <see cref="DefaultTransaction"/> class.
//            /// </summary>
//            /// <param name="transactionScope">The transaction scope.</param>
//            public DefaultTransaction(TransactionScope transactionScope)
//            {
//                _transactionScope = transactionScope;
//            }

//            /// <summary>
//            ///     Gets the underlying <see cref="TransactionScope" />
//            /// </summary>
//            /// <returns>
//            ///     The actual scope
//            /// </returns>
//            public TransactionScope AsTransactionScope()
//            {
//                return _transactionScope;
//            }

//            /// <summary>
//            /// Completes the actual transaction
//            /// </summary>
//            public void Complete()
//            {
//                _transactionScope.Complete();
//            }

//            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
//            public void Dispose()
//            {
//                _transactionScope.Dispose();
//            }
//        }

//        /// <summary>
//        ///     Represents the methods required on a connection
//        /// </summary>
//        /// <seealso cref="System.IDisposable" />
//        public interface IConnection : IDisposable
//        {
//            /// <summary>
//            /// Gets the connection state.
//            /// </summary>
//            ConnectionState State { get; }

//            /// <summary>
//            ///     Gets the underlying <see cref="SqlConnection" />
//            /// </summary>
//            /// <returns>The actual connection</returns>
//            SqlConnection AsSqlConnection();

//            /// <summary>
//            /// Closes the connection
//            /// </summary>
//            void Close();

//            /// <summary>
//            /// Opens the connection
//            /// </summary>
//            void Open();
//        }

//        /// <summary>
//        ///     Represents the methods required on a Transaction
//        /// </summary>
//        /// <seealso cref="System.IDisposable" />
//        public interface ITransaction : IDisposable
//        {
//            /// <summary>
//            ///     Gets the underlying <see cref="TransactionScope" />
//            /// </summary>
//            /// <returns>The actual scope</returns>
//            TransactionScope AsTransactionScope();

//            /// <summary>
//            /// Completes the transaction
//            /// </summary>
//            void Complete();
//        }

//        /// <summary>
//        /// A transaction that reference counts nested completions
//        /// </summary>
//        protected class CompletionCountedTransaction
//        {
//            /// <summary>
//            /// The completion count.
//            /// </summary>
//            public int CompletionCount { get; set; }

//            /// <summary>
//            /// The transaction implementation.
//            /// </summary>
//            public ITransaction Transaction { get; set; }
//        }

//        /// <summary>
//        ///     Reference counts an SQLConnection so that the same one can be shared whilst nested.
//        /// </summary>
//        protected class RefCountedSqlConnection
//        {
//            /// <summary>
//            ///     The reference count
//            /// </summary>
//            public int ReferenceCount { get; set; }

//            /// <summary>
//            ///     The SQL connection
//            /// </summary>
//            public IConnection SqlConnection { get; set; }
//        }

//        /// <summary>
//        ///     Method to allow testing to use a non database connection type
//        /// </summary>
//        /// <returns>The created IConnection</returns>
//        public delegate IConnection ConnectionFactory(string connectionString);

//        /// <summary>
//        ///     Method to allow testing to use a non database transaction type
//        /// </summary>
//        /// <returns>The created ITransaction</returns>
//        public delegate ITransaction TransactionFactory();

//        /// <summary>
//        ///     The transaction factory method which is only to be used for testing!
//        /// </summary>
//        public static TransactionFactory TransactionFactoryMethod = DefaultTransactionFactory;

//        /// <summary>
//        ///     The connection factory method which is only to be used for testing!
//        /// </summary>
//        public static ConnectionFactory ConnectionFactoryMethod = DefaultConnectionFactory;

//        /// <summary>
//        ///     The connection string dictionary to allow multiple connection string connections within the same
//        ///     TransactedConnection
//        /// </summary>
//        protected static ThreadLocal<Dictionary<string, RefCountedSqlConnection>> connectionStringDictionary =
//            new ThreadLocal<Dictionary<string, RefCountedSqlConnection>>();

//        /// <summary>
//        ///     Indicates that the transaction has already been nested
//        /// </summary>
//        protected static ThreadLocal<bool> NestedTransaction = new ThreadLocal<bool>();

//        /// <summary>
//        ///     The transaction depth count
//        /// </summary>
//        protected static ThreadLocal<int> TransactionCount = new ThreadLocal<int>();

//        /// <summary>
//        ///     The transaction scope to use for all TransactedConnections
//        /// </summary>
//        protected static ThreadLocal<CompletionCountedTransaction> TransactionScope = new ThreadLocal<CompletionCountedTransaction>();

//        /// <summary>
//        ///     Initializes a new instance of the <see cref="TransactedConnection" /> class.
//        /// </summary>
//        /// <param name="connectionString">The connection string.</param>
//        /// <exception cref="NoNullAllowedException">There was a mismatch between a zero reference count and a non null connection!</exception>
//        /// <exception cref="System.NullReferenceException">
//        ///     There was a mismatch between a non-zero reference count and a null
//        ///     connection!
//        /// </exception>
//        public TransactedConnection(string connectionString)
//        {
//            // Determine if this is a "first" stacked construction - for this thread (remember, all those variables you see with the .Value are thread statics.
//            if (TransactionCount.Value == 0)
//            {
//                // It is, so this isn't (yet) a nested transaction scope.
//                NestedTransaction.Value = false;

//                // So create a transaction scope object.
//                TransactionScope.Value = new CompletionCountedTransaction
//                                         {
//                                             Transaction = TransactionFactoryMethod(),
//                                             CompletionCount = 1
//                                         };

//                // And - for this thread - create a connection string dictionary so that we re-use any nested connections using the SAME connection string
//                connectionStringDictionary.Value = new Dictionary<string, RefCountedSqlConnection>();
//            }
//            else
//            {
//                TransactionScope.Value.CompletionCount++;
//                NestedTransaction.Value = true;
//            }

//            // Increase the nesting - for this thread.
//            TransactionCount.Value++;

//            RefCountedSqlConnection refCountedSqlConnection;

//            // Look to see if we can reuse a connection
//            if (connectionStringDictionary.Value.ContainsKey(connectionString))
//            {
//                refCountedSqlConnection = connectionStringDictionary.Value[connectionString];
//            }
//            else
//            {
//                refCountedSqlConnection = new RefCountedSqlConnection();
//                connectionStringDictionary.Value.Add(connectionString, refCountedSqlConnection);
//            }

//            if (refCountedSqlConnection.ReferenceCount == 0)
//            {
//                if (refCountedSqlConnection.SqlConnection != null)
//                {
//                    throw new NoNullAllowedException("There was a mismatch between a zero reference count and a non null connection!");
//                }

//                SqlConnection = ConnectionFactoryMethod(connectionString);
//                refCountedSqlConnection.SqlConnection = SqlConnection;

//                // This is the first level connection and so not nested.
//                Nested = false;
//            }
//            else
//            {
//                if (refCountedSqlConnection.SqlConnection == null)
//                {
//                    throw new NullReferenceException("There was a mismatch between a non-zero reference count and a null connection!");
//                }

//                SqlConnection = refCountedSqlConnection.SqlConnection;

//                // This IS nested
//                Nested = true;
//            }

//            ConnectionString = connectionString;
//            refCountedSqlConnection.ReferenceCount++;
//        }

//        /// <summary>
//        /// Indicates whether this <see cref="TransactedConnection"/> is completed.
//        /// </summary>
//        public bool Completed { get; set; }

//        /// <summary>
//        ///     The connection string for this TransactedConnection
//        /// </summary>
//        public string ConnectionString { get; protected set; }

//        /// <summary>
//        ///     Determines if the TransactedConnection is nested or not
//        /// </summary>
//        public bool Nested { get; protected set; }

//        /// <summary>
//        ///     The underlying SqlConnection
//        /// </summary>
//        protected IConnection SqlConnection { get; set; }

//        /// <summary>
//        ///     Performs an implicit conversion from <see cref="TransactedConnection" /> to <see cref="SqlConnection" />.
//        /// </summary>
//        /// <param name="transactedConnection">The transacted connection.</param>
//        /// <returns>
//        ///     The result of the conversion.
//        /// </returns>
//        public static implicit operator SqlConnection(TransactedConnection transactedConnection)
//        {
//            return transactedConnection.SqlConnection.AsSqlConnection();
//        }

//        /// <summary>
//        /// Performs an implicit conversion from <see cref="TransactedConnection"/> to <see cref="TransactionScope"/>.
//        /// </summary>
//        /// <param name="transactedConnection">The transacted connection.</param>
//        /// <returns>
//        /// The result of the conversion.
//        /// </returns>
//        public static implicit operator TransactionScope(TransactedConnection transactedConnection)
//        {
//            return TransactionScope.Value.Transaction.AsTransactionScope();
//        }

//        /// <summary>
//        ///     Completes the transaction - will only ACTUALLY complete at the outermost level and MUST be called at the end of
//        ///     even Read operations otherwise the reference counting will get out of whack.
//        /// </summary>
//        public void Complete()
//        {
//            if (!Completed)
//            {
//                TransactionScope.Value.CompletionCount--;
//                Completed = true;
//            }
//        }

//        /// <summary>
//        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
//        /// </summary>
//        /// <exception cref="System.NullReferenceException">
//        ///     There was a mismatch between a zero reference count and a null connection!
//        ///     or
//        ///     Sanity check failed, we can't own a reference count > 0 and not be Nested!
//        /// </exception>
//        /// <exception cref="NoNullAllowedException">Sanity check failed, we can't be Nested and own a reference count of 0!</exception>
//        public void Dispose()
//        {
//            if (!connectionStringDictionary.Value.ContainsKey(ConnectionString))
//            {
//                throw new NullReferenceException("There was a mismatch between a zero reference count and a null connection!");
//            }

//            RefCountedSqlConnection refCountedSqlConnection = connectionStringDictionary.Value[ConnectionString];

//            refCountedSqlConnection.ReferenceCount--;

//            if (refCountedSqlConnection.ReferenceCount == 0)
//            {
//                if (Nested)
//                {
//                    throw new NoNullAllowedException("Sanity check failed, we can't be Nested and own a reference count of 0!");
//                }

//                SqlConnection.Close();
//                refCountedSqlConnection.SqlConnection = null;
//            }
//            else if (!Nested)
//            {
//                throw new NullReferenceException("Sanity check failed, we can't own a reference count > 0 and not be Nested!");
//            }

//            TransactionCount.Value--;

//            if (TransactionCount.Value == 1)
//            {
//                NestedTransaction.Value = false;
//            }
//            else if (TransactionCount.Value == 0)
//            {
//                if (TransactionScope.Value.CompletionCount == 0)
//                {
//                    TransactionScope.Value.Transaction.Complete();
//                }

//                TransactionScope.Value.Transaction.Dispose();
//            }
//        }

//        /// <summary>
//        ///     Opens the connection, if it's already open, nothing extra will happen
//        /// </summary>
//        public void Open()
//        {
//            if (SqlConnection.State != ConnectionState.Open)
//            {
//                SqlConnection.Open();
//            }
//        }

//        /// <summary>
//        ///     Default factory to create a connection. Will be overridden by UnitTests
//        /// </summary>
//        /// <param name="connectionString">The connection string.</param>
//        /// <returns>A <see cref="DefaultConnection" /></returns>
//        protected static IConnection DefaultConnectionFactory(string connectionString)
//        {
//            IConnection retVal;
//            SqlConnection defaultconnection = new SqlConnection(connectionString);
//            retVal = new DefaultConnection(defaultconnection);
//            return retVal;
//        }

//        /// <summary>
//        ///     Default factory to create a transaction. Will be overridden by UnitTests
//        /// </summary>
//        /// <returns>A <see cref="DefaultTransaction" /></returns>
//        protected static ITransaction DefaultTransactionFactory()
//        {
//            ITransaction retVal;
//            TransactionScope defaultTransactionScope = new TransactionScope(TransactionScopeOption.Required,
//                                                                            new TransactionOptions
//                                                                            {
//                                                                                IsolationLevel = IsolationLevel.ReadCommitted,
//                                                                                Timeout = TransactionManager.MaximumTimeout
//                                                                            });
//            retVal = new DefaultTransaction(defaultTransactionScope);
//            return retVal;
//        }

//        /// <summary>
//        ///     Closes the connection, but only when we reach the matching open.
//        /// </summary>
//        protected void Close()
//        {
//            if (!Nested)
//            {
//                SqlConnection.Close();
//            }
//        }
//    }
//}
