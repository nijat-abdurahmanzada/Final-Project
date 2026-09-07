using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopwatch.Stop();

    app.Logger.LogInformation(
        "{Method} {Path} returned {StatusCode} in {ElapsedMilliseconds} ms",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        stopwatch.ElapsedMilliseconds);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var users = new List<User>
{
    new User { Name = "Alice", Age = 30 },
    new User { Name = "Bob", Age = 25 },
    new User { Name = "Charlie", Age = 35 }
};

app.UseHttpsRedirection();

app.MapGet("/users", () => users);

app.MapGet("/users/{name}", (string name) =>
{
    var user = users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

app.MapPost("/users", (User newUser) =>
{
    var validationErrors = ValidateUser(newUser);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    users.Add(newUser);
    return Results.Created($"/users/{newUser.Name}", newUser);
});

app.MapPut("/users/{name}", (string name, User updatedUser) =>
{
    var validationErrors = ValidateUser(updatedUser);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var user = users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    if (user is null)
    {
        return Results.NotFound();
    }

    user.Age = updatedUser.Age;
    return Results.Ok(user);
});

app.MapDelete("/users/{name}", (string name) =>
{
    var user = users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    if (user is null)
    {
        return Results.NotFound();
    }

    users.Remove(user);
    return Results.NoContent();
});

app.Run();

static Dictionary<string, string[]> ValidateUser(User user)
{
    var validationContext = new ValidationContext(user);
    var validationResults = new List<ValidationResult>();
    Validator.TryValidateObject(user, validationContext, validationResults, validateAllProperties: true);

    return validationResults
        .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty), (result, memberName) => new
        {
            MemberName = memberName,
            ErrorMessage = result.ErrorMessage ?? "The value is invalid."
        })
        .GroupBy(error => error.MemberName)
        .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
}

public class User
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }
}