namespace ProjectAPI.Models
{
    public class Users
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime LastVisited { get; set; }
        public int Active { get; set; }
        public string ShopID { get; set; }

    }
}
