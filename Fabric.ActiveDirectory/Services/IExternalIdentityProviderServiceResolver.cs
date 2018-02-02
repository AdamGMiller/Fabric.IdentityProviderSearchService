namespace Fabric.ActiveDirectory.Services
{
    public interface IExternalIdentityProviderServiceResolver
    {
        IExternalIdentityProviderService GetExternalIdentityProviderService(string identityProviderName);
    }
}
