using System;
using Pienetes.App.Domain.Events;

namespace Pienetes.App.Domain.Model
{
    public class QueuedManifest : AggregateRoot<QueuedManifestId>
    {
        private readonly string _content;
        private readonly string _contentType;
        private readonly DateTime _queuedTimestamp;
        private bool _isProcessed;

        private QueuedManifest(QueuedManifestId id, string content, string contentType, DateTime queuedTimestamp, 
            bool isProcessed) : base(id)
        {
            _content = content;
            _contentType = contentType;
            _queuedTimestamp = queuedTimestamp;
            _isProcessed = isProcessed;
        }

        public string Content => _content;
        public string ContentType => _contentType;
        public DateTime QueuedTimestamp => _queuedTimestamp;
        public bool IsProcessed => _isProcessed;

        public void MarkAsProcessed() => _isProcessed = true;

        public static QueuedManifest Create(string content, string contentType)
        {
            var instance =  new QueuedManifest(
                id: QueuedManifestId.Create(),
                content: content,
                contentType: contentType,
                queuedTimestamp: DateTime.Now,
                isProcessed: false
            );
            
            instance.Raise(new NewManifestHasBeenQueued(instance.Id));
            
            return instance;
        }
    }
}