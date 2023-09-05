using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers;

[Route("/v1/posts")]
[ApiController]
public class PostController : ControllerBase {
  [HttpGet]
  public async Task<IActionResult> GetAsync(
    [FromServices] BlogDataContext context,
    [FromQuery] int page = 0,
    [FromQuery] int pageSize = 25
  ) {
    try {
      var posts = await context
        .Posts
        .AsNoTracking()
        .Include(x => x.Category)
        .Include(x => x.Author)
        .Select(x => new ListPostsViewModel {
          Id = x.Id,
          Title = x.Title,
          Slug = x.Slug,
          LastUpdateDate = x.LastUpdateDate,
          Category = x.Category.Name,
          Author = $"{x.Author.Name} ({x.Author.Email})"
        })
        .Skip(page * pageSize)
        .Take(pageSize)
        .ToListAsync();

      return Ok(new ResultViewModel<dynamic>(new {
        page,
        pageSize,
        posts
      }));
    } catch {
      return StatusCode(500, "05X04 - Falha interna no servidor.");
    }
  }

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetByIdAsync(
    [FromServices] BlogDataContext context,
    [FromRoute] int id
  ) {
    try {
      var post = await context
        .Posts
        .AsNoTracking()
        .Include(x => x.Author)
        .ThenInclude(x => x.Roles)
        .Include(x => x.Category)
        .FirstOrDefaultAsync(x => x.Id == id);

      if (post == null) {
        return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado."));
      }

      return Ok(new ResultViewModel<Post>(post));
    } catch {
      return StatusCode(500, "05X04 - Falha interna no servidor");
    }
  }
}
