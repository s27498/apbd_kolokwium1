using Microsoft.Data.SqlClient;
using WebApplication1.DTOs;

namespace WebApplication1.Repositories;

public class BookRepository
{
// configuration    
    private readonly IConfiguration _configuration;

    public BookRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    
    public async Task<bool> DoesBookExist(int id)
    {
        var query = "Select 1 from Books where PK = @id";
// connection
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }
    public async Task<bool> DoesGenreExist(int id)
    {
        var query = "Select 1 from Books_genres where FK_genre = @id";
// connection
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }
    
    
    
    public async Task<BookDTO> GetBook(int id)
    {
        var query = "SELECT B.PK as id, B.title as title,G.name as genres" + 
        " from books B join books_genres BG on B.PK = BG.FK_book join genres G on BG.FK_genre = G.PK"+
        " where B.PK = @id";
// connection        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);
        
        connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var bookIdOrdinal = reader.GetOrdinal("id");
        var bookTitleOrdinal = reader.GetOrdinal("title");
        var bookGenreOrdinal = reader.GetOrdinal("genres");

        BookDTO bookDto = null;
        

        while (await reader.ReadAsync())
        {
            if (bookDto is not null)
            {
                bookDto.GenresList.Add( reader.GetString(bookGenreOrdinal));
            }
            else
                bookDto = new BookDTO()
                {
                    Id = reader.GetInt32(bookIdOrdinal),
                    Title = reader.GetString(bookTitleOrdinal),
                    GenresList = new List<string> { reader.GetString(bookGenreOrdinal)}

                };


        }

        if (bookDto is null) throw new Exception();
        {
            return bookDto;
        }

    }

    public async Task AddBook(NewBookDTO newBookDto)
    {
        var query = "insert into books values (@title); select @@identity as id";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@title", newBookDto.Title);
        
        connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        
        try
        {
            var id = await command.ExecuteScalarAsync();

            foreach (var genreId in newBookDto.GenresList)
            {
                command.Parameters.Clear();
                command.CommandText = "insert into books_genres values (@bookId,@genreId)";
                command.Parameters.AddWithValue("@bookId", id);
                command.Parameters.AddWithValue("@genreId", genreId);

                await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            
        } catch (Exception )
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

}