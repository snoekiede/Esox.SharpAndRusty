using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Samples.Basics.Models;
using Esox.SharpAndRusty.Samples.Basics.Repositories;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Samples.Basics;

public class Program
{
    static void Main(string[] args)
    {
        StandardResultDemo();
        StandardUserRepositoryDemo();
        SharpResultDemo();
        UserRepositoryBindExample();
        UserRepositoryMapExample();
    }

    private static string printTitle(string title)
    {
        var line = new string('-', title.Length);
        var result = $"{line}\n{title}\n{line}";
        Console.WriteLine(result);
        return result;
    }
    internal static void UserRepositoryMapExample()
    {
        printTitle("User Repository Map");
        var repo = new SharpUserRepository();

        var result = repo.AddUser(new User { Id = 1, Name = "John Doe",Email="johndoe@example.com"});
        result.Match(
            success => Console.WriteLine($"Successfully added user"),
            failure => Console.WriteLine($"Failure: {failure}")
        );
        
        var mapResult= repo.GetUserById(1)
            .Map(u => u.Email!.ToUpper());
        mapResult.Match(
            success => Console.WriteLine($"Mapped email: {success}"),
            failure => Console.WriteLine($"Failure: {failure}")
        );
        
    }
    internal static void UserRepositoryBindExample()
    {
        printTitle("User Repository Bind");
        var repo = new SharpUserRepository();

        var result = repo.AddUser(new User { Id = 1, Name = "John Doe",Email="johndoe@example.com" });
        result.Match(
            success => Console.WriteLine($"Successfully added user"),
            failure => Console.WriteLine($"Failure: {failure}")
        );

        result = repo.AddUser(new User { Id = 2, Name = "Jane Doe",Email="janedoe@example.com" });
        result.Match(
            success => Console.WriteLine($"Successfully added user"),
            failure => Console.WriteLine($"Failure: {failure}")
        );

        foreach (var id in new[] { 1, 2, 3 })
        {
            repo.GetUserById(id)
                .Bind(GetMail)
                .Match(
                    success => Console.WriteLine($"Found email: {success}"),
                    failure => Console.WriteLine($"Failure: {failure}")
                );
        }
    }

    internal static Result<string, Error> GetMail(User u)
    {
        if (u is null)
        {
            return Error.New("User is null");
        }
        else
        {
            return u.Email!;
        }
    }
    internal static void StandardUserRepositoryDemo()
    {
        printTitle("Standard User Repository Demo");

        var repo = new UserRepository();

        repo.AddUser(new User { Id=1,Name = "John Doe" });
        repo.AddUser(new User { Id=2,Name = "Jane Doe" });

        //here we get a real user back
        var userResult = repo.GetUserByName("John Doe");
        System.Console.WriteLine(userResult.ToString());

        // here we get a non existent user back and we get an exception, so we have to catch it
        try {
            var userResult2 = repo.GetUserByName("Non Existent User");
            System.Console.WriteLine(userResult2.ToString());
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Exception: {ex.Message}");
        }

    }

    internal static void SharpResultDemo()
    {
        printTitle("Sharp Result Demo");
        var repo = new SharpUserRepository();

        var result=repo.AddUser(new User { Id = 1, Name = "John Doe" });
        result.Match(
            success => Console.WriteLine($"Successfully added user"),
            failure => Console.WriteLine($"Failure: {failure}")
        );
        
        result=repo.AddUser(new User { Id = 2, Name = "Jane Doe" });
        result.Match(
            success => Console.WriteLine($"Successfully added user"),
            failure => Console.WriteLine($"Failure: {failure}")
        );

        var searchResult = repo.GetUserByName("John Doe");
        searchResult.Match(
            success => Console.WriteLine($"Found user: {success}"),
            failure => Console.WriteLine($"Failure: {failure}")
        );
        // a result is returned here, forcing the developer to handle the error case, instead of throwing an exception
        searchResult = repo.GetUserByName("Non Existent User");
        searchResult.Match(
            success => Console.WriteLine($"Found user: {success}"),
            failure => Console.WriteLine($"Failure: {failure}")
        );
    }


    internal static void StandardResultDemo()
    {
        printTitle("Standard Result Demo");
        // standard result initialization
        var result = Result<int, Error>.Ok(42);

        // and match it
        result.Match(
            success => Console.WriteLine($"Success: {success}"),
            failure => Console.WriteLine($"Failure: {failure}")
        );

        // implict conversion
        result = 43;

        // and match it
        result.Match(
            success => Console.WriteLine($"Success: {success}"),
            failure => Console.WriteLine($"Failure: {failure}")
        );

        // this also works for errors
        result = Error.New("Wrong value");

        // and match it
        result.Match(
            success => Console.WriteLine($"Success: {success}"),
            failure => Console.WriteLine($"Failure: {failure}")
        );

        // back to an int
        result = 64;

        //use map to transform the value if it's a success
        var mathResult = result.Map(x => x * 2);

        mathResult.Match(
            success => Console.WriteLine($"Success: {success}"),
            failure => Console.WriteLine($"Failure: {failure}")
        );
        
        var otherMathResult=result.TryMap(x=>x/0);
        otherMathResult.Match(
            success => Console.WriteLine($"Success: {success}"),
            failure => Console.WriteLine($"Failure: {failure}")
        );
    }
}

