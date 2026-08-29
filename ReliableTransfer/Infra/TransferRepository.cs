using ReliableTransfer.Application;
using ReliableTransfer.Domain;
using Npgsql;
using Dapper;

namespace ReliableTransfer.Infra;

public class TransferRepository : ITransferRepository
{
	private readonly string _connectionString;

	public TransferRepository(string conString)
	{
		_connectionString = conString;
	}

	public void Add(Transfer t)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			INSERT INTO Transfers
			VALUES (@Id, @Amount, @SenderId, @ReceiverId)
			""";
		var rowsAffected = conn.Execute(sql, t);
	}

	public void Save(Transfer t)
	{
		using var conn = new NpgsqlConnection(_connectionString);

		const string sql = """
			UPDATE Transfers
			SET Amount = @Amount,
			SenderId = @SenderId,
			ReceiverId = @ReceiverId,
			Status = @Status
			WHERE Id = @Id
			""";
		var rowsAffected = conn.Execute(sql, t);
	}
}
