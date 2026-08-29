using ReliableTransfer.Domain;

namespace ReliableTransfer.Application;

public class TransferApplication
{
	private readonly IUserRepository users;
	private readonly ITransferRepository transfers;

	public TransferApplication(IUserRepository userRepo, ITransferRepository transferRepo)
	{
		users = userRepo;
		transfers = transferRepo;
	}

	public Transfer QueueTransfer(int senderId, int receiverId, decimal amount)
	{
		var transfer = new Transfer(senderId, receiverId, amount);
		transfers.Add(transfer);

		return transfer;
	}

	public void ProcessTransfer(Transfer transfer)
	{
		var sender = users.Get(transfer.SenderId);
		var receiver = users.Get(transfer.ReceiverId);

		sender.Debit(transfer.Amount);
		receiver.Credit(transfer.Amount);

		transfer.Complete();

		users.Save(sender);
		users.Save(receiver);

		transfers.Save(transfer);
	}
}
