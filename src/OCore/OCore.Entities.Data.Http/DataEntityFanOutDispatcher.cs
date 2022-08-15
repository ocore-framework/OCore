namespace OCore.Entities.Data.Http
{
    /// <summary>
    /// A dispatcher that looks for the "," character in the DataEntity-name.
    /// </summary>
    public class DataEntityFanOutDispatcher : DataEntityDispatcher
    {
        public DataEntityFanOutDispatcher(string prefix, string dataEntityName, KeyStrategy keyStrategy, int maxFanOutLimit) : base(prefix, dataEntityName, keyStrategy, maxFanOutLimit)
        {
        }
    }
}
