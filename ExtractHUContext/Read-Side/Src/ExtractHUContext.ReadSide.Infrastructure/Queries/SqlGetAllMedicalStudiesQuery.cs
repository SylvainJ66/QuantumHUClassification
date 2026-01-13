using Dapper;
using SharedKernel.Infrastructure;
using ExtractHUContext.ReadSide.Domain.Ports;
using ExtractHUContext.ReadSide.Domain.ReadModels;

namespace ExtractHUContext.ReadSide.Infrastructure.Queries;

public class SqlGetAllMedicalStudiesQuery(IDbConnectionFactory connectionFactory)
    : IGetAllMedicalStudiesQuery
{
    public async Task<IEnumerable<MedicalStudyReadModel>> Execute()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();

        const string sql = @"
            SELECT
                ""Id"" AS Id,
                ""UploadDate"" AS UploadDate,
                ""FileName"" AS FileName,
                ""ContentType"" AS ContentType,
                ""FileSizeBytes"" AS FileSizeBytes,
                ""StorageKey"" AS StorageKey
            FROM
                quantum_hu_context.""Medical_Studies""
            ORDER BY
                ""UploadDate"" DESC";

        var studies = await connection.QueryAsync<MedicalStudyReadModel>(sql);

        return studies;
    }
}
