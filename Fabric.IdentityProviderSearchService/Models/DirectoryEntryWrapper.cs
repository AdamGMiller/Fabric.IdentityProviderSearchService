using System.Collections.Generic;
using System.DirectoryServices;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class DirectoryEntryWrapper : IDirectoryEntry
    {
        public DirectoryEntryWrapper(DirectoryEntry directoryEntry)
        {
            Properties = new Dictionary<string, string>();
            SchemaClassName = directoryEntry.SchemaClassName;

            foreach (var property in _propertiesToSet)
            {
                var directoryEntryProperty = directoryEntry.Properties[property];
                Properties.Add(directoryEntryProperty.PropertyName.ToLower(), ReadUserEntryProperty(directoryEntryProperty));
            }
        }

        public Dictionary<string, string> Properties { get; }
        public string SchemaClassName { get; }

        private string ReadUserEntryProperty(PropertyValueCollection propertyValueCollection)
        {
            return propertyValueCollection.Value?.ToString() ?? string.Empty;
        }

        private readonly IEnumerable<string> _propertiesToSet = new List<string>
        {
            "givenname",
            "sn",
            "middlename",
            "samaccountname",
            "name"
        };
    }

    public interface IDirectoryEntry
    {
        Dictionary<string,string> Properties { get; }
        string SchemaClassName { get; }
    }
}