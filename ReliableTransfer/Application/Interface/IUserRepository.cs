using ReliableTransfer.Domain;

namespace ReliableTransfer.Application;

public interface IUserRepository
{
	public User Get(int id);
	public void Save(User user);
}
