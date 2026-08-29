using ReliableTransfer.Domain;
using Npgsql;
using Dapper;

namespace ReliableTransfer.Infra;

public class UserRepository
{
	private readonly string _connectionString;

	public UserRepository(IConfiguration config)
	{
		_connectionString = config.GetConnectionString("TransferConnection");
	}

	public async Task<User> Get(Guid id)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			SELECT *
			FROM Users
			WHERE Id = @Id
			""";
		return conn.QuerySingle(sql, new { Id = id });
	}

	private async void Save(User user)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			UPDATE Users
			SET Balance = @Balance
			WHERE Id = @Id
			""";

		conn.Execute(sql, user);
	}
}
