using Dapper;
using SharedKernel.Infrastructure;
using ExtractHUContext.ReadSide.Domain.Ports;
using ExtractHUContext.ReadSide.Domain.ReadModels;

namespace ExtractHUContext.ReadSide.Infrastructure.Queries;

public class SqlGetAllQuantumGreetingsQuery(IDbConnectionFactory connectionFactory)
    : IGetAllQuantumGreetingsQuery
{
    public async Task<IEnumerable<QuantumGreetingReadModel>> Execute()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();

        const string sql = @"
            SELECT
                ""Id"" AS Id,
                ""Message"" AS Message,
                ""CreatedAt"" AS CreatedAt
            FROM
                quantum_hu_context.""Quantum_Greetings""
            ORDER BY
                ""CreatedAt"" DESC";

        var greetings = await connection.QueryAsync<QuantumGreetingReadModel>(sql);

        return greetings;
    }
}
