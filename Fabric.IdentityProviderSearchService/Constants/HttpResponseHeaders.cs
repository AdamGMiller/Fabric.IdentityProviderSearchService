using System;

namespace Fabric.IdentityProviderSearchService.Constants
{
    public static class HttpResponseHeaders
    {
        public static Tuple<string, string>[] CorsHeaders { get; } =
        {
            new Tuple<string, string>("Access-Control-Allow-Origin", "*"),
            new Tuple<string, string>("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization"),
            new Tuple<string, string>("Access-Control-Allow-Methods", "POST, GET, PUT, DELETE")
        };
    }
}