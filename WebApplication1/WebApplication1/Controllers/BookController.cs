using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]

public class BookController : ControllerBase
{
    
    private readonly BookRepository _bookRepository;
    
    public BookController(BookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    
    [HttpGet]
    [Route("{id}/genres")]
    
    public async Task<IActionResult> GetBook(int id)
    {
        if (!await _bookRepository.DoesBookExist(id))
        {
            return NotFound($"Book with given id: {id} does not exist!");
        }

        var group = await _bookRepository.GetBook(id);

        return Ok(group);

    }

    [HttpPost]
    
    public async Task<IActionResult> AddBook(NewBookDTO newBookDto)
    {
        foreach (var genre in newBookDto.GenresList)
        {
            if (!await _bookRepository.DoesGenreExist(genre))
            {
                return NotFound($"Genre with given id: {genre} does not exist!");
            }
        }
        
        await _bookRepository.AddBook(newBookDto);
        
        return Created(Request.Path.Value ?? "api/books", newBookDto);
    }
}