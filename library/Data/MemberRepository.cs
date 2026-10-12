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

    public async Task AddAsync(Member member)
    {
        const string sql = """
            INSERT INTO lending.member (full_name, email, member_type)
            VALUES (@full_name, @email, @member_type);
            """;

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue("full_name", member.FullName);

        command.Parameters.AddWithValue(
            "email",
            (object?)member.Email ?? DBNull.Value
        );

        command.Parameters.AddWithValue("member_type", member.MemberType);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Member?> GetByIdAsync(long id)
    {
        const string sql = """
        SELECT member_id, full_name, email, member_type
        FROM lending.member
        WHERE member_id = @id;
        """;

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Member
        {
            MemberId = reader.GetInt64(0),
            FullName = reader.GetString(1),
            Email = reader.IsDBNull(2) ? null : reader.GetString(2),
            MemberType = reader.GetString(3)
        };
    }
    public async Task UpdateAsync(Member member)
    {
        const string sql = """
        UPDATE lending.member
        SET full_name = @full_name,
            email = @email,
            member_type = @member_type
        WHERE member_id = @id;
        """;

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue("full_name", member.FullName);
        command.Parameters.AddWithValue(
            "email",
            (object?)member.Email ?? DBNull.Value
        );
        command.Parameters.AddWithValue("member_type", member.MemberType);
        command.Parameters.AddWithValue("id", member.MemberId);

        await command.ExecuteNonQueryAsync();
    }

}


