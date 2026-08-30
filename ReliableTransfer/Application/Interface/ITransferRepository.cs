using ReliableTransfer.Domain;

namespace ReliableTransfer.Application;

public interface ITransferRepository
{
	public Task<Transfer?> GetByIdempotency(Guid idempotency);
	public Task Add(Guid idempotency, Transfer transfer);
	public Task Save(Transfer transfer);
}
