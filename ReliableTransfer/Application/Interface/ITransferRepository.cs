using ReliableTransfer.Domain;

namespace ReliableTransfer.Application;

public interface ITransferRepository
{
	public void Add(Guid idempotency, Transfer transfer);
	public void Save(Transfer transfer);
	public Transfer? GetByIdempotency(Guid idempotency);
}
