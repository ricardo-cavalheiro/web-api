namespace Blog;

public static class Configuration {
  public static string JwtKey = "ricardaoricardao";
  public static string ApiKeyName = "api_key";
  public static string ApiKey = "curso_api.HJKLDFD*&DFJHLKFD(/==fD%#$44";
  public static SmptConfiguration Smtp = new ();
}

public class SmptConfiguration {
  public string Host { get; set; }
  public int Port { get; set; }
  public string UserName { get; set; }
  public string Password { get; set; }
}
