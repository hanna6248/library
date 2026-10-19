
using library.Models;
using Npgsql;

namespace library.Data;

public class NoteRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public NoteRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<Note>> GetAllAsync()
    {
        var notes = new List<Note>();

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        const string sql = """
            SELECT note_id, title, body, created_on
            FROM lending.note
            ORDER BY note_id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notes.Add(new Note
            {
                NoteId = reader.GetInt64(0),
                Title = reader.GetString(1),
                Body = reader.IsDBNull(2) ? null : reader.GetString(2),
                CreatedOn = reader.GetDateTime(3)
            });
        }

        return notes;
    }

    public async Task<Note?> GetByIdAsync(long id)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        const string sql = """
            SELECT note_id, title, body, created_on
            FROM lending.note
            WHERE note_id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Note
        {
            NoteId = reader.GetInt64(0),
            Title = reader.GetString(1),
            Body = reader.IsDBNull(2) ? null : reader.GetString(2),
            CreatedOn = reader.GetDateTime(3)
        };
    }

    public async Task AddAsync(Note note)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        const string sql = """
            INSERT INTO lending.note (title, body)
            VALUES (@title, @body);
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("title", note.Title);
        command.Parameters.AddWithValue(
            "body", (object?)note.Body ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Note note)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        const string sql = """
            UPDATE lending.note
            SET title = @title, body = @body
            WHERE note_id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", note.NoteId);
        command.Parameters.AddWithValue("title", note.Title);
        command.Parameters.AddWithValue(
            "body", (object?)note.Body ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        const string sql = """
            DELETE FROM lending.note
            WHERE note_id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}
