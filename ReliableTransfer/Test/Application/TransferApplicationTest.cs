using System;
using Npgsql;
using Testcontainers.PostgreSql;

using ReliableTransfer.Infra;
using ReliableTransfer.Domain;
using ReliableTransfer.Application;

namespace ReliableTransfer.Test;

public class TransferApplicationTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _db =
        new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

    private UserRepository users = null!;
    private TransferRepository transfers = null!;
    private TransferApplication sut = null!;

    public async Task InitializeAsync()
    {
	    await _db.StartAsync();

	    var sql = await File.ReadAllTextAsync(
			    Path.Combine(AppContext.BaseDirectory, "../../../../Infra/schema.pgsql"));

	    var connString = _db.GetConnectionString();
	    await using var conn = new NpgsqlConnection(connString);

	    await conn.OpenAsync();

	    await using var command = new NpgsqlCommand(sql, conn);
	    await command.ExecuteNonQueryAsync();

	    users = new UserRepository(connString);
	    transfers = new TransferRepository(connString);

	    sut = new TransferApplication(users, transfers);
    }

    public async Task DisposeAsync()
    {
	    await _db.DisposeAsync();
    }

    private async Task<User> CreateUser(decimal amount)
    {
	    var user = new User();
	    user.Credit(amount);
	    await users.Add(user);
	    return (await users.Get(user.Id))!;
    }

    [Fact]
    public async Task ProcessTransfer_ShouldNotReprocessCompleteTransfer()
    {
	var sender = await CreateUser(42m);
	var receiver = await CreateUser(42m);

	var idempotency = Guid.NewGuid();
	var t1 = await sut.ProcessTransfer(idempotency, sender.Id, receiver.Id, 21m);
	var t2 = await sut.ProcessTransfer(idempotency, sender.Id, receiver.Id, 21m);

	Assert.Equal(t1.Id, t2.Id);

	sender = (await users.Get(t1.SenderId))!;
	receiver = (await users.Get(t1.ReceiverId))!;

	Assert.Equal(21m, sender.Balance);
	Assert.Equal(63m, receiver.Balance);
    }

    [Fact]
    public async Task ProcessTransfer_ShouldProcessTransferOnlyOnce()
    {
	var sender = await CreateUser(42m);
	var receiver = await CreateUser(42m);

	var idempotency = Guid.NewGuid();
	var t1 = sut.ProcessTransfer(idempotency, sender.Id, receiver.Id, 21m);
	var t2 = sut.ProcessTransfer(idempotency, sender.Id, receiver.Id, 21m);

	var transfers = await Task.WhenAll(t1, t2);

	Assert.Equal(transfers[0].Id, transfers[1].Id);

	sender = (await users.Get(transfers[0].SenderId))!;
	receiver = (await users.Get(transfers[0].ReceiverId))!;

	Assert.Equal(21m, sender.Balance);
	Assert.Equal(63m, receiver.Balance);
    }
}

