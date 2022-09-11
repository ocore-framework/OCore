using Orleans;
using System;

namespace OCore.Events
{
    [GenerateSerializer]
    public class Event<T>
    {
        [Id(0)]
        public Guid MessageId { get; set; }

        [Id(1)]
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// This message could not be handled
        /// </summary>
        [Id(2)]
        public bool IsPoisonous { get; set; }

        /// <summary>
        /// This will be populated from the EventCounter when the message is deemed poisonous
        /// </summary>
        [Id(3)]
        public int Retries { get; set; }

        [Id(4)]
        public T Payload { get; set; }
    }
}
