using System;
using System.Collections.Generic;
using System.Linq;

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

		/// <summary>
		/// Given a column from this table, if it is a foreign key return the name of the table it references.
		/// </summary>
		public string ForeignKeyReferenceForColumn(Column column)
		{
			if (ForeignKeys != null) {
				var fKey = ForeignKeys.Where(fk => fk.Name == column.Name).FirstOrDefault();
				if (fKey != null) return fKey.References;
			}
			return String.Format("/* TODO: UNKNOWN FOREIGN ENTITY for column {0} */", column.Name);
		}

    	public override string ToString() { return Name; }

		/// <summary>
		/// When one table has multiple fields that represent different relationships to the same foreign entity, it is required to give them unique names.
		/// </summary>
    	public static void SetUniqueNamesForForeignKeyProperties(IEnumerable<ForeignKey> foreignKeys) {
    		var referencesUsedMoreThanOnce = foreignKeys.Select(f => f.References).Distinct()
    			.GroupJoin(foreignKeys, a=>a, b=>b.References, (a,b)=> new { References = a, Count = b.Count() } )
    			.Where(@t=>t.Count > 1)
    			.Select(@t=>t.References);

    		foreignKeys.Join(referencesUsedMoreThanOnce, a=>a.References, b=>b, (a,b)=>a).ToList()
    			.ForEach(fk=>
    			{
    				fk.UniquePropertyName = fk.Name + "_" + fk.References;
    			});
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
    public class Column
    {
        public string Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsUnique { get; set; }
        public string DataType { get; set; }
        public int? DataLength { get; set; }
        public string MappedDataType { get; set; }
        public bool IsNullable { get; set; }
		public string ConstraintName { get; set; }
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
        /// Foreign key column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines what table the foreign key references.
        /// </summary>
        public string References { get; set; }

    	private string _uniquePropertyName;

    	/// <summary>
		/// When one table has multiple fields that represent different relationships to the same foreign entity, it is required to give them unique names.
		/// </summary>
		public string UniquePropertyName
    	{
			get { return string.IsNullOrEmpty(_uniquePropertyName) ? References : _uniquePropertyName; }
    		set { _uniquePropertyName = value; }
    	}

		/// <summary>
		/// A foreign key may be one of multiple columns of a composite key to a foreign entity
		/// </summary>
		public string[] AllColumnsNamesForTheSameConstraint { get; set; }

    	public override string ToString() { return Name; }
    }
}