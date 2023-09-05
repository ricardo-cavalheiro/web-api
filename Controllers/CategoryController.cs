using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers;

[Route("v1/categories")]
[ApiController]
public class CategoryController : ControllerBase {
  [HttpGet]
  public async Task<IActionResult> GetAsync(
    [FromServices] BlogDataContext context,
    [FromServices] IMemoryCache cache
  ) {
    try {
      var categories = await cache.GetOrCreateAsync("CategoriesCache", async entry => {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        return GetCategories(context);
      });

      return Ok(new ResultViewModel<List<Category>>(categories));
    } catch {
      return StatusCode(500, new ResultViewModel<List<Category>>("05X04 - Falha interna no servidor"));
    }
  }

  private static List<Category> GetCategories(BlogDataContext context) {
    return context.Categories.ToList();
  }

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetByIdAsync(
    [FromRoute] int id,
    [FromServices] BlogDataContext context
  ) {
    try {
      var category = await context
                    .Categories
                    .FirstOrDefaultAsync(x => x.Id == id);

      if (category == null)
        return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

      return Ok(new ResultViewModel<Category>(category));
    } catch {
      return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
    }
  }

  [HttpPost]
  public async Task<IActionResult> PostAsync(
    [FromBody] EditorCategoryViewModel model,
    [FromServices] BlogDataContext context
  ) {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

    try {
      var category = new Category
      {
        Id = 0,
        Name = model.Name,
        Slug = model.Slug.ToLower(),
      };
      await context.Categories.AddAsync(category);
      await context.SaveChangesAsync();

      return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
    } catch (DbUpdateException ex) {
      return StatusCode(500, new ResultViewModel<Category>("05XE9 - Não foi possível incluir a categoria"));
    } catch {
      return StatusCode(500, new ResultViewModel<Category>("05X10 - Falha interna no servidor"));
    }
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> PutAsync(
    [FromRoute] int id,
    [FromBody] EditorCategoryViewModel model,
    [FromServices] BlogDataContext context
  ) {
    try {
      var category = await context
                    .Categories
                    .FirstOrDefaultAsync(x => x.Id == id);

      if (category == null)
        return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

      category.Name = model.Name;
      category.Slug = model.Slug;

      context.Categories.Update(category);
      await context.SaveChangesAsync();

      return Ok(new ResultViewModel<Category>(category));
    } catch (DbUpdateException ex) {
      return StatusCode(500, new ResultViewModel<Category>("05XE8 - Não foi possível alterar a categoria"));
    } catch (Exception ex) {
      return StatusCode(500, new ResultViewModel<Category>("05X11 - Falha interna no servidor"));
    }
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteAsync(
    [FromRoute] int id,
    [FromServices] BlogDataContext context
  ) {
    try {
      var category = 
        await context
         .Categories
         .FirstOrDefaultAsync(x => x.Id == id);

      if (category == null)
        return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

      context.Categories.Remove(category);
      await context.SaveChangesAsync();

      return Ok(new ResultViewModel<Category>(category));
    } catch (DbUpdateException ex) {
      return StatusCode(500, new ResultViewModel<Category>("05XE7 - Não foi possível excluir a categoria"));
    } catch (Exception ex) {
      return StatusCode(500, new ResultViewModel<Category>("05X12 - Falha interna no servidor"));
    }
  }
}
