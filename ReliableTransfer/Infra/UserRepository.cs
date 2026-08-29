using ReliableTransfer.Application;
using ReliableTransfer.Domain;
using Npgsql;
using Dapper;

namespace ReliableTransfer.Infra;

public class UserRepository : IUserRepository
{
	private readonly string _connectionString;

	public UserRepository(string conString)
	{
		_connectionString = conString;
	}

	public User Get(int id)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			SELECT *
			FROM Users
			WHERE Id = @Id
			""";
		return conn.QuerySingle(sql, new { Id = id });
	}

	public void Save(User user)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			UPDATE Users
			SET Balance = @Balance
			WHERE Id = @Id
			""";

		conn.Execute(sql, user);
	}
}
