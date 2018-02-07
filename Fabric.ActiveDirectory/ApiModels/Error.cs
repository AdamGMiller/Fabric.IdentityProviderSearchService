namespace Fabric.ActiveDirectory.ApiModels
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Target { get; set; }
        public Error[] Details { get; set; }
    }
}