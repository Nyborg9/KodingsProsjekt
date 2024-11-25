namespace WebApplication2.Models
{
    // View model for the user list
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsUser { get; set; }
        public bool IsCaseworker { get; set; }
    }
}