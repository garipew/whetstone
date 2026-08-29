using ReliableTransfer.Domain;

namespace ReliableTransfer.Application;

public interface IUserRepository
{
	public User Get(Guid id);
	public void Save(User user);
}
