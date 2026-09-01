using System.Data.Common;

namespace ReliableTransfer.Domain;

public interface ITransferRepository
{
	public Task<Transfer?> GetByIdempotency(DbTransaction tx, Guid idempotency);
	public Task Add(DbTransaction tx, Guid idempotency, Transfer transfer);
	public Task Save(DbTransaction tx, Transfer transfer);
}
