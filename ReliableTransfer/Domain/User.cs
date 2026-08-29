namespace ReliableTransfer.Domain;

public class User
{
	public int Id { get; set; }
	public decimal Balance { get; private set; }

	public User()
	{
		Balance = 0m;
	}

	public void Debit(decimal amount)
	{
		if (Balance < amount) {
			throw new InsufficientBalanceException();
		}

		Balance -= amount;
	}

	public void Credit(decimal amount)
	{
		Balance += amount;
	}
}

public class InsufficientBalanceException : Exception
{
	public InsufficientBalanceException() : base() {}
}
