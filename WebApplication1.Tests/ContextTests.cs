using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace WebApplication1.Tests
{
    /*
     https://docs.microsoft.com/en-us/ef/core/saving/transactions#using-systemtransactions
     */
    public class ContextTests
    {
        private readonly string connectionString = "Data Source=CHATAIGNE\\SQLEXPRESS;Initial Catalog=MassTransit_Mediator_UnitTest; User Id=sa; Password=CheminDuCitron.07";

        private readonly ITestOutputHelper _testOutputHelper;

        public ContextTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void OK_CommittableTransactionTest()
        {
            //using (var transaction = new CommittableTransaction(
            //    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            //{
            //    var connection = new SqlConnection(connectionString);

            //    var options = new DbContextOptionsBuilder<Context>()
            //        .UseSqlServer(connection)
            //        .Options;

            //    using (var context = new Context(options))
            //    {
            //        context.Database.OpenConnection();
            //        context.Database.EnlistTransaction(transaction);

            //        // Run raw ADO.NET command in the transaction
            //        var command = connection.CreateCommand();
            //        command.CommandText = "DELETE FROM dbo.Input1s";
            //        command.ExecuteNonQuery();

            //        // Run an EF Core command in the transaction
            //        context.Input1s!.Add(new Input1History { Input = "Foo", Output = "Bar" });
            //        context.SaveChanges();
            //        context.Database.CloseConnection();
            //    }

            //    // Commit transaction if all commands succeed, transaction will auto-rollback
            //    // when disposed if either commands fails
            //    transaction.Commit();
            //}
        }

        [Fact]
        public void Failed_CommittableTransaction_2DbContext()
        {
            using (var transaction = new CommittableTransaction(
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                using (var context = new Context(new DbContextOptionsBuilder<Context>()
                    .UseSqlServer(connectionString)
                    .Options))
                {
                    context.Database.OpenConnection();
                    context.Database.EnlistTransaction(transaction);

                    context.Input1s!.Add(new Input1History { Input = "Foo1", Output = "Bar2" });
                    context.SaveChanges();
                }

                using (var context = new Context(new DbContextOptionsBuilder<Context>()
                    .UseSqlServer(connectionString)
                    .Options))
                {
                    context.Database.OpenConnection();
                    context.Database.EnlistTransaction(transaction);

                    context.Input1s!.Add(new Input1History { Input = "Foo2", Output = "Bar1" });
                    context.SaveChanges();
                }

                // Commit transaction if all commands succeed, transaction will auto-rollback
                // when disposed if either commands fails
                //transaction.Commit();
            }
        }

        [Fact]
        public async Task Failed_TransactionScope_2DbContextAsync()
        {
            using (var scope = TransactionScopeFactory.CreateNew())
            {
                Task t1 = Task.Run(async () =>
                {
                    await Task.Delay(1);
                    using (var context = new Context(new DbContextOptionsBuilder<Context>()
                   .UseSqlServer(connectionString)
                   .Options))
                    {
                        context.Input1s!.Add(new Input1History { Input = "Foo1", Output = "Bar2" });
                        context.SaveChanges();
                    }
                });

                Task t2 = Task.Run(async () =>
                {
                    using (var context = new Context(new DbContextOptionsBuilder<Context>()
                        .UseSqlServer(connectionString)
                        .Options))
                    {
                        context.Input1s!.Add(new Input1History { Input = "Foo2", Output = "Bar1" });
                        await Task.Delay(1);
                        context.SaveChanges();
                    }
                });

                await Task.WhenAll(t1, t2);

                // Commit transaction if all commands succeed, transaction will auto-rollback
                // when disposed if either commands fails
                scope.Complete();
            }
        }

        [Fact]
        public async Task OK_TransactionCommited_DifferentThreads()
        {
            using (var transaction = new CommittableTransaction(
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    connection.EnlistTransaction(transaction);

                    //Task t1 = Task.Run(() => InsertHistory(connection));
                    //Task t2 = Task.Run(() => InsertHistory(connection));
                    //await Task.WhenAll(t1, t2);

                    await InsertHistory(connection);
                    await InsertHistory(connection);

                    transaction.Commit();
                }
            }
        }

        private async Task InsertHistory(SqlConnection connection)
        {
            try
            {
                using (var context = new Context(connection))
                {
                    await context.Input1s!.AddAsync(new Input1History { Id = 0, Input = "Foo3", Output = "Bar3" });
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine(ex.ToString());
            }
        }
    }
}