#region Apache-v2.0

//    Copyright 2016 Will Hopkins - Moonrise Media Ltd.
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
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Database.Transactions;
using Moq;

namespace Moonrise.Utils.Standard.Database.Tests
{
    [TestClass]
    public class TransactedConnectionTests
    {
        public class MockedConnection : TransactedConnection.IConnection
        {
            public ConnectionState State { get; set; }

            /// <summary>
            ///     Gets the underlying <see cref="SqlConnection" />
            /// </summary>
            /// <returns>The actual connection</returns>
            public SqlConnection AsSqlConnection()
            {
                throw new NotImplementedException();
            }

            public void Close() { }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose() { }

            public void Open() { }
        }

        private TransactedConnection.ITransaction MockedTransaction { get; set; }

        public static TransactedConnection.IConnection MockedConnectionFactory(string connectionString)
        {
            TransactedConnection.IConnection retVal = new MockedConnection();
            return retVal;
        }

        [TestMethod]
        public void CompleteOrderDoesntAffectActualComplete()
        {
            TransactedConnection.TransactionFactoryMethod = MockedTransactionFactory;
            TransactedConnection.ConnectionFactoryMethod = MockedConnectionFactory;

            Mock<TransactedConnection.ITransaction> mockedTransaction = new Mock<TransactedConnection.ITransaction>(MockBehavior.Strict);
            MockedTransaction = mockedTransaction.Object;

            string connStr1 = "One";

            using (TransactedConnection sut1 = new TransactedConnection(connStr1))
            {
                sut1.Complete();

                using (TransactedConnection sut2 = new TransactedConnection(connStr1))
                {
                    sut2.Complete();
                }

                mockedTransaction.VerifyAll();

                // We tell the mock that Complete & Dispose is now expected to be called - It shouldn't have been called in the sut.Complete
                mockedTransaction.Setup(mt => mt.Complete());
                mockedTransaction.Setup(mt => mt.Dispose());
            }

            mockedTransaction.VerifyAll();

            using (TransactedConnection sut1 = new TransactedConnection(connStr1))
            {
                using (TransactedConnection sut2 = new TransactedConnection(connStr1))
                {
                    sut2.Complete();
                    sut1.Complete();
                }

                mockedTransaction.VerifyAll();

                // We tell the mock that Complete & Dispose is now expected to be called - It shouldn't have been called in the sut.Complete
                mockedTransaction.Setup(mt => mt.Complete());
                mockedTransaction.Setup(mt => mt.Dispose());
            }

            mockedTransaction.VerifyAll();
        }

        [TestMethod]
        public void LeavingUsingCallsActualComplete()
        {
            TransactedConnection.TransactionFactoryMethod = MockedTransactionFactory;
            TransactedConnection.ConnectionFactoryMethod = MockedConnectionFactory;

            Mock<TransactedConnection.ITransaction> mockedTransaction = new Mock<TransactedConnection.ITransaction>(MockBehavior.Strict);
            mockedTransaction.Setup(mt => mt.Dispose());
            MockedTransaction = mockedTransaction.Object;

            string connStr1 = "One";

            using (TransactedConnection sut = new TransactedConnection(connStr1))
            {
                sut.Complete();

                // We tell the mock that Complete is now expected to be called - It shouldn't have been called in the sut.Complete
                mockedTransaction.Setup(mt => mt.Complete());
            }

            mockedTransaction.VerifyAll();
        }

        [TestMethod]
        public void LeavingUsingDoesntCallCompleteWhenNotCalled()
        {
            TransactedConnection.TransactionFactoryMethod = MockedTransactionFactory;
            TransactedConnection.ConnectionFactoryMethod = MockedConnectionFactory;

            Mock<TransactedConnection.ITransaction> mockedTransaction = new Mock<TransactedConnection.ITransaction>(MockBehavior.Strict);
            mockedTransaction.Setup(mt => mt.Dispose());
            MockedTransaction = mockedTransaction.Object;

            string connStr1 = "One";

            using (TransactedConnection sut = new TransactedConnection(connStr1)) { }

            mockedTransaction.VerifyAll();
        }

        public TransactedConnection.ITransaction MockedTransactionFactory()
        {
            return MockedTransaction;
        }

