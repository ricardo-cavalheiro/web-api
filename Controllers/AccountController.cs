using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace Blog.Controllers;

[Route("v1/accounts")]
[ApiController]
public class AccountController : ControllerBase {
  [HttpPost]
  public async Task<IActionResult> CreateUser(
    [FromBody] RegisterViewModel model,
    [FromServices] EmailService emailService,
    [FromServices] BlogDataContext context
  ) {
    if (!ModelState.IsValid) {
      return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
    }

    var user = new User {
      Name = model.Name,
      Email = model.Email,
      Slug = model.Email.Replace("@", "-").Replace(".", "-")
    };

    var password = PasswordGenerator.Generate(25, true, false);

    user.PasswordHash = PasswordHasher.Hash(password);

    try {
      await context.Users.AddAsync(user);

      var emailSent = emailService.Send(user.Name, user.Email, "Bem vindo ao blog!", $"Sua senha é {password}");

      if (!emailSent) {
        throw new Exception();
      }

      await context.SaveChangesAsync();

      return Ok(new ResultViewModel<dynamic>(new { user = user.Email, password }));
    } catch (DbUpdateException) {
      return StatusCode(400, new ResultViewModel<string>("05X99 - Este e-mail já está cadastrado."));
    } catch {
      return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor."));
    }
  }

  [AllowAnonymous]
  [HttpPost("login")]
  public async Task<IActionResult> Login(
    [FromBody] LoginViewModel model,
    [FromServices] BlogDataContext context,
    [FromServices] TokenService tokenService
  ) {
    if (!ModelState.IsValid) {
      return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
    }

    var user = await context
      .Users
      .AsNoTracking()
      .Include(x => x.Roles)
      .FirstOrDefaultAsync(x => x.Email == model.Email);

    if (user == null) {
      return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido."));
    }

    if (!PasswordHasher.Verify(user.PasswordHash, model.Password)) {
      return StatusCode(401, "Usuário ou senha inválido.");
    }

    try {
      var token = tokenService.GenerateToken(user);

      return Ok(new ResultViewModel<string>(token));
    } catch {
      return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
    }
  }

  [Authorize]
  [HttpPost("upload-image")]
  public async Task<IActionResult> UploadImage(
    [FromBody] UploadImageViewModel model,
    [FromServices] BlogDataContext context
  ) {
    var fileName = $"{Guid.NewGuid()}.jpg";
    var data = new Regex(@"data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
    var bytes = Convert.FromBase64String(data);

    try {
      await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
    } catch (Exception) {
      return StatusCode(500, new ResultViewModel<string>("05X04 - Falaha interna no servidor."));
    }

    var user = await context
      .Users
      .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

    if (user == null) {
      return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));
    }

    user.Image = $"http://localhost:5186/images/{fileName}";

    try {
      context.Users.Update(user);
      await context.SaveChangesAsync();

      return Ok(new ResultViewModel<string>("Imagem alterada com sucesso"));
    } catch (Exception) {
      return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor."));
    }
  }
}
