namespace NMG.Core.Domain
{
    /// <summary>
    /// Defines what primary keys are supported.
    /// </summary>
    public enum PrimaryKeyType
    {
        /// <summary>
        /// Primary key consisting of one column.
        /// </summary>
        PrimaryKey,
        /// <summary>
        /// Primary key consisting of two or more columns.
        /// </summary>
        CompositeKey,
        /// <summary>
        /// Default primary key type.
        /// </summary>
        Default = PrimaryKey
    }
}