        [TestMethod]
        public void MultipleCompleteDoesntAffectActualComplete()
        {
            TransactedConnection.TransactionFactoryMethod = MockedTransactionFactory;
            TransactedConnection.ConnectionFactoryMethod = MockedConnectionFactory;

            Mock<TransactedConnection.ITransaction> mockedTransaction = new Mock<TransactedConnection.ITransaction>(MockBehavior.Strict);
            MockedTransaction = mockedTransaction.Object;

            string connStr1 = "One";

            using (TransactedConnection sut1 = new TransactedConnection(connStr1))
            {
                sut1.Complete();
                sut1.Complete();

                using (TransactedConnection sut2 = new TransactedConnection(connStr1))
                {
                    sut2.Complete();
                    sut2.Complete();
                }

                mockedTransaction.VerifyAll();

                // We tell the mock that Complete & Dispose is now expected to be called - It shouldn't have been called in the sut.Complete
                mockedTransaction.Setup(mt => mt.Complete());
                mockedTransaction.Setup(mt => mt.Dispose());
            }

            mockedTransaction.VerifyAll();

            using (TransactedConnection sut1 = new TransactedConnection(connStr1))
            {
                using (TransactedConnection sut2 = new TransactedConnection(connStr1))
                {
                    sut2.Complete();
                    sut1.Complete();
                    sut2.Complete();
                    sut1.Complete();
                }

                mockedTransaction.VerifyAll();

                // We tell the mock that Complete & Dispose is now expected to be called - It shouldn't have been called in the sut.Complete
                mockedTransaction.Setup(mt => mt.Complete());
                mockedTransaction.Setup(mt => mt.Dispose());
            }

            mockedTransaction.VerifyAll();
        }

        [TestMethod]
        public void NestedCompleteDoesntCompletesIfInnerNotComplete()
        {
            TransactedConnection.TransactionFactoryMethod = MockedTransactionFactory;
            TransactedConnection.ConnectionFactoryMethod = MockedConnectionFactory;

            Mock<TransactedConnection.ITransaction> mockedTransaction = new Mock<TransactedConnection.ITransaction>(MockBehavior.Strict);
            MockedTransaction = mockedTransaction.Object;

            string connStr1 = "One";

            using (TransactedConnection sut1 = new TransactedConnection(connStr1))
            {
                using (TransactedConnection sut2 = new TransactedConnection(connStr1)) { }

                mockedTransaction.VerifyAll();

                // We tell the mock that Dispose is now expected to be called - It shouldn't have been called in the sut.Complete
                mockedTransaction.Setup(mt => mt.Dispose());
                sut1.Complete();
            }

            mockedTransaction.VerifyAll();
        }

        [TestMethod]
        public void NestedCompleteDoesntCompletesIfOuterNotComplete()
        {
            TransactedConnection.TransactionFactoryMethod = MockedTransactionFactory;
            TransactedConnection.ConnectionFactoryMethod = MockedConnectionFactory;

            Mock<TransactedConnection.ITransaction> mockedTransaction = new Mock<TransactedConnection.ITransaction>(MockBehavior.Strict);
            MockedTransaction = mockedTransaction.Object;

            string connStr1 = "One";

            using (TransactedConnection sut1 = new TransactedConnection(connStr1))
            {
                using (TransactedConnection sut2 = new TransactedConnection(connStr1))
                {
                    sut2.Complete();
                }

                mockedTransaction.VerifyAll();

                // We tell the mock that Dispose is now expected to be called - It shouldn't have been called in the sut.Complete
                mockedTransaction.Setup(mt => mt.Dispose());
            }

            mockedTransaction.VerifyAll();
        }

        [TestMethod]
        public void NestedCompleteOnlyCompletesOnLeavingLastUsing()
        {
            TransactedConnection.TransactionFactoryMethod = MockedTransactionFactory;
            TransactedConnection.ConnectionFactoryMethod = MockedConnectionFactory;

            Mock<TransactedConnection.ITransaction> mockedTransaction = new Mock<TransactedConnection.ITransaction>(MockBehavior.Strict);
            MockedTransaction = mockedTransaction.Object;

            string connStr1 = "One";

            using (TransactedConnection sut1 = new TransactedConnection(connStr1))
            {
                using (TransactedConnection sut2 = new TransactedConnection(connStr1))
                {
                    sut2.Complete();
                }

                mockedTransaction.VerifyAll();

                // We tell the mock that Dispose & Complete is now expected to be called - It shouldn't have been called in the sut.Complete
                mockedTransaction.Setup(mt => mt.Dispose());
                mockedTransaction.Setup(mt => mt.Complete());
                sut1.Complete();
            }

            mockedTransaction.VerifyAll();
        }
    }
}
