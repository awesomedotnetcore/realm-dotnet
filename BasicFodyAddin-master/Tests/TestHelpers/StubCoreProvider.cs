﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RealmIO;

namespace Tests.TestHelpers
{
    public class StubCoreProvider : ICoreProvider
    {
        public class Table
        {
            public Dictionary<string, Type> Columns = new Dictionary<string, Type>();
            public List<Dictionary<string, object>> Rows = new List<Dictionary<string, object>>(); 
        }

        public Dictionary<string, Table> Tables = new Dictionary<string, Table>();
        public List<Query> Queries { get; } = new List<Query>();

        public bool HasTable(string tableName)
        {
            return Tables.ContainsKey(tableName);
        }

        public void AddTable(string tableName)
        {
            Tables[tableName] = new Table();
        }

        public void AddColumnToTable(string tableName, string columnName, Type columnType)
        {
            Tables[tableName].Columns[columnName] = columnType;
        }

        public int InsertEmptyRow(string tableName)
        {
            Tables[tableName].Rows.Add(new Dictionary<string, object>());
            return Tables[tableName].Rows.Count - 1;
        }

        public T GetValue<T>(string tableName, int rowIndex, string propertyName)
        {
            return (T)Tables[tableName].Rows[rowIndex][propertyName];
        }

        public void SetValue<T>(string tableName, int rowIndex, string propertyName, T value)
        {
            Tables[tableName].Rows[rowIndex][propertyName] = value;
        }

        public ICoreQueryHandle CreateQuery(string tableName)
        {
            var q = new Query {TableName = tableName};
            Queries.Add(q);
            return q;
        }

        public void QueryEqual(ICoreQueryHandle queryHandle, string columnName, object value)
        {
            var q = (Query) queryHandle;
            q.Sequence.Add(new Query.SequenceElement { Name = "Equal" });
        }

        public IEnumerable ExecuteQuery(ICoreQueryHandle queryHandle, Type returnType)
        {
            var innerType = returnType.GenericTypeArguments[0];
            var result = Activator.CreateInstance(typeof (List<>).MakeGenericType(innerType));
            return (IEnumerable)result;
            //Oreturn (IQueryable)Activator.CreateInstance(typeof(RealmQuery<>).MakeGenericType(elementType), new object[] { this, expression });
        }

        // Non-interface helpers:

        public void AddBulk(string tableName, dynamic[] data)
        {
            if (!Tables.ContainsKey(tableName))
                Tables[tableName] = new Table();

            var table = Tables[tableName];

            foreach(var inputRow in data)
            {
                var row = new Dictionary<string, object>();

                foreach(var prop in inputRow.GetType().GetProperties())
                {
                    string propName = prop.Name;
                    object propValue = prop.GetValue(inputRow);
                    Type propType = prop.PropertyType;

                    //Debug.WriteLine(propType.Name + " " + propName + " = " + propValue);

                    if (!table.Columns.ContainsKey(propName))
                        table.Columns[propName] = propType;

                    row[propName] = propValue;
                }
                table.Rows.Add(row);
            }
        }

        public class Query : ICoreQueryHandle
        {
            public string TableName;

            public class SequenceElement
            {
                public string Name;
            }

            public List<SequenceElement> Sequence = new List<SequenceElement>();
        }
    }
}
