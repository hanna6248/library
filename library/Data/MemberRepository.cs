
using Npgsql;
using library.Models;

namespace library.Data;

public class MemberRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public MemberRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<Member>> GetAllAsync()
    {
        var members = new List<Member>();

        const string sql = """
            SELECT member_id, full_name, email, member_type
            FROM lending.member
            ORDER BY full_name;
            """;

        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            members.Add(new Member
            {
                MemberId = reader.GetInt64(0),
                FullName = reader.GetString(1),
                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                MemberType = reader.GetString(3)
            });
        }

        return members;
    }
}
