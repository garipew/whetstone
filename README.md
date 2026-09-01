# Whetstone

A collection of code snippets to hone back-end skills.

## Reliable Transfer

This section simulates a transfer application and explores how to ensure that an action happens only once even on multiple, concurrent tries.

On ReliableTransfer, I got familiar with SQL Transactions and Idempotency Keys.

The experiment was structured based on the Onion Architecture.

Behaviour is demonstrated in tests written with xUnit. Run the tests with:

```
cd ReliableTransfer
dotnet test
```

Dapper for the Repository Pattern implementation. EF Core primitives abstract away the need for both Repository Pattern and SQL Transactions.

For architecture purity, the next step would be to utilize the Unit Of Work Pattern to remove direct transaction creation and handling on the Application layer. For the sake of demonstration, it proves itself not needed.
