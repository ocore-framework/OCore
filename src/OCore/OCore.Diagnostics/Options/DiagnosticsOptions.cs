namespace OCore.Diagnostics.Options
{
    public class DiagnosticsOptions
    {
        /// <summary>
        /// Whether or not to persist the contents of the CorrelationId recorder
        /// and registry to the backing store.
        /// </summary>
        public bool StoreCorrelationIdData { get; set; } = false;
        
        /// <summary>
        /// Whether or not to create a registry of correlation ids. NOTE!
        /// This will become a hot grain in any production system under any load.
        /// </summary>
        public bool StoreInCorrelationIdRegistry { get; set; } = true;

        /// <summary>
        /// How many, approximately, correlation ids to store in the registry.
        ///
        /// These are pruned on interval, so this is not a hard limit.
        /// </summary>
        public int MaxRegistryStoredCorrelationIds { get; set; } = 1000;
    }
}
