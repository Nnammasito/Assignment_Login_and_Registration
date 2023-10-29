#pragma warning disable CS8618
namespace Assignment_Login_and_Registration.Models;

public class MyViewModel
{
    public User User {get;set;} = new User();
    public Login Login {get;set;} = new Login();
}