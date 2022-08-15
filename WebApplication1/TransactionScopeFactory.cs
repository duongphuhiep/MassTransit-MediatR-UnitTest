using System.Transactions;

namespace WebApplication1;

public static class TransactionScopeFactory
{
    private static TransactionOptions RepeatableReadTransactionOptions = new TransactionOptions
    {
        IsolationLevel = IsolationLevel.RepeatableRead
    };

    public static TransactionScope CreateNew()
    {
        return new TransactionScope(
            scopeOption: TransactionScopeOption.Required,
            transactionOptions: RepeatableReadTransactionOptions,
            asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled
        );
    }
}
