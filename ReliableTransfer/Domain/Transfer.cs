namespace ReliableTransfer.Domain; 

public class Transfer
{
	public Guid Id { get; set; }
	public decimal Amount { get; set; }

	public Guid SenderId { get; set; }
	public Guid ReceiverId { get; set; }

	public string Status { get; private set; }

	public Transfer(Guid senderId, Guid receiverId, decimal amount)
	{
		Id = new Guid();
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
