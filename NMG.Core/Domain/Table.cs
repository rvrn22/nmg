using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NMG.Core.Annotations;

namespace NMG.Core.Domain
{
    /// <summary>
    /// Defines a database table entity.
    /// </summary>
    public class Table
    {
        public Table()
        {
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
            HasManyRelationships = new List<HasMany>();
        }

		public string Name { get; set; }
        public string Owner { get; set; }
    	public PrimaryKey PrimaryKey { get; set; }

    	public IList<ForeignKey> ForeignKeys { get; set; }
        public IList<Column> Columns { get; set; }
        public IList<HasMany> HasManyRelationships { get; set; }

    	public override string ToString() { return Name; }

		/// <summary>
		/// When one table has multiple fields that represent different relationships to the same foreign entity, it is required to give them unique names.
		/// </summary>
    	public static void SetUniqueNamesForForeignKeyProperties(IList<ForeignKey> foreignKeys)
		{
            // Create unique names foreign keys that access the same table more than once.
		    var groupedForeignKeys = (from fk in foreignKeys
		                              group fk by fk.References
		                              into g
		                              where g.Count() > 1
		                              select g).ToList();

		    foreach (var group in groupedForeignKeys)
		    {
		        foreach (var fk in group)
		        {
                    // Use the field name instead of the table name
		            fk.UniquePropertyName = fk.Columns.First().Name;
		        }
		    }
    	}
    }

    public class HasMany
    {
		public HasMany() {
			AllReferenceColumns = new List<string>();
		}

		/// <summary>
		/// An identifier for a constraint so that we might detect from querying the database whether a relationship has one is a composite key.
		/// </summary>
		public string ConstraintName { get; set; }
		public string Reference { get; set; }

		/// <summary>
		/// In support of relationships that use composite keys.
		/// </summary>
		public IList<string> AllReferenceColumns { get; set; }

		/// <summary>
		/// Provide the first (and very often the only) column used to define a foreign key relationship.
		/// </summary>
		public string ReferenceColumn { 
			get { return AllReferenceColumns.Count > 0 ? AllReferenceColumns[0] : ""; }
			set { AllReferenceColumns = new List<string>{value};   }   }
	}

    /// <summary>
    /// Defines a database column entity;
    /// </summary>
    public class Column: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsUnique { get; set; }
        public bool IsIdentity { get; set; }
        public string DataType { get; set; }
        public int? DataLength { get; set; }
        public string MappedDataType { get; set; }
        public bool IsNullable { get; set; }
		public string ConstraintName { get; set; }
        public int? DataPrecision { get; set; }
        public int? DataScale { get; set; }
        public string ForeignKeyTableName { get; set; }
        public string ForeignKeyColumnName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface IPrimaryKey
    {
        PrimaryKeyType KeyType { get; }
        IList<Column> Columns { get; set; }
    }

    public abstract class AbstractPrimaryKey : IPrimaryKey
    {
        protected AbstractPrimaryKey()
        {
            Columns = new List<Column>();
        }

        #region IPrimaryKey Members

        public abstract PrimaryKeyType KeyType { get; }
        public IList<Column> Columns { get; set; }

        #endregion
    }

    /// <summary>
    /// Defines a primary key entity.
    /// </summary>
    public class PrimaryKey
    {
        public PrimaryKey()
        {
            Columns = new List<Column>();
        }

        public PrimaryKeyType Type { get; set; }
        public IList<Column> Columns { get; set; }
        public bool IsGeneratedBySequence { get; set; } // Oracle only.
        public bool IsSelfReferencing { get; set; }
    }

    /// <summary>
    /// Defines a composite key entity.
    /// </summary>
    public class CompositeKey : AbstractPrimaryKey
    {
        public override PrimaryKeyType KeyType
        {
            get { return PrimaryKeyType.CompositeKey; }
        }
    }

    /// <summary>
    /// Defines a foreign key entity.
    /// </summary>
    public class ForeignKey
    {
        /// <summary>
        /// Foreign key constraint name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// One or more columns linked to the foreign key (more than one being a composite fk)
        /// </summary>
        public IList<Column> Columns { get; set; }

        /// <summary>
        /// Defines what table the foreign key references.
        /// </summary>
        public string References { get; set; }

    	/// <summary>
		/// When one table has multiple fields that represent different relationships to the same foreign entity, it is required to give them unique names.
		/// </summary>
		public string UniquePropertyName { get; set; }

        public override string ToString() { return Name; }
    }
}