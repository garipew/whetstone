using ReliableTransfer.Domain;
using Npgsql;
using Dapper;

namespace ReliableTransfer.Infra;

public class TransferRepository
{
	private readonly string _connectionString;

	public TransferRepository(IConfiguration config)
	{
		_connectionString = config.GetConnectionString("TransferConnection");
	}

	public async void Create(Transfer t)
	{
		await using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			INSERT INTO Transfers
			VALUES (@Id, @Amount, @SenderId, @ReceiverId)
			""";
		var rowsAffected = conn.Execute(sql, t);
	}
}
