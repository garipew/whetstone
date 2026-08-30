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

	public async Task<Transfer> ProcessTransfer(Guid idempotency, int senderId, int receiverId, decimal amount)
	{
		Transfer? transfer = await transfers.GetByIdempotency(idempotency);
		if (transfer != null) {
			return transfer;
		}
		transfer = new Transfer(senderId, receiverId, amount);
		await transfers.Add(idempotency, transfer);

		var sender = await users.Get(transfer.SenderId);
		var receiver = await users.Get(transfer.ReceiverId);

		sender.Debit(transfer.Amount);
		receiver.Credit(transfer.Amount);

		transfer.Complete();

		await users.Save(sender);
		await users.Save(receiver);

		await transfers.Save(transfer);
		return transfer;
	}
}
