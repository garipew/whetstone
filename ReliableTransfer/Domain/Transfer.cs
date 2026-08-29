namespace ReliableTransfer.Domain; 

public class Transfer
{
	public int Id { get; set; }
	public decimal Amount { get; set; }

	public int SenderId { get; set; }
	public int ReceiverId { get; set; }

	public string Status { get; private set; }

	public Transfer(int senderId, int receiverId, decimal amount)
	{
		Amount = amount;
		SenderId = senderId;
		ReceiverId = receiverId;
		Status = "pending";
	}

	public void Complete()
	{
		Status = "complete";
	}
}
