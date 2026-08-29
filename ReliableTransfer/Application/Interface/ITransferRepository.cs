using ReliableTransfer.Domain;

namespace ReliableTransfer.Application;

public interface ITransferRepository
{
	public void Add(Transfer transfer);
	public void Save(Transfer transfer);
}
