using System.Data.Common;
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

	public async Task<Transfer> ProcessTransfer(DbConnection db, Guid idempotency, int senderId, int receiverId, decimal amount)
	{
		await using var tx = await db.BeginTransactionAsync();
		try {
			Transfer? transfer = await transfers.GetByIdempotency(tx, idempotency);
			if (transfer != null) {
				return transfer;
			}

			var sender = await users.Get(tx, senderId);
			var receiver = await users.Get(tx, receiverId);

			if (sender is null || receiver is null) {
				throw new InvalidOperationException("User not found");
			}

			transfer = new Transfer(senderId, receiverId, amount);
			if (!transfer.Validate()) {
				throw new InvalidOperationException("Transfer not valid");
			}
			await transfers.Add(tx, idempotency, transfer);

			sender.Debit(transfer.Amount);
			receiver.Credit(transfer.Amount);

			transfer.Complete();

			await users.Save(tx, sender);
			await users.Save(tx, receiver);

			await transfers.Save(tx, transfer);
			await tx.CommitAsync();
			return transfer;
		} catch (TransferConflictException) {
			await tx.RollbackAsync();
			await using var ttx = await db.BeginTransactionAsync();
			var existing = (await transfers.GetByIdempotency(ttx, idempotency))!;
			await ttx.CommitAsync();
			return existing;
		} catch {
			await tx.RollbackAsync();
			throw;
		}
	}
}
