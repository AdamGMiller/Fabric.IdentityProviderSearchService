using System.Collections.Generic;
using System.DirectoryServices;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class DirectoryEntryWrapper : IDirectoryEntry
    {
        private Dictionary<string, string> Properties { get; }
        public string SchemaClassName { get; }
        
        public DirectoryEntryWrapper(DirectoryEntry directoryEntry)
        {
            Properties = new Dictionary<string, string>();
            
            SchemaClassName = directoryEntry.SchemaClassName;

            foreach (var property in _propertiesToSet)
            {
                var directoryEntryProperty = directoryEntry.Properties[property];
                Properties.Add(directoryEntryProperty.PropertyName.ToLower(), ReadUserEntryProperty(directoryEntryProperty));
            }
            directoryEntry.Dispose();
        }

        public string FirstName => Properties[GivenNameString];
        public string LastName => Properties[SnString];
        public string MiddleName => Properties[MiddleNameString];
        public string SamAccountName => Properties[SamAccountNameString];
        public string Name => Properties[NameString];

        private string ReadUserEntryProperty(PropertyValueCollection propertyValueCollection)
        {
            return propertyValueCollection.Value?.ToString() ?? string.Empty;
        }

        private readonly IEnumerable<string> _propertiesToSet = new List<string>
        {
            GivenNameString,
            SnString,
            MiddleNameString,
            SamAccountNameString,
            NameString
        };

        private const string GivenNameString = "givenname";
        private const string SnString = "sn";
        private const string MiddleNameString = "middlename";
        private const string SamAccountNameString = "samaccountname";
        private const string NameString = "name";
    }

    public interface IDirectoryEntry
    {        
        string SchemaClassName { get; }
        string FirstName { get; }
        string LastName { get; }
        string MiddleName { get; }
        string SamAccountName { get; }
        string Name { get; }
    }
}