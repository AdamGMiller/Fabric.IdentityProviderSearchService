using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;

namespace Fabric.IdentityProviderSearchService.Models
{
    public class DirectoryEntryWrapper : IDirectoryEntry
    {
        private readonly DirectoryEntry _directoryEntry;

        public DirectoryEntryWrapper(DirectoryEntry directoryEntry)
        {
            Properties = new Dictionary<string, object>();
            _directoryEntry = directoryEntry;            

            foreach (PropertyValueCollection directoryEntryProperty in directoryEntry.Properties)
            {
                Properties.Add(directoryEntryProperty.PropertyName.ToLower(), ReadUserEntryProperty(directoryEntryProperty));
            }
            SchemaClassName = _directoryEntry.SchemaClassName;
        }

        public IDictionary Properties { get; }
        public string SchemaClassName { get; }

        private string ReadUserEntryProperty(PropertyValueCollection propertyValueCollection)
        {
            return propertyValueCollection.Value?.ToString() ?? string.Empty;
        }
    }

    public interface IDirectoryEntry
    {
        IDictionary Properties { get; }
        string SchemaClassName { get; }
    }
}