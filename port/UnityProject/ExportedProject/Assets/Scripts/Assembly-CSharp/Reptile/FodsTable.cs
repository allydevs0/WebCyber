using System.Collections.Generic;
using System.Xml;

namespace Reptile
{
	public class FodsTable
	{
		public const string FODS_XML_TABLE_NODE_NAME = "table:table";

		public const string FODS_XML_TABLE_NAME_ATTRIBUTE_NAME = "table:name";

		private const string FODS_XML_ROW_NODE_NAME = "table:table-row";

		private const string FODS_XML_CELL_NODE_NAME = "table:table-cell";

		private const string FODS_XML_CELL_TEXT_NODE_NAME = "text:p";

		private const string FODS_XML_COLUMN_REPEAT_ATTRIBUTE_NAME = "table:number-columns-repeated";

		private int amountOfRows;

		private XmlNode tableXmlNode;

		private XmlNode[] rowXmlNodes;

		public int AmountOfRows => amountOfRows;

		public FodsTable(XmlNode tableXmlNode)
		{
			this.tableXmlNode = tableXmlNode;
			rowXmlNodes = XmlHelper.FindAllXmlNodesByName(tableXmlNode, "table:table-row");
			amountOfRows = rowXmlNodes.Length;
		}

		public string[] GetAllCellTextValuesForRow(int rowIndex)
		{
			List<string> list = new List<string>();
			XmlNode[] allCellXmlNodesForRow = GetAllCellXmlNodesForRow(rowIndex);
			for (int i = 0; i < allCellXmlNodesForRow.Length; i++)
			{
				string[] collection = RetrieveTextValuesForCellXmlNode(allCellXmlNodesForRow[i]);
				list.AddRange(collection);
			}
			return list.ToArray();
		}

		private string[] RetrieveTextValuesForCellXmlNode(XmlNode cellXmlNode)
		{
			List<string> list = new List<string>();
			XmlNode[] array = XmlHelper.FindAllXmlNodesByNameRecursive(cellXmlNode, "text:p");
			if (array.Length != 0)
			{
				AddTextCellToList(list, array, cellXmlNode);
			}
			else
			{
				AddEmptyCellsToList(list, cellXmlNode);
			}
			return list.ToArray();
		}

		private void AddTextCellToList(List<string> cellTextValues, XmlNode[] textXmlNodes, XmlNode cellXmlNode)
		{
			int repeatedCellValueCount = GetRepeatedCellValueCount(cellXmlNode);
			for (int i = 0; i < textXmlNodes.Length; i++)
			{
				string[] collection = CreateRepeatedCellValueArray(textXmlNodes[i].InnerText, repeatedCellValueCount);
				cellTextValues.AddRange(collection);
			}
		}

		private void AddEmptyCellsToList(List<string> cellTextValues, XmlNode cellXmlNodeWithoutText)
		{
			int repeatedCellValueCount = GetRepeatedCellValueCount(cellXmlNodeWithoutText);
			string[] collection = CreateRepeatedCellValueArray(string.Empty, repeatedCellValueCount);
			cellTextValues.AddRange(collection);
		}

		private int GetRepeatedCellValueCount(XmlNode cellXmlNode)
		{
			XmlAttributeCollection attributes = cellXmlNode.Attributes;
			int result = 1;
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i].Name.Equals("table:number-columns-repeated"))
				{
					result = int.Parse(attributes[i].Value);
					break;
				}
			}
			return result;
		}

		private string[] CreateRepeatedCellValueArray(string cellValue, int amountOfRepeats)
		{
			string[] array = new string[amountOfRepeats];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = cellValue;
			}
			return array;
		}

		private XmlNode[] GetAllCellXmlNodesForRow(int rowIndex)
		{
			return XmlHelper.FindAllXmlNodesByNameRecursive(rowXmlNodes[rowIndex], "table:table-cell");
		}

		public string[] GetAllCellTextValuesForColumn(int columnIndex)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < amountOfRows; i++)
			{
				string textValueForCell = GetTextValueForCell(i, columnIndex);
				list.Add(textValueForCell);
			}
			return list.ToArray();
		}

		public string GetTextValueForCell(int rowIndex, int columnIndex)
		{
			return GetAllCellTextValuesForRow(rowIndex)[columnIndex];
		}
	}
}
