using ReliableTransfer.Domain;
using ReliableTransfer.Infra;

namespace ReliableTransfer.Application;

public class TransferApplication
{
	private readonly UserRepository users;
	private readonly TransferRepository transfers;

	public TransferApplication(UserRepository userRepo, TransferRepository transferRepo)
	{
		users = userRepo;
		transfers = transferRepo;
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
