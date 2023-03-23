using CQRS.Core.Events;

namespace CQRS.Core.Domain
{
    public abstract class AggregateRoot
    {
        protected Guid _id;
        private readonly List<BaseEvent> _changes = new();

        public Guid Id
        {
            get { return _id; }
        }

        public int Version { get; set; } = -1;

        // Used to get the uncommitted changes to the aggregate
        public IEnumerable<BaseEvent> GetUncommittedChanges()
        {
            return _changes;
        }

        // Used to mark changes as committed after they have been persisted
        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        // Used to apply events to the aggregate
        private void ApplyChanges(BaseEvent @event, bool isNew)
        {
            var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method), $"The Apply method was not found in the aggregate for {@event.GetType().Name}");
            }

            method.Invoke(this, new object[] { @event });

            if (isNew)
            {
                _changes.Add(@event);
            }
        }

        // Used to raise events from the aggregate
        protected void RaiseEvent(BaseEvent @event)
        {
            ApplyChanges(@event, true);
        }

        // Used to replay events when loading an aggregate from the event store
        public void ReplayEvents(IEnumerable<BaseEvent> events)
        {
            foreach (var @event in events)
            {
                ApplyChanges(@event, false);
            }
        }
    }
}