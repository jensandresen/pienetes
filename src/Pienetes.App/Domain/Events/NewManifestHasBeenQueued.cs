using System;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Domain.Events
{
    public class NewManifestHasBeenQueued : IDomainEvent
    {
        public NewManifestHasBeenQueued(QueuedManifestId manifestId)
        {
            ManifestId = manifestId ?? throw new ArgumentNullException(nameof(manifestId));
        }

        public QueuedManifestId ManifestId { get; }
    }
}