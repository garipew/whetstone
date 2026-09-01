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

		var sender = await users.Get(senderId);
		var receiver = await users.Get(receiverId);

		if (sender is null || receiver is null) {
			throw new InvalidOperationException("User not found");
		}

		try {
			transfer = new Transfer(senderId, receiverId, amount);
			if (!transfer.Validate()) {
				throw new InvalidOperationException("Transfer not valid");
			}
			await transfers.Add(idempotency, transfer);

			sender.Debit(transfer.Amount);
			receiver.Credit(transfer.Amount);

			transfer.Complete();

			await users.Save(sender);
			await users.Save(receiver);

			await transfers.Save(transfer);
			return transfer;
		} catch (TransferConflictException) {
			return (await transfers.GetByIdempotency(idempotency))!;
		}
	}
}
