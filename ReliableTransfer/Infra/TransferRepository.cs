using System.Data.Common;
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

	public async Task<Transfer?> GetByIdempotency(DbTransaction tx, Guid idempotency)
	{
		var db = tx.Connection;
		const string sql = """
			SELECT Id, SenderId, ReceiverId, Amount
			FROM transfers
			WHERE IdempotencyKey = @Idempotency
			""";
		return await db.QueryFirstOrDefaultAsync<Transfer>(sql, new { Idempotency = idempotency }, tx);
	}

	public async Task Add(DbTransaction tx, Guid idempotency, Transfer t)
	{
		var db = tx.Connection;
		const string sql = """
			INSERT INTO transfers (IdempotencyKey, Amount, SenderId, ReceiverId, Status)
			VALUES (@IdempotencyKey, @Amount, @SenderId, @ReceiverId, @Status)
			RETURNING Id
			""";
		try {
			t.Id = await db.ExecuteScalarAsync<int>(sql, new {
					IdempotencyKey = idempotency,
					Amount = t.Amount,
					SenderId = t.SenderId,
					ReceiverId = t.ReceiverId,
					Status = t.Status
					}, tx);
		} catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation) {
			throw new TransferConflictException();
		}
	}

	public async Task Save(DbTransaction tx, Transfer t)
	{
		var db = tx.Connection;
		const string sql = """
			UPDATE transfers
			SET Amount = @Amount,
			SenderId = @SenderId,
			ReceiverId = @ReceiverId,
			Status = @Status
			WHERE Id = @Id
			""";

		await db.ExecuteAsync(sql, t, tx);
	}
}
