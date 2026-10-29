namespace Reptile
{
	public interface IMultiTransactionHandler
	{
		int GetTransactionsCount();

		ATransaction GetTransaction(int index);
	}
}
