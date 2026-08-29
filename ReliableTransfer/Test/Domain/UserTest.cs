using ReliableTransfer.Domain;

namespace ReliableTransfer.Test;

public class UserTest
{
    [Fact]
    public void Credit_ShouldCredit()
    {
	var user = new User();

	user.Credit(27m);
	Assert.Equal(27m, user.Balance);
    }

    [Fact]
    public void Debit_ShouldThrowOnInsufficientBalance()
    {
	var user = new User();

	Assert.Equal(0m, user.Balance);
	Assert.Throws<InsufficientBalanceException>(() => user.Debit(27m));
    }
}
