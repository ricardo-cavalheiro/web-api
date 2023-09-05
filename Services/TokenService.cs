using Blog.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Blog.Extensions;

namespace Blog.Services;
public class TokenService {
  public string GenerateToken(User user) {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);
    var claims = user.GetClaims();
    var tokenDescription = new SecurityTokenDescriptor 
    { 
      Expires = DateTime.UtcNow.AddHours(8), 
      SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key), 
        SecurityAlgorithms.HmacSha256Signature
      ),
      Subject = new ClaimsIdentity(claims)
    };

    var token = tokenHandler.CreateToken(tokenDescription);

    return tokenHandler.WriteToken(token);
  }
}
