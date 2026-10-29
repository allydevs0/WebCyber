using System.Collections.Generic;
using System.Xml;

namespace Reptile
{
	public static class XmlHelper
	{
		public static XmlNode FindXmlNodeByName(XmlNode xmlNodeToSearchIn, string xmlNodeName)
		{
			if (CompareXmlNodeByName(xmlNodeToSearchIn, xmlNodeName))
			{
				return xmlNodeToSearchIn;
			}
			foreach (XmlNode childNode in xmlNodeToSearchIn.ChildNodes)
			{
				if (CompareXmlNodeByName(childNode, xmlNodeName))
				{
					return childNode;
				}
			}
			return null;
		}

		public static XmlNode FindXmlNodeByNameWithAttributeValue(XmlNode startNode, string xmlNodeName, string xmlAttributeName, string xmlAttributeValue)
		{
			bool num = CompareXmlNodeByName(startNode, xmlNodeName);
			bool flag = false;
			if (num)
			{
				flag = HasAttributeWithValue(startNode, xmlAttributeName, xmlAttributeValue);
			}
			if (flag)
			{
				return startNode;
			}
			foreach (XmlNode childNode in startNode.ChildNodes)
			{
				XmlNode xmlNode = FindXmlNodeByNameWithAttributeValue(childNode, xmlNodeName, xmlAttributeName, xmlAttributeValue);
				if (xmlNode != null)
				{
					return xmlNode;
				}
			}
			return null;
		}

		public static XmlNode FindXmlNodeByNameRecursive(XmlNode startNode, string xmlNodeName)
		{
			if (CompareXmlNodeByName(startNode, xmlNodeName))
			{
				return startNode;
			}
			foreach (XmlNode childNode in startNode.ChildNodes)
			{
				XmlNode xmlNode = FindXmlNodeByNameRecursive(childNode, xmlNodeName);
				if (xmlNode != null)
				{
					return xmlNode;
				}
			}
			return null;
		}

		public static XmlNode[] FindAllXmlNodesByName(XmlNode xmlNodeToSearchIn, string xmlNodeName)
		{
			List<XmlNode> list = new List<XmlNode>();
			foreach (XmlNode childNode in xmlNodeToSearchIn.ChildNodes)
			{
				if (CompareXmlNodeByName(childNode, xmlNodeName))
				{
					list.Add(childNode);
				}
			}
			return list.ToArray();
		}

		public static XmlNode[] FindAllXmlNodesByNameRecursive(XmlNode startNode, string xmlNodeName)
		{
			List<XmlNode> list = new List<XmlNode>();
			if (CompareXmlNodeByName(startNode, xmlNodeName))
			{
				list.Add(startNode);
			}
			foreach (XmlNode childNode in startNode.ChildNodes)
			{
				XmlNode[] collection = FindAllXmlNodesByNameRecursive(childNode, xmlNodeName);
				list.AddRange(collection);
			}
			return list.ToArray();
		}

		public static bool CompareXmlNodeByName(XmlNode xmlNode, string xmlNodeName)
		{
			return xmlNode.Name.Equals(xmlNodeName);
		}

		public static XmlAttribute GetAttributeWithName(XmlNode xmlNode, string xmlAttributeName)
		{
			XmlAttributeCollection attributes = xmlNode.Attributes;
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i].Name.Equals(xmlAttributeName))
				{
					return attributes[i];
				}
			}
			return null;
		}

		public static bool HasAttributeWithValue(XmlNode xmlNode, string xmlAttributeName, string xmlAttributeValue)
		{
			return GetAttributeWithName(xmlNode, xmlAttributeName)?.Value.Equals(xmlAttributeValue) ?? false;
		}

		public static bool HasAttributeWithName(XmlNode xmlNode, string xmlAttributeName)
		{
			return GetAttributeWithName(xmlNode, xmlAttributeName) != null;
		}

		public static void PrintXmlNodeChildren(XmlNode xmlNode)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				PrintXmlNode(childNode);
			}
		}

		public static void PrintXmlNode(XmlNode xmlNode)
		{
		}
	}
}
