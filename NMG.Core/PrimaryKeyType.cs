using System.Collections.Generic;

namespace NMG.Core
{
    /// <summary>
    /// Defines what primary keys are allowed.
    /// </summary>
    public enum PrimaryKeyType
    {
        PrimaryKey,
        CompositeKey,
        Default = PrimaryKey
    }

    /// <summary>
    /// Defines a database table entity.
    /// </summary>
    public class Table
    {
        public string Name { get; set; }
        public IPrimaryKey PrimaryKey { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Defines a database column entity;
    /// </summary>
    public class Column
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
    }

    public interface IPrimaryKey
    {
        
    }

    public abstract class AbstractPrimaryKey : IPrimaryKey
    {
        public PrimaryKeyType KeyType { get; set; }
    }

    /// <summary>
    /// Defines a primary key entity.
    /// </summary>
    public class PrimaryKey : AbstractPrimaryKey
    {
        public Column KeyColumn { get; set; }
    }

    /// <summary>
    /// Defines a composite key entity.
    /// </summary>
    public class CompositeKey : AbstractPrimaryKey
    {
        public IList<Column> KeyColumns { get; set; }
    }

    /// <summary>
    /// Defines a foreign key entity.
    /// </summary>
    public class ForeignKey
    {
        /// <summary>
        /// Foreign key column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines what table the foreign key references.
        /// </summary>
        public string Reference { get; set; }
    }

    public class O
    {
        public IList<Table> tables;

        public O()
        {
            tables.Add(new Table { Name = "TableA" });
            tables.Add(new Table { Name = "TableB" });
            tables.Add(new Table { Name = "TableC" });
            tables.Add(new Table { Name = "TableD" });
        }
        public void F()
        {
            foreach(var table in tables)
            {
                var compositeKey = new CompositeKey
                {
                    KeyColumns =
                                {
                                    new Column
                                        {
                                            Name = "ColumnA",
                                            DataType = "string"
                                        }
                                }
                };
                table.PrimaryKey = compositeKey;
            }
        }
    }
}