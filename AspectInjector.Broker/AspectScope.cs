namespace AspectInjector.Broker
{
    public enum AspectScope
    {
        /// <summary>
        /// Instantiate aspect per instance.
        /// </summary>
        Instance = 0,

        /// <summary>
        /// Instantiate aspect per type. Aspects are disposed when application is shutting down.
        /// </summary>
        Type = 1
    }
}
