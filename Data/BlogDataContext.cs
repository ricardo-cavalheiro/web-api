using Blog.Data.Mappings;
using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data;

public class BlogDataContext : DbContext {
  public DbSet<User> Users { get; set; }
  public DbSet<Post> Posts { get; set; }
  public DbSet<Category> Categories { get; set; }

  public BlogDataContext(DbContextOptions<BlogDataContext> options) : base(options) {

  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
    optionsBuilder.UseSqlServer();
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.ApplyConfiguration(new CategoryMap());
    modelBuilder.ApplyConfiguration(new UserMap());
    modelBuilder.ApplyConfiguration(new PostMap());
  }
}