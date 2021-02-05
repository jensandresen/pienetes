using System;
using System.Collections.Generic;

namespace Pienetes.App.Domain.Model
{
    public class QueuedManifestId : ValueObject
    {
        private readonly Guid _value;

        private QueuedManifestId(Guid value)
        {
            _value = value;
        }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _value;
        }

        public override string ToString()
        {
            return _value.ToString("D");
        }
        
        public static implicit operator string(QueuedManifestId id)
        {
            return id.ToString();
        }

        public static implicit operator QueuedManifestId(string id)
        {
            return Parse(id);
        }

        public static QueuedManifestId Parse(string text)
        {
            var value = Guid.Parse(text);
            return new QueuedManifestId(value);
        }
        
        public static QueuedManifestId Create()
        {
            var value = Guid.NewGuid();
            return new QueuedManifestId(value);
        }
    }
}