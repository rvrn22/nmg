using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    public class MSAccessMetadataReader : IMetadataReader
    {
        [Flags]
        public enum OleDbColumnFlagsEnum : long
        {
            ISBOOKMARK = 0x1,

            MAYDEFER = 0x2,

            WRITE = 0x4,

            WRITEUNKNOWN = 0x8,

            ISFIXEDLENGTH = 0x10,

            ISNULLABLE = 0x20,

            MAYBENULL = 0x40,

            ISLONG = 0x80,

            ISROWID = 0x100,

            ISROWVER = 0x200,

            CACHEDEFERRED = 0x1000
        }

        /// <summary>
        ///     Reference at
        ///     https://stackoverflow.com/questions/21388962/columns-flags-field-values-for-columns-collection-of-ole-db-schema-collections
        /// </summary>
        private static readonly OleDbColumnFlagsEnum AutonumberColumnFlags = OleDbColumnFlagsEnum.MAYBENULL |
                                                                             OleDbColumnFlagsEnum.ISFIXEDLENGTH |
                                                                             OleDbColumnFlagsEnum.WRITEUNKNOWN |
                                                                             OleDbColumnFlagsEnum.MAYDEFER;

        private static readonly OleDbColumnFlagsEnum ShortTextFlags = OleDbColumnFlagsEnum.MAYBENULL |
                                                                      OleDbColumnFlagsEnum.WRITEUNKNOWN |
                                                                      OleDbColumnFlagsEnum.MAYDEFER;

        private static readonly OleDbColumnFlagsEnum LongTextFlags =
            OleDbColumnFlagsEnum.ISLONG | OleDbColumnFlagsEnum.MAYBENULL | OleDbColumnFlagsEnum.ISNULLABLE |
            OleDbColumnFlagsEnum.WRITEUNKNOWN |
            OleDbColumnFlagsEnum.MAYDEFER;

        private static readonly OleDbColumnFlagsEnum PictureFlags =
            OleDbColumnFlagsEnum.ISLONG | OleDbColumnFlagsEnum.MAYBENULL | OleDbColumnFlagsEnum.ISNULLABLE |
            OleDbColumnFlagsEnum.WRITEUNKNOWN |
            OleDbColumnFlagsEnum.MAYDEFER;

        private readonly string _connectionStr;
        private readonly Lazy<SchemaInfo> _lazySchemaInfo;

        public MSAccessMetadataReader(string connectionStr)
        {
            _connectionStr = connectionStr;
            _lazySchemaInfo = new Lazy<SchemaInfo>(GetSchemaInfo);
        }

        private object _mutex = new object();
        public IList<Column> GetTableDetails(Table table, string owner)
        {
            lock (_mutex)
            {
                var info = _lazySchemaInfo.Value;
                var accessColumns = info.Columns.Where(i => i.TABLE_NAME == table.Name).ToList();
                var accessIndexes = info.Indexes.Where(i => i.TABLE_NAME == table.Name).ToList();
                var constraintColumnUsage =
                    info.ConstraintColumnUsages.Where(i => i.TABLE_NAME == table.Name).ToList();
                var foreignKeys = info.ForeignKeys.Where(i => i.PK_TABLE_NAME == table.Name).ToList();
                var dataTypeMapper = new DataTypeMapper();
                var result = accessColumns
                    .Select(c => CreateColumn(c, accessIndexes, constraintColumnUsage, dataTypeMapper, info)).ToList();
                table.Owner = owner;
                table.Columns = result;
                table.PrimaryKey = DeterminePrimaryKeys(table);
                table.HasManyRelationships = DetermineHasManyRelationships(table, foreignKeys);
                table.ForeignKeys = DetermineForeignKeyReferences(table);
                return result;
            }
        }

        public IList<string> GetOwners()
        {
            return new List<string> { "master" };
        }

        public List<string> GetSequences(string owner)
        {
            return new List<string>();
        }

        public PrimaryKey DeterminePrimaryKeys(Table table)
        {
            var primaryKeys = table.Columns.Where(x => x.IsPrimaryKey).ToList();

            if (primaryKeys.Count() == 1)
            {
                var c = primaryKeys.First();
                var key = new PrimaryKey
                {
                    Type = PrimaryKeyType.PrimaryKey,
                    Columns = { c }
                };
                return key;
            }

            if (primaryKeys.Count() > 1)
            {
                var key = new PrimaryKey
                {
                    Type = PrimaryKeyType.CompositeKey,
                    Columns = primaryKeys
                };
                return key;
            }

            return null;
        }

        public IList<ForeignKey> DetermineForeignKeyReferences(Table table)
        {
            var foreignKeys = table.Columns.Where(x => x.IsForeignKey).Distinct()
                .Select(column => new ForeignKey
                {
                    Name = column.Name,
                    UniquePropertyName = column.Name,
                    References = column.ForeignKeyTableName,
                    Columns = DetermineColumnsForForeignKey(table.Columns, column.ConstraintName)
                }).ToList();

            Table.SetUniqueNamesForForeignKeyProperties(foreignKeys);

            return foreignKeys;
        }

        public List<Table> GetTables(string owner)
        {
            var info = _lazySchemaInfo.Value;
            return info.Tables.Where(i => i.TABLE_TYPE == "TABLE" || i.TABLE_TYPE == "PASS-THROUGH")
                .Select(i => new Table { Name = i.TABLE_NAME }).ToList();
        }

        /// <summary>
        ///     reference at
        ///     https://stackoverflow.com/questions/21388962/columns-flags-field-values-for-columns-collection-of-ole-db-schema-collections
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool IsAutonumber(OleDbColumn column)
        {
            return column.COLUMN_FLAGS == AutonumberColumnFlags && column.DATA_TYPE == OleDbType.Integer;
        }

        private SchemaInfo GetSchemaInfo()
        {
            SchemaInfo info;

            using (var connection = new OleDbConnection(_connectionStr))
            {
                try
                {
                    connection.Open();
                    var result = new SchemaInfo();

                    var restrictions = new object[] { null, null, null, null };
                    var columnsTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions);
                    var tablesTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, restrictions);
                    var indexesTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, restrictions);
                    var keyColumnUsageTable =
                        connection.GetOleDbSchemaTable(OleDbSchemaGuid.Key_Column_Usage, restrictions);
                    var foreignKeysTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, restrictions);
                    var constraintUsageColumnTable =
                        connection.GetOleDbSchemaTable(OleDbSchemaGuid.Constraint_Column_Usage, restrictions);
                    var proceduresTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Procedures, restrictions);
                    var tableConstraintsTable =
                        connection.GetOleDbSchemaTable(OleDbSchemaGuid.Table_Constraints, restrictions);
                    result.Tables = tablesTable.Rows.Cast<DataRow>().Select(i => new OleDbTable
                    {
                        TABLE_CATALOG = ConvertString(i, "TABLE_CATALOG"),
                        TABLE_SCHEMA = ConvertString(i, "TABLE_SCHEMA"),
                        TABLE_NAME = ConvertString(i, "TABLE_NAME"),
                        TABLE_TYPE = ConvertString(i, "TABLE_TYPE"),
                        TABLE_GUID = ConvertStruct<Guid>(i, "TABLE_GUID"),
                        DESCRIPTION = ConvertString(i, "DESCRIPTION"),
                        TABLE_PROPID = ConvertStruct<long>(i, "TABLE_PROPID"),
                        DATE_CREATED = ConvertStruct<DateTime>(i, "DATE_CREATED"),
                        DATE_MODIFIED = ConvertStruct<DateTime>(i, "DATE_MODIFIED")
                    }).OrderBy(i => i.TABLE_NAME).ToList();
                    result.Indexes = indexesTable.Rows.Cast<DataRow>().Select(i => new OleDbIndex
                    {
                        TABLE_CATALOG = ConvertString(i, "TABLE_CATALOG"),
                        TABLE_SCHEMA = ConvertString(i, "TABLE_SCHEMA"),
                        TABLE_NAME = ConvertString(i, "TABLE_NAME"),
                        INDEX_CATALOG = ConvertString(i, "INDEX_CATALOG"),
                        INDEX_SCHEMA = ConvertString(i, "INDEX_SCHEMA"),
                        INDEX_NAME = ConvertString(i, "INDEX_NAME"),
                        PRIMARY_KEY = ConvertStruct<bool>(i, "PRIMARY_KEY"),
                        UNIQUE = ConvertStruct<bool>(i, "UNIQUE"),
                        CLUSTERED = ConvertStruct<bool>(i, "CLUSTERED"),
                        TYPE = ConvertStruct<int>(i, "TYPE"),
                        FILL_FACTOR = ConvertStruct<int>(i, "FILL_FACTOR"),
                        INITIAL_SIZE = ConvertStruct<int>(i, "INITIAL_SIZE"),
                        NULLS = ConvertStruct<int>(i, "NULLS"),
                        SORT_BOOKMARKS = ConvertStruct<bool>(i, "SORT_BOOKMARKS"),
                        AUTO_UPDATE = ConvertStruct<bool>(i, "AUTO_UPDATE"),
                        NULL_COLLATION = ConvertStruct<int>(i, "NULL_COLLATION"),
                        ORDINAL_POSITION = ConvertStruct<long>(i, "ORDINAL_POSITION"),
                        COLUMN_NAME = ConvertString(i, "COLUMN_NAME"),
                        COLUMN_GUID = ConvertStruct<Guid>(i, "COLUMN_GUID"),
                        COLUMN_PROPID = ConvertStruct<long>(i, "COLUMN_PROPID"),
                        COLLATION = ConvertStruct<short>(i, "COLLATION"),
                        CARDINALITY = ConvertStruct<decimal>(i, "CARDINALITY"),
                        PAGES = ConvertStruct<int>(i, "PAGES"),
                        FILTER_CONDITION = ConvertString(i, "FILTER_CONDITION"),
                        INTEGRATED = ConvertStruct<bool>(i, "INTEGRATED")
                    }).OrderBy(i => i.INDEX_NAME).ThenBy(i => i.ORDINAL_POSITION).ToList();
                    result.KeyColumnUsages = keyColumnUsageTable.Rows.Cast<DataRow>().Select(i =>
                        new OleDbKeyColumnUsage
                        {
                            CONSTRAINT_CATALOG = ConvertString(i, "CONSTRAINT_CATALOG"),
                            CONSTRAINT_SCHEMA = ConvertString(i, "CONSTRAINT_SCHEMA"),
                            CONSTRAINT_NAME = ConvertString(i, "CONSTRAINT_NAME"),
                            TABLE_CATALOG = ConvertString(i, "TABLE_CATALOG"),
                            TABLE_SCHEMA = ConvertString(i, "TABLE_SCHEMA"),
                            TABLE_NAME = ConvertString(i, "TABLE_NAME"),
                            COLUMN_NAME = ConvertString(i, "COLUMN_NAME"),
                            COLUMN_GUID = ConvertStruct<Guid>(i, "COLUMN_GUID"),
                            COLUMN_PROPID = ConvertStruct<long>(i, "COLUMN_PROPID"),
                            ORDINAL_POSITION = ConvertStruct<long>(i, "ORDINAL_POSITION")
                        }).OrderBy(i => i.TABLE_NAME).ThenBy(i => i.ORDINAL_POSITION).ToList();
                    result.ForeignKeys = foreignKeysTable.Rows.Cast<DataRow>().Select(i => new OleDbForeignKey
                    {
                        PK_TABLE_CATALOG = ConvertString(i, "PK_TABLE_CATALOG"),
                        PK_TABLE_SCHEMA = ConvertString(i, "PK_TABLE_SCHEMA"),
                        PK_TABLE_NAME = ConvertString(i, "PK_TABLE_NAME"),
                        PK_COLUMN_NAME = ConvertString(i, "PK_COLUMN_NAME"),
                        PK_COLUMN_GUID = ConvertStruct<Guid>(i, "PK_COLUMN_GUID"),
                        PK_COLUMN_PROPID = ConvertStruct<long>(i, "PK_COLUMN_PROPID"),
                        FK_TABLE_CATALOG = ConvertString(i, "FK_TABLE_CATALOG"),
                        FK_TABLE_SCHEMA = ConvertString(i, "FK_TABLE_SCHEMA"),
                        FK_TABLE_NAME = ConvertString(i, "FK_TABLE_NAME"),
                        FK_COLUMN_NAME = ConvertString(i, "FK_COLUMN_NAME"),
                        FK_COLUMN_GUID = ConvertStruct<Guid>(i, "FK_COLUMN_GUID"),
                        FK_COLUMN_PROPID = ConvertStruct<long>(i, "FK_COLUMN_PROPID"),
                        ORDINAL = ConvertStruct<long>(i, "ORDINAL"),
                        UPDATE_RULE = ConvertString(i, "UPDATE_RULE"),
                        DELETE_RULE = ConvertString(i, "DELETE_RULE"),
                        PK_NAME = ConvertString(i, "PK_NAME"),
                        FK_NAME = ConvertString(i, "FK_NAME"),
                        DEFERRABILITY = ConvertStruct<short>(i, "DEFERRABILITY")
                    }).OrderBy(i => i.FK_NAME).ThenBy(i => i.ORDINAL).ToList();

                    result.ConstraintColumnUsages = constraintUsageColumnTable.Rows.Cast<DataRow>().Select(i =>
                        new OleDbConstraintColumnUsage
                        {
                            TABLE_CATALOG = ConvertString(i, "TABLE_CATALOG"),
                            TABLE_SCHEMA = ConvertString(i, "TABLE_SCHEMA"),
                            TABLE_NAME = ConvertString(i, "TABLE_NAME"),
                            COLUMN_NAME = ConvertString(i, "COLUMN_NAME"),
                            COLUMN_GUID = ConvertStruct<Guid>(i, "COLUMN_GUID"),
                            COLUMN_PROPID = ConvertStruct<long>(i, "COLUMN_PROPID"),
                            CONSTRAINT_CATALOG = ConvertString(i, "CONSTRAINT_CATALOG"),
                            CONSTRAINT_SCHEMA = ConvertString(i, "CONSTRAINT_SCHEMA"),
                            CONSTRAINT_NAME = ConvertString(i, "CONSTRAINT_NAME")
                        }).ToList();

                    result.Columns = columnsTable.Rows.Cast<DataRow>().Select(i =>
                    {
                        var columns = new OleDbColumn();
                        columns.TABLE_CATALOG = ConvertString(i, "TABLE_CATALOG");
                        columns.TABLE_SCHEMA = ConvertString(i, "TABLE_SCHEMA");
                        columns.TABLE_NAME = ConvertString(i, "TABLE_NAME");
                        columns.COLUMN_NAME = ConvertString(i, "COLUMN_NAME");
                        columns.COLUMN_GUID = ConvertStruct<Guid>(i, "COLUMN_GUID");
                        columns.COLUMN_PROPID = ConvertStruct<long>(i, "COLUMN_PROPID");
                        columns.ORDINAL_POSITION = ConvertStruct<long>(i, "ORDINAL_POSITION");
                        columns.COLUMN_HASDEFAULT = ConvertStruct<bool>(i, "COLUMN_HASDEFAULT");
                        columns.COLUMN_DEFAULT = ConvertString(i, "COLUMN_DEFAULT");
                        columns.COLUMN_FLAGS = ConvertStruct<OleDbColumnFlagsEnum>(i, "COLUMN_FLAGS");
                        columns.IS_NULLABLE = ConvertStruct<bool>(i, "IS_NULLABLE");
                        columns.DATA_TYPE = (OleDbType)i["DATA_TYPE"];
                        columns.TYPE_GUID = ConvertStruct<Guid>(i, "TYPE_GUID");
                        columns.CHARACTER_MAXIMUM_LENGTH = ConvertStruct<long>(i, "CHARACTER_MAXIMUM_LENGTH");
                        columns.CHARACTER_OCTET_LENGTH = ConvertStruct<long>(i, "CHARACTER_OCTET_LENGTH");
                        columns.NUMERIC_PRECISION = ConvertStruct<int>(i, "NUMERIC_PRECISION");
                        columns.NUMERIC_SCALE = ConvertStruct<short>(i, "NUMERIC_SCALE");
                        columns.DATETIME_PRECISION = ConvertStruct<long>(i, "DATETIME_PRECISION");
                        columns.CHARACTER_SET_CATALOG = ConvertString(i, "CHARACTER_SET_CATALOG");
                        columns.CHARACTER_SET_SCHEMA = ConvertString(i, "CHARACTER_SET_SCHEMA");
                        columns.CHARACTER_SET_NAME = ConvertString(i, "CHARACTER_SET_NAME");
                        columns.COLLATION_CATALOG = ConvertString(i, "COLLATION_CATALOG");
                        columns.COLLATION_SCHEMA = ConvertString(i, "COLLATION_SCHEMA");
                        columns.COLLATION_NAME = ConvertString(i, "COLLATION_NAME");
                        columns.DOMAIN_CATALOG = ConvertString(i, "DOMAIN_CATALOG");
                        columns.DOMAIN_SCHEMA = ConvertString(i, "DOMAIN_SCHEMA");
                        columns.DOMAIN_NAME = ConvertString(i, "DOMAIN_NAME");
                        columns.DESCRIPTION = ConvertString(i, "DESCRIPTION");
                        return columns;
                    }).OrderBy(i => i.TABLE_NAME).ThenBy(i => i.ORDINAL_POSITION).ToList();


                    result.Procedures = proceduresTable.Rows.Cast<DataRow>().Select(i => new OleDbProcedure
                    {
                        PROCEDURE_CATALOG = ConvertString(i, "PROCEDURE_CATALOG"),
                        PROCEDURE_SCHEMA = ConvertString(i, "PROCEDURE_SCHEMA"),
                        PROCEDURE_NAME = ConvertString(i, "PROCEDURE_NAME"),
                        PROCEDURE_TYPE = ConvertStruct<short>(i, "PROCEDURE_TYPE"),
                        PROCEDURE_DEFINITION = ConvertString(i, "PROCEDURE_DEFINITION"),
                        DESCRIPTION = ConvertString(i, "DESCRIPTION"),
                        DATE_CREATED = ConvertStruct<DateTime>(i, "DATE_CREATED"),
                        DATE_MODIFIED = ConvertStruct<DateTime>(i, "DATE_MODIFIED")
                    }).OrderBy(i => i.PROCEDURE_NAME).ToList();

                    result.TableConstraints = tableConstraintsTable.Rows.Cast<DataRow>().Select(i =>
                        new OleDbTableConstraint
                        {
                            CONSTRAINT_CATALOG = ConvertString(i, "CONSTRAINT_CATALOG"),
                            CONSTRAINT_SCHEMA = ConvertString(i, "CONSTRAINT_SCHEMA"),
                            CONSTRAINT_NAME = ConvertString(i, "CONSTRAINT_NAME"),
                            TABLE_CATALOG = ConvertString(i, "TABLE_CATALOG"),
                            TABLE_SCHEMA = ConvertString(i, "TABLE_SCHEMA"),
                            TABLE_NAME = ConvertString(i, "TABLE_NAME"),
                            CONSTRAINT_TYPE = ConvertString(i, "CONSTRAINT_TYPE"),
                            IS_DEFERRABLE = ConvertStruct<bool>(i, "IS_DEFERRABLE"),
                            INITIALLY_DEFERRED = ConvertStruct<bool>(i, "INITIALLY_DEFERRED"),
                            DESCRIPTION = ConvertString(i, "DESCRIPTION")
                        }).OrderBy(i => i.TABLE_NAME).ThenBy(i => i.CONSTRAINT_NAME).ToList();
                    info = result;
                }
                finally
                {
                    connection.Close();
                }
            }

            return info;
        }

        private Column CreateColumn(OleDbColumn oleDbColumn, List<OleDbIndex> oleDbIndexes,
            List<OleDbConstraintColumnUsage> constraintColumnUsages, DataTypeMapper dataTypeMapper,
            SchemaInfo schemaInfo)
        {
            var tableName = oleDbColumn.TABLE_NAME;
            var columnName = oleDbColumn.COLUMN_NAME;

            var primaryKeyConstraintColumnUsages = constraintColumnUsages.Where(i =>
                i.CONSTRAINT_NAME.Equals("PrimaryKey", StringComparison.InvariantCultureIgnoreCase)).ToList();
            var foreignKey = schemaInfo.ForeignKeys.SingleOrDefault(i =>
                i.FK_COLUMN_NAME == columnName && i.FK_TABLE_NAME == tableName);

            var column = new Column
            {
                Name = columnName,
                IsNullable = oleDbColumn.IS_NULLABLE ?? false,
                IsForeignKey = foreignKey != null,
                ConstraintName = foreignKey?.FK_NAME,
                ForeignKeyColumnName = foreignKey?.PK_COLUMN_NAME,
                ForeignKeyTableName = foreignKey?.PK_TABLE_NAME,
                IsIdentity = foreignKey == null && IsAutonumber(oleDbColumn) &&
                             primaryKeyConstraintColumnUsages.Count == 1 &&
                             primaryKeyConstraintColumnUsages.Any(i => i.COLUMN_NAME == columnName),
                IsPrimaryKey = oleDbIndexes.Any(i => i.COLUMN_NAME == columnName && i.PRIMARY_KEY == true)
                               || primaryKeyConstraintColumnUsages.Any(i => i.COLUMN_NAME == columnName),
                MappedDataType = dataTypeMapper.MapFromDBType(ServerType.MSAccess, oleDbColumn.DATA_TYPE.ToString(),
                    Convert.ToInt32(oleDbColumn.CHARACTER_MAXIMUM_LENGTH), oleDbColumn.NUMERIC_PRECISION,
                    oleDbColumn.NUMERIC_SCALE).ToString(),
                DataLength = Convert.ToInt32(oleDbColumn.CHARACTER_MAXIMUM_LENGTH),
                DataType = oleDbColumn.DATA_TYPE.ToString(),
                IsUnique = oleDbIndexes.GroupBy(i => i.INDEX_NAME).Where(i => i.Count() == 1)
                    .Any(i => i.Any(index =>
                        index.UNIQUE == true && index.PRIMARY_KEY == false &&
                        index.COLUMN_NAME == columnName))
            };

            return column;
        }


        private List<HasMany> DetermineHasManyRelationships(Table table,
            List<OleDbForeignKey> data)
        {
            return data.Where(i => i.PK_TABLE_NAME == table.Name).Select(i => new { i.FK_COLUMN_NAME, i.FK_TABLE_NAME })
                .Distinct().Select(i => new HasMany
                    { Reference = i.FK_TABLE_NAME, ReferenceColumn = i.FK_COLUMN_NAME }).ToList();
        }

        private string ConvertString(DataRow row, string columnName)
        {
            var tmp = row[columnName];
            if (tmp == DBNull.Value) return null;
            return tmp.ToString();
        }

        private T? ConvertStruct<T>(DataRow row, string columnName) where T : struct
        {
            var tmp = row[columnName];
            if (tmp == DBNull.Value) return null;
            return (T)tmp;
        }


        /// <summary>
        ///     Search for one or more columns that make up the foreign key.
        /// </summary>
        /// <param name="columns">All columns that could be used for the foreign key</param>
        /// <param name="foreignKeyName">Name of the foreign key constraint</param>
        /// <returns>List of columns associated with the foreign key</returns>
        /// <remarks>Composite foreign key will return multiple columns</remarks>
        private IList<Column> DetermineColumnsForForeignKey(IList<Column> columns, string foreignKeyName)
        {
            return (from c in columns
                where c.IsForeignKey && c.ConstraintName == foreignKeyName
                select c).ToList();
        }

        public class SchemaInfo
        {
            public List<OleDbIndex> Indexes { get; set; } = new List<OleDbIndex>();
            public List<OleDbTable> Tables { get; set; } = new List<OleDbTable>();
            public List<OleDbColumn> Columns { get; set; } = new List<OleDbColumn>();
            public List<OleDbKeyColumnUsage> KeyColumnUsages { get; set; } = new List<OleDbKeyColumnUsage>();
            public List<OleDbForeignKey> ForeignKeys { get; set; } = new List<OleDbForeignKey>();

            public List<OleDbConstraintColumnUsage> ConstraintColumnUsages { get; set; } =
                new List<OleDbConstraintColumnUsage>();

            public List<OleDbProcedure> Procedures { get; set; } = new List<OleDbProcedure>();
            public List<OleDbTableConstraint> TableConstraints { get; set; } = new List<OleDbTableConstraint>();
        }

        public class OleDbColumn
        {
            public string TABLE_CATALOG { get; set; }
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }
            public string COLUMN_NAME { get; set; }
            public Guid? COLUMN_GUID { get; set; }
            public long? COLUMN_PROPID { get; set; }
            public long? ORDINAL_POSITION { get; set; }
            public bool? COLUMN_HASDEFAULT { get; set; }
            public string COLUMN_DEFAULT { get; set; }
            public OleDbColumnFlagsEnum? COLUMN_FLAGS { get; set; }
            public bool? IS_NULLABLE { get; set; }
            public OleDbType DATA_TYPE { get; set; }
            public Guid? TYPE_GUID { get; set; }
            public long? CHARACTER_MAXIMUM_LENGTH { get; set; }
            public long? CHARACTER_OCTET_LENGTH { get; set; }
            public int? NUMERIC_PRECISION { get; set; }
            public short? NUMERIC_SCALE { get; set; }
            public long? DATETIME_PRECISION { get; set; }
            public string CHARACTER_SET_CATALOG { get; set; }
            public string CHARACTER_SET_SCHEMA { get; set; }
            public string CHARACTER_SET_NAME { get; set; }
            public string COLLATION_CATALOG { get; set; }
            public string COLLATION_SCHEMA { get; set; }
            public string COLLATION_NAME { get; set; }
            public string DOMAIN_CATALOG { get; set; }
            public string DOMAIN_SCHEMA { get; set; }
            public string DOMAIN_NAME { get; set; }
            public string DESCRIPTION { get; set; }
        }

        public class OleDbConstraintColumnUsage
        {
            public string TABLE_CATALOG { get; set; }
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }
            public string COLUMN_NAME { get; set; }
            public Guid? COLUMN_GUID { get; set; }
            public long? COLUMN_PROPID { get; set; }
            public string CONSTRAINT_CATALOG { get; set; }
            public string CONSTRAINT_SCHEMA { get; set; }
            public string CONSTRAINT_NAME { get; set; }
        }


        public class OleDbForeignKey
        {
            public string PK_TABLE_CATALOG { get; set; }
            public string PK_TABLE_SCHEMA { get; set; }
            public string PK_TABLE_NAME { get; set; }
            public string PK_COLUMN_NAME { get; set; }
            public Guid? PK_COLUMN_GUID { get; set; }
            public long? PK_COLUMN_PROPID { get; set; }
            public string FK_TABLE_CATALOG { get; set; }
            public string FK_TABLE_SCHEMA { get; set; }
            public string FK_TABLE_NAME { get; set; }
            public string FK_COLUMN_NAME { get; set; }
            public Guid? FK_COLUMN_GUID { get; set; }
            public long? FK_COLUMN_PROPID { get; set; }
            public long? ORDINAL { get; set; }
            public string UPDATE_RULE { get; set; }
            public string DELETE_RULE { get; set; }
            public string PK_NAME { get; set; }
            public string FK_NAME { get; set; }
            public short? DEFERRABILITY { get; set; }
        }

        public class OleDbIndex
        {
            public string TABLE_CATALOG { get; set; }
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }
            public string INDEX_CATALOG { get; set; }
            public string INDEX_SCHEMA { get; set; }
            public string INDEX_NAME { get; set; }
            public bool? PRIMARY_KEY { get; set; }
            public bool? UNIQUE { get; set; }
            public bool? CLUSTERED { get; set; }
            public int? TYPE { get; set; }
            public int? FILL_FACTOR { get; set; }
            public int? INITIAL_SIZE { get; set; }
            public int? NULLS { get; set; }
            public bool? SORT_BOOKMARKS { get; set; }
            public bool? AUTO_UPDATE { get; set; }
            public int? NULL_COLLATION { get; set; }
            public long? ORDINAL_POSITION { get; set; }
            public string COLUMN_NAME { get; set; }
            public Guid? COLUMN_GUID { get; set; }
            public long? COLUMN_PROPID { get; set; }
            public short? COLLATION { get; set; }
            public decimal? CARDINALITY { get; set; }
            public int? PAGES { get; set; }
            public string FILTER_CONDITION { get; set; }
            public bool? INTEGRATED { get; set; }
        }

        public class OleDbKeyColumnUsage
        {
            public string CONSTRAINT_CATALOG { get; set; }
            public string CONSTRAINT_SCHEMA { get; set; }
            public string CONSTRAINT_NAME { get; set; }
            public string TABLE_CATALOG { get; set; }
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }
            public string COLUMN_NAME { get; set; }
            public Guid? COLUMN_GUID { get; set; }
            public long? COLUMN_PROPID { get; set; }
            public long? ORDINAL_POSITION { get; set; }
        }


        public class OleDbProcedure
        {
            public string PROCEDURE_CATALOG { get; set; }
            public string PROCEDURE_SCHEMA { get; set; }
            public string PROCEDURE_NAME { get; set; }
            public short? PROCEDURE_TYPE { get; set; }
            public string PROCEDURE_DEFINITION { get; set; }
            public string DESCRIPTION { get; set; }
            public DateTime? DATE_CREATED { get; set; }
            public DateTime? DATE_MODIFIED { get; set; }
        }


        public class OleDbTableConstraint
        {
            public string CONSTRAINT_CATALOG { get; set; }
            public string CONSTRAINT_SCHEMA { get; set; }
            public string CONSTRAINT_NAME { get; set; }
            public string TABLE_CATALOG { get; set; }
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }
            public string CONSTRAINT_TYPE { get; set; }
            public bool? IS_DEFERRABLE { get; set; }
            public bool? INITIALLY_DEFERRED { get; set; }
            public string DESCRIPTION { get; set; }
        }

        public class OleDbTable
        {
            public string TABLE_CATALOG { get; set; }
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }
            public string TABLE_TYPE { get; set; }
            public Guid? TABLE_GUID { get; set; }
            public string DESCRIPTION { get; set; }
            public long? TABLE_PROPID { get; set; }
            public DateTime? DATE_CREATED { get; set; }
            public DateTime? DATE_MODIFIED { get; set; }
        }
    }
}