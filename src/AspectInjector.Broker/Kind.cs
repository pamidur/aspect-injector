namespace AspectInjector.Broker
{
    /// <summary>
    /// Advice method injection points enumeration.
    /// </summary>
    public enum Kind : byte
    {
        /// <summary>
        /// Advice method is called before target method.
        /// </summary>
        Before = 1,

        /// <summary>
        /// Advice method is called after target method.
        /// </summary>
        After = 2,

        /// <summary>
        /// Advice method is called instead of target method. Consider using <see cref="Source.Target" /> and <see cref="Source.Arguments" /> in order to make a subsequent call to target method.
        /// </summary>
        Around = 4
    }
}
