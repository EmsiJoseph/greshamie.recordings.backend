namespace backend.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    private readonly IConfiguration _configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

