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

        [Theory]
        [InlineData("negative case", -1)]
        [InlineData("on s'en fou", 0)]
        [InlineData("positive case", 1)]
        public void PapaTest(string description, int value)
        {
        }

        [Fact]
        public async Task AttachTest_Change_Before_Attach_Is_Not_Count()
        {
            //ARRANGE: clean the database and insert a new entity1

            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            Input1History entity1;
            using (var dbContext = new Context(connection))
            {
                await dbContext.Database.ExecuteSqlRawAsync("Delete Input1s");
                var entry1 = await dbContext.Input1s.AddAsync(new Input1History { Input = "aa", Output = "bb" });
                dbContext.SaveChanges();
                entity1 = entry1.Entity;
            }

            Assert.True(entity1.Id > 0); //make sure the entity is saved in the database
            Assert.Equal("bb", entity1.Output);

            /*
             * ACT: we have a entity1 object predefine 
             * we change this object before Attach it to the dbContext 
             * the changes will not counted and the SaveChanges won't do anything
             */

            entity1.Output = "cc"; // change before tracking
            using (var dbContext = new Context(connection))
            {
                var entry1 = dbContext.Attach(entity1); //start tracking
                dbContext.SaveChanges(); //no changes is detected
            }

            //ASSERT: expect nothing change in the database
            using (var dbContext = new Context(connection))
            {
                var currentEntity = await dbContext.Input1s!.FirstAsync(i => i.Id == entity1.Id);
                Assert.Equal("bb", currentEntity.Output);
            }
        }

        [Fact]
        public async Task AttachTest_Change_After_Attach_Is_Count()
        {
            //ARRANGE: clean the database and insert a new entity1

            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            Input1History entity1;
            using (var dbContext = new Context(connection))
            {
                await dbContext.Database.ExecuteSqlRawAsync("Delete Input1s");
                var entry1 = await dbContext.Input1s.AddAsync(new Input1History { Input = "aa", Output = "bb" });
                dbContext.SaveChanges();
                entity1 = entry1.Entity;
            }

            Assert.True(entity1.Id > 0); //make sure the entity is saved in the database
            Assert.Equal("bb", entity1.Output);

            /*
             * ACT: we have a entity1 object predefine 
             * we change this object after Attach it to the dbContext 
             * the changes will be tracked and and the SaveChanges will update the database
             */

            using (var dbContext = new Context(connection))
            {
                var entry1 = dbContext.Attach(entity1); //start tracking
                entity1.Output = "cc"; // change after tracking
                dbContext.SaveChanges(); //changes is saved
            }

            //ASSERT: expect change in the database
            using (var dbContext = new Context(connection))
            {
                var currentEntity = await dbContext.Input1s!.FirstAsync(i => i.Id == entity1.Id);
                Assert.Equal("cc", currentEntity.Output);
            }
        }

        [Fact]
        public async Task AttachTest_Manually_Mark_Changes()
        {
            //ARRANGE: clean the database and insert a new entity1

            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            Input1History entity1;
            using (var dbContext = new Context(connection))
            {
                await dbContext.Database.ExecuteSqlRawAsync("Delete Input1s");
                var entry1 = await dbContext.Input1s.AddAsync(new Input1History { Input = "aa", Output = "bb" });
                dbContext.SaveChanges();
                entity1 = entry1.Entity;
            }

            Assert.True(entity1.Id > 0); //make sure the entity is saved in the database
            Assert.Equal("bb", entity1.Output);

            /*
             * ACT: we have a entity1 object predefine 
             * we change this object before Attach it to the dbContext 
             * then we manually mark as changes to tell EF that the entity is changed
             */

            entity1.Output = "cc"; // change before tracking
            entity1.Input = "xx"; // change before tracking
            using (var dbContext = new Context(connection))
            {
                var entry1 = dbContext.Attach(entity1); //start tracking

                //tell EF that the entity was changes but do not tell what member is changed it is
                entry1.State = EntityState.Modified;

                dbContext.SaveChanges(); //update the entity in the database
            }

            //ASSERT: expect changes in the database
            using (var dbContext = new Context(connection))
            {
                var currentEntity = await dbContext.Input1s!.FirstAsync(i => i.Id == entity1.Id);
                Assert.Equal("cc", currentEntity.Output);
                Assert.Equal("xx", currentEntity.Input);
            }
        }
    }
}