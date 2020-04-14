using System;
using System.Collections.Generic;
using System.Data;
using IBM.Data.DB2;
using System.Linq;
using NMG.Core.Domain;


namespace NMG.Core.Reader
{

    public class InformixMetadataReader : IMetadataReader
    {

        readonly string _connectionStr;

        public InformixMetadataReader(string connectionStr)
        {
            _connectionStr = connectionStr;
        }


        public IList<string> GetOwners()
        {
            var owners = new List<string>();
            using (var sqlCon = new DB2Connection(_connectionStr))
            {
                sqlCon.Open();
                try
                {
                    using (DB2Command tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText =
                            "select distinct owner from systables where tabtype in ('T', 'E', 'V', 'S') and owner != 'informix' order by owner";
                        using (DB2DataReader reader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read())
                            {
                                owners.Add(reader.GetString("owner"));
                            }
                        }
                    }
                }
                finally
                {
                    sqlCon.Close();
                }
            }
            return owners;
        }


        public List<Table> GetTables(string owner)
        {
            var tables = new List<Table>();

            using (var sqlCon = new DB2Connection(_connectionStr))
            {
                sqlCon.Open();
                try
                {
                    using (DB2Command tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText =
                            $"select tabname, owner from systables where tabtype in ('T', 'E', 'V', 'S') and owner = '{owner}' order by tabname";
                        using (DB2DataReader reader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read())
                            {
                                tables.Add(new Table
                                {
                                    Name = reader.GetString("tabname"),
                                    Owner = reader.GetString("owner")
                                });
                            }
                        }
                    }
                }
                finally
                {
                    sqlCon.Close();
                }
            }

