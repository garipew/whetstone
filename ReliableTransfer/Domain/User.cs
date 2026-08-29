namespace ReliableTransfer.Domain;

public class User
{
	public Guid Id { get; set; }
	public decimal Balance { get; private set; }

	public User()
	{
		Id = new Guid();
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
