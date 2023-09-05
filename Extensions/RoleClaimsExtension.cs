using Blog.Models;
using System.Security.Claims;

namespace Blog.Extensions;

public static class RoleClaimsExtension {
  public static IEnumerable<Claim> GetClaims(this User user) {
    var results = new List<Claim> {
      new Claim(ClaimTypes.Name, user.Email)
    };

    results.AddRange(
      user.Roles.Select(
        role => new Claim(ClaimTypes.Role, role.Slug)
      )  
    );

    return results;
  }
}