            return tables;
        }


        public IList<Column> GetTableDetails(Table table, string owner)
        {
            var columns = new List<Column>();

            using (var sqlCon = new DB2Connection(_connectionStr))
            {
                try
                {
                    sqlCon.Open();

                    using (DB2Command tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText = $@"
select c.colno, c.colname, c.coltype
     , cast(case when c.collength >= 256 then null else c.collength end as int) collength
     , cast(case when c.collength >= 256 then c.collength / 256 else null end as int) colprecision 
     , cast(case when c.collength >= 256 then c.collength - (cast(c.collength / 256 as int) * 256) else null end as int) colscale 
--     , cast(case when c.collength >= 256 then c.collength / 256 else c.collength end as int) collength
--     , cast(case when c.collength >= 256 then c.collength / 256 else c.collength end as int) colprecision 
--     , cast(case when c.collength >= 256 then c.collength - (cast(c.collength / 256 as int) * 256) else 0 end as int) colscale 
     , case when pkidx.idxtype is not null then 'P' else '-' end ispk
     , nvl(nl.constrtype, '-') isnullable
     , (select nvl(min(uidx.idxtype), '-') uniq
         from sysindexes uidx
        where uidx.tabid = c.tabid and uidx.idxtype = 'U'
          and c.colno in (uidx.part1, uidx.part2, uidx.part3, uidx.part4, uidx.part5, uidx.part6, uidx.part7, uidx.part8,
                          uidx.part9, uidx.part10, uidx.part11, uidx.part12, uidx.part13, uidx.part14, uidx.part15, uidx.part16)
        ) isunique
     , case when c.coltype in (6, 18, 262, 274) then 'I' else '-' end isidentity
from syscolumns c
  inner join systables t on c.tabid = t.tabid
  left outer join syscoldepend d on d.tabid = c.tabid and d.colno = c.colno
  left outer join sysconstraints nl on nl.constrid = d.constrid and nl.constrtype = 'N'
  left outer join sysconstraints pk on pk.tabid = c.tabid and pk.constrtype = 'P'
  left outer join sysindexes pkidx on pkidx.tabid = c.tabid and pkidx.idxname = pk.idxname
                                  and c.colno in (pkidx.part1, pkidx.part2, pkidx.part3, pkidx.part4, pkidx.part5, pkidx.part6, pkidx.part7, pkidx.part8,
                                                  pkidx.part9, pkidx.part10, pkidx.part11, pkidx.part12, pkidx.part13, pkidx.part14, pkidx.part15, pkidx.part16)
where t.owner = '{owner}'
  and t.tabname = '{table.Name}'
";
                        using (DB2DataReader reader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            var m = new DataTypeMapper();

                            while (reader.Read())
                            {
                                string dataType = GetColumnType(reader.Get<short>("coltype"));
                                int? dataLength = reader.Get<int?>("collength");
                                int? dataPrecision = reader.Get<int?>("colprecision");
                                int? dataScale = reader.Get<int?>("colscale");

                                columns.Add(new Column
                                {
                                    Name = reader.GetString("colname"),
                                    DataType = dataType,
                                    DataLength = dataLength,
                                    DataPrecision = dataPrecision,
                                    DataScale = dataScale,
                                    IsNullable = reader.GetString("isnullable") != "N",
                                    IsPrimaryKey = reader.GetString("ispk") == "P",
                                    IsUnique = reader.GetString("isunique") == "U",
                                    IsIdentity = reader.GetString("isidentity") == "I",
                                    MappedDataType = m.MapFromDBType(ServerType.Informix, dataType, dataLength, dataPrecision, dataScale).ToString(),
                                });
                            }
                        }
                    }

                    table.Owner = owner;
                    table.Columns = columns;
                }
                finally
                {
                    sqlCon.Close();
                }
            }

            table.PrimaryKey = DeterminePrimaryKeys(table);
            table.ForeignKeys = DetermineForeignKeyReferences(table);
            table.HasManyRelationships = DetermineHasManyRelationships(table);

            return columns;
        }

        public List<string> GetSequences(string owner)
        {
            var sequences = new List<string>();
            /*
            using (var sqlCon = new DB2Connection(_connectionStr))
            {
                sqlCon.Open();
                try
                {
                    using (DB2Command tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText =
                            "SELECT distinct owner FROM syssequences WHERE tabtype in ('T', 'V') AND name not like 'sys%'";
                        using (DB2DataReader sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            while (sqlDataReader.Read())
                            {
                                sequences.Add(sqlDataReader.GetString(0));
                            }
                        }
                    }
                }
                finally
                {
                    sqlCon.Close();
                }
            }
            */
            return sequences;
        }

        public PrimaryKey DeterminePrimaryKeys(Table table)
        {
            PrimaryKey key = null;

            var primaryKeys = table.Columns.Where(x => x.IsPrimaryKey).ToList();
            if (primaryKeys.Count > 0)
            {
                key = new PrimaryKey
                {
                    Type = primaryKeys.Count == 1 ? PrimaryKeyType.PrimaryKey : PrimaryKeyType.CompositeKey,
                    Columns = primaryKeys
                };
            }

            return key;
        }

        public IList<ForeignKey> DetermineForeignKeyReferences(Table table)
        {
            var foreignKeys = new List<ForeignKey>();

            using (var sqlCon = new DB2Connection(_connectionStr))
            {
                try
                {
                    sqlCon.Open();

                    using (DB2Command tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText = $@"
select co.constrname, t.tabname, c.colno, c.colname, t2.tabname tabname2, c2.colno colno2, c2.colname colname2
  from sysreferences r
  inner join sysconstraints co on co.constrid = r.constrid and co.constrtype = 'R'
  inner join systables t on t.tabid = co.tabid
  inner join sysindexes i on i.idxname = co.idxname
  inner join syscolumns c on c.tabid = co.tabid

  inner join sysconstraints co2 on co2.constrid = r.primary
  inner join systables t2 on t2.tabid = co2.tabid
  inner join sysindexes i2 on i2.idxname = co2.idxname
  inner join syscolumns c2 on c2.tabid = co2.tabid
 
where (
       (c.colno = i.part1 and c2.colno = i2.part1)
    or (c.colno = i.part2 and c2.colno = i2.part2)
    or (c.colno = i.part3 and c2.colno = i2.part3)
    or (c.colno = i.part4 and c2.colno = i2.part4)
    or (c.colno = i.part5 and c2.colno = i2.part5)
    or (c.colno = i.part6 and c2.colno = i2.part6)
    or (c.colno = i.part7 and c2.colno = i2.part7)
    or (c.colno = i.part8 and c2.colno = i2.part8)
    or (c.colno = i.part9 and c2.colno = i2.part9)
    or (c.colno = i.part10 and c2.colno = i2.part10)
    or (c.colno = i.part11 and c2.colno = i2.part11)
    or (c.colno = i.part12 and c2.colno = i2.part12)
    or (c.colno = i.part13 and c2.colno = i2.part13)
    or (c.colno = i.part14 and c2.colno = i2.part14)
    or (c.colno = i.part15 and c2.colno = i2.part15)
    or (c.colno = i.part16 and c2.colno = i2.part16)
    )
  and t.owner = '{table.Owner}'
  and t.tabname = '{table.Name}'
";
                        using (DB2DataReader reader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            ForeignKey foreignKey = null;
                            while (reader.Read())
                            {
                                string constraintName = reader.GetString("constrname");
                                if (foreignKey != null && foreignKey.Name != constraintName)
                                {
                                    foreignKeys.Add(foreignKey);
                                    foreignKey = null;
                                }
                                if (foreignKey == null)
                                {
                                    foreignKey = new ForeignKey
                                    {
                                        Name = constraintName,
                                        References = reader.GetString("tabname2"),
                                        Columns = new List<Column>(),
                                        UniquePropertyName = null,
                                        IsNullable = true
                                    };
                                }

                                Column column = FindTableColumn(table, reader.GetString("colname"));
                                column.IsForeignKey = true;
                                column.ConstraintName = foreignKey.Name;
                                column.ForeignKeyTableName = foreignKey.References;
                                column.ForeignKeyColumnName = reader.GetString("colname2");
                                foreignKey.Columns.Add(column);
                            }

                            if (foreignKey != null)
                            {
                                foreignKeys.Add(foreignKey);
                            }
                        }
                    }
                }
                catch
                {
                    sqlCon.Close();
                    throw;
                }
            }

            Table.SetUniqueNamesForForeignKeyProperties(foreignKeys);

            return foreignKeys;
        }


        IList<HasMany> DetermineHasManyRelationships(Table table)
        {
            var hasManys = new List<HasMany>();

            using (var sqlCon = new DB2Connection(_connectionStr))
            {
                try
                {
                    sqlCon.Open();

                    using (DB2Command tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText = $@"
select co.constrname, t.tabname, c.colno, c.colname, t2.tabname tabname2, c2.colno colno2, c2.colname colname2
  from sysreferences r
  inner join sysconstraints co on co.constrid = r.constrid and co.constrtype = 'R'
  inner join systables t on t.tabid = co.tabid
  inner join sysindexes i on i.idxname = co.idxname
  inner join syscolumns c on c.tabid = co.tabid

  inner join sysconstraints co2 on co2.constrid = r.primary
  inner join systables t2 on t2.tabid = co2.tabid
  inner join sysindexes i2 on i2.idxname = co2.idxname
  inner join syscolumns c2 on c2.tabid = co2.tabid
 
where (
       (c.colno = i.part1 and c2.colno = i2.part1)
    or (c.colno = i.part2 and c2.colno = i2.part2)
    or (c.colno = i.part3 and c2.colno = i2.part3)
    or (c.colno = i.part4 and c2.colno = i2.part4)
    or (c.colno = i.part5 and c2.colno = i2.part5)
    or (c.colno = i.part6 and c2.colno = i2.part6)
    or (c.colno = i.part7 and c2.colno = i2.part7)
    or (c.colno = i.part8 and c2.colno = i2.part8)
    or (c.colno = i.part9 and c2.colno = i2.part9)
    or (c.colno = i.part10 and c2.colno = i2.part10)
    or (c.colno = i.part11 and c2.colno = i2.part11)
    or (c.colno = i.part12 and c2.colno = i2.part12)
    or (c.colno = i.part13 and c2.colno = i2.part13)
    or (c.colno = i.part14 and c2.colno = i2.part14)
    or (c.colno = i.part15 and c2.colno = i2.part15)
    or (c.colno = i.part16 and c2.colno = i2.part16)
    )
  and t2.owner = '{table.Owner}'
  and t2.tabname = '{table.Name}'
";
                        using (DB2DataReader reader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            HasMany hasMany = null;
                            while (reader.Read())
                            {
                                string constraintName = reader.GetString("constrname");
                                if (hasMany != null && hasMany.ConstraintName != constraintName)
                                {
                                    hasManys.Add(hasMany);
                                    hasMany = null;
                                }
                                if (hasMany == null)
                                {
                                    hasMany = new HasMany
                                    {
                                        ConstraintName = constraintName,
                                        Reference = reader.GetString("tabname2"),
                                        PKTableName = reader.GetString("tabname")
                                    };
                                }

                                hasMany.AllReferenceColumns.Add(reader.GetString("colname"));
                            }

                            if (hasMany != null)
                            {
                                hasManys.Add(hasMany);
                            }
                        }
                    }
                }
                catch
                {
                    sqlCon.Close();
                    throw;
                }
            }

            return hasManys;
        }


        Column FindTableColumn(Table table, string name)
        {
            return table.Columns.First(c => c.Name == name);
        }

        string GetColumnType(int columnType)
        {
            columnType &= 0xFF;
            if (Enum.IsDefined(typeof(ColumnType), columnType))
            {
                return ((ColumnType) columnType).ToString();
            }
            return columnType.ToString();
        }

        enum ColumnType
        {

            Char = 0,
            SmallInt = 1,
            Int = 2,
            Float = 3,
            SmallFloat = 4,
            Decimal = 5,
            Serial = 6,
            Date = 7,
            Money = 8,
            Null = 9,
            Datetime = 10,
            Byte = 11,
            Text = 12,
            Varchar = 13,
            Interval = 14,
            NChar = 15,
            NVarchar = 16,
            Long = 17,
            Serial8 = 18,
            Set = 19,
            MultiSet = 20,
            List = 21,
            Row = 22,
            Collection = 23,
            LVarchar = 40,
            Blob = 41, // blob, boolean, clob
            ClientLVarchar = 43,
            Boolean = 45,
            Bigint = 52,
            BigSerial = 53,
            IdsSecurityLabel = 2061,
            NamedRow = 4118

        }

        enum ColumnTypeMask
        {

            NotNull = 0x100,
            HostVariable = 0x200,
            FloatToDecimal = 0x400,
            Distinct = 0x800,
            NamedRow = 0x1000,
            DistinctLVarchar = 0x2000,
            DistinctBoolean = 0x4000,
            ClientCollection = 0x8000

        }

    }

    public static class ReaderExtension
    {

        public static T Get<T>(this IDataReader reader, string name)
        {
            object value = reader[name];
            if (value is DBNull)
            {
                return default(T);
            }
            return (T) value;
        }

        public static string GetString(this IDataReader reader, string name)
        {
            object value = reader[name];
            if (value is DBNull)
            {
                return null;
            }
            return ((string) value).Trim();
        }

    }

}