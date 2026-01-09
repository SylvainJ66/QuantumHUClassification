using System.Data;

namespace SharedKernel.Infrastructure;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}
