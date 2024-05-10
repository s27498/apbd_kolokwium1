namespace WebApplication1.DTOs;

public class BookDTO
{
    public int Id { get; set; }
    public string Title { get; set; } 
    public List<String> GenresList { get; set; }
}