namespace outofoffice.Models
{
    public class ZohoFSMUserDetails
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
        public string scope { get; set; }
        public string scope_users { get; set; }
        public string scope_modules { get; set; }
    }
    public class ServiceResource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
