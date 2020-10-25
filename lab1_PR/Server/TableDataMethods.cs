using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    // retrieve TableData with values
    static class TableDataMethods
    {
        // retrieve data with values for every item
        public static TableData GetTableData(JObject raw_object)
        {
            TableData values = null;
            var mime_type = raw_object["mime_type"];
            var data = raw_object["data"];

            if (mime_type == null)
            {
                return values = ObjArrayToTableData(data.ToString());
            }
            else if (mime_type.ToString() == "application/xml")
            {
                return values = GetXMLToTableData(data.ToString());
            }

            return null;
        }

        // retrieve names of the columns, after deserializing string to object
        private static TableData ObjArrayToTableData(string raw_string)
        {
            TableData values = new TableData();
            var array = (JArray)JsonConvert.DeserializeObject(raw_string);
            foreach (var item in array.Children<JObject>())
            {
                // temporary list for storing each value returned by foreach loop
                var temporary_item = new List<string>();
                foreach (var value in item)
                {
                    if (!values.columns.Contains(value.Key))
                    {
                        values.columns.Add(value.Key);
                    }
                    temporary_item.Add(value.Value.ToString());
                }
                values.data.Add(temporary_item);
            }

            return values;
        }

        private static TableData GetXMLToTableData(string raw_string)
        {
            TableData values = new TableData();
            XmlDocument xml_document = new XmlDocument();
            xml_document.LoadXml(raw_string);

            var nodes = xml_document.SelectSingleNode("dataset");
            foreach (XmlNode info in nodes.ChildNodes)
            {
                var temporary_item = new List<string>();
                foreach (XmlNode value in info.ChildNodes)
                {
                    if (!values.columns.Contains(value.Name))
                    {
                        values.columns.Add(value.Name);
                    }
                    temporary_item.Add(value.InnerText);
                }
                values.data.Add(temporary_item);
            }

            return values;
        }

        // Find the specific column by column name
        public static List<string> GetColumn(List<TableData> list, string column_name)
        {
            // the storing list 
            List<string> response = new List<string>();

            foreach (var table in list)
            {
                if (table != null)
                {
                    if (column_name != null)
                    {
                        // check if there is a column equivalent with string given by user
                        if (table.columns.Contains(column_name))
                        {
                            // get index of the specific column
                            var index = table.columns.IndexOf(column_name);

                            foreach (var info in table.data)
                            {
                                if (info.Count > index)
                                {
                                    response.Add(info[index]);
                                }
                            }
                        }
                    }
                }
            }
            if (response.Count == 0)
            {
                response.Add("This column doesn't exist.");
            }

            return response;
        }
    }
}