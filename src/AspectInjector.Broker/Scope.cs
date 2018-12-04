namespace AspectInjector.Broker
{
    /// <summary>
    /// Advice creation scope enumeration.
    /// </summary>
    public enum Scope
    {
        /// <summary>
        /// Aspect is created and used as singleton. Default value.
        /// </summary>
        Global = 1,

        /// <summary>
        /// Instance of an aspect is created per target class instance.
        /// </summary>
        PerInstance = 2
    }
}