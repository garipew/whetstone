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

	public Transfer(int id, int senderId, int receiverId, decimal amount)
	{
		Id = id;
		Amount = amount;
		SenderId = senderId;
		ReceiverId = receiverId;
		Status = "pending";
	}

	public bool Validate()
	{
		return Amount > 0 && SenderId != ReceiverId;
	}

	public void Complete()
	{
		Status = "complete";
	}
}
