using System.Xml;
#pragma warning disable

namespace NameToDisplayName
{
    public class Node
    {
        public string Name;
        public List<Node> Children;

        public Node Parent;

        public Node(string name, Node parent = null, params Node[] nodes)
        {
            Name = name;
            Parent = parent;
            Children = [.. nodes];
        }
    }

    public static class XmlHelpers
    {
        //places where condition can be
        public static HashSet<string> ConditionsContainingCalculators = new HashSet<string>
        {
            "Condition",
            "Formula",
            "TimeIndexFormula"
        };

        public static Node GetOrCreateNode(HashSet<Node> list, string name, Node parent)
        {
            Node workNode = list.Where(x=>x.Name == name).FirstOrDefault();
            if(workNode == null)
                workNode = new Node(name, parent);
            return workNode;   
        }

        public static HashSet<XmlNode> GetUsages(XmlDocument doc, string name)
        {
            var ret = new HashSet<XmlNode>();

                /*
                <AvxMimerDefinitionProperties>
            <DefinitionId>1086610</DefinitionId>
            <PropertyName>Condition</PropertyName>
            <LibraryId>1</LibraryId>
            <PropertyValue>(IsFirstReserve())  &amp;&amp;  (_Exists_(LastReserveStart()))  &amp;&amp;  (_Exists_("ReserveBlockBreaker()",FirstReserveEnd(),LastReserveStart()))</PropertyValue>
        </AvxMimerDefinitionProperties>
                */
            CollectNodes(doc.ChildNodes, ret, "AvxMimerDefinitionProperties","PropertyValue",name);

            return ret;
        }

        public static HashSet<XmlNode> GetAlternatives(XmlDocument doc, string name)
        {
            var id = GetIdForName(doc, name);
            //Console.WriteLine("Id: "+id);
/*
  <AvxMimerDefiniton>
    <DefinitionId>3604</DefinitionId>
    <LibraryId>1</LibraryId>
    <ParentId>3600</ParentId>
    <Name>MixedFlightCrewComplements:3</Name>
    <Kind>4</Kind>
  </AvxMimerDefiniton>
*/
            var ret = new HashSet<XmlNode>();
            CollectNodes(doc.ChildNodes, ret, "AvxMimerDefiniton","ParentId",id);
            return ret;
        }

        public static HashSet<string> GetUsagesNamesUniq(XmlDocument doc, string name, bool removeAlternatives = false)
        {
            var usages = GetUsages(doc, name);
            var uniq = new HashSet<string>();
            var usedNames = new HashSet<string>() { name };
            foreach(var node in usages)
            {
                var n = GetNameForId(doc,FindNode(node.ChildNodes, "DefinitionId")!.InnerText);
                if(removeAlternatives && n.IndexOf(":") > 0)
                     n = n.Remove(n.IndexOf(":"));

                if(usedNames.Contains(n))
                    continue;

                uniq.Add(n);
            }
            return uniq;
        }

        public static string GetNameForId(XmlDocument doc, string id)
        {
            var node = FindNode(doc.ChildNodes, "AvxMimerDefiniton","DefinitionId",id);
            if(node != null)
                return FindNode(node.ChildNodes, "Name")!.InnerText;
            return "";
        }

        public static string GetIdForName(XmlDocument doc, string name)
        {
            var node = FindNode(doc.ChildNodes, "AvxMimerDefiniton","Name",name);
            if(node != null)
                return FindNode(node.ChildNodes, "DefinitionId")!.InnerText;
            return "";
        }

        public static string GetXXXForId(XmlDocument doc, string propertyName, string id)
        {
            var node = FindNode(doc.ChildNodes, "AvxMimerDefinitionProperties"
            ,"DefinitionId",id
            ,"PropertyName",propertyName
            );
            if(node != null)
                return FindNode(node.ChildNodes, "PropertyValue")!.InnerText;
            return "";
        }

        
        public static void CollectNodes(XmlNodeList list, HashSet<XmlNode> collected, string? nodeName
        , string? subNodeName = null, string? subNodeValue = null
        , string? subNodeName2 = null, string? subNodeValue2 = null)
        {
            if(nodeName == null)
                return;

            if (list.Count > 0)
            {
                foreach (XmlNode node in list)
                {
                    if (node.Name.Equals(nodeName)) 
                    {
                        XmlNode? subNode = FindNode(node.ChildNodes, subNodeName);
                        XmlNode? subNode2 = FindNode(node.ChildNodes, subNodeName2);
                        if (subNode != null && (subNodeName2 == null || subNode2 != null ))
                        {
                            //if(subNode.InnerText.Contains(subNodeValue+"(")//can be a problem...
                            if(IsConditionContainsObject(subNode.InnerText, subNodeValue)
                                && (subNodeName2 == null || subNode2 != null
                                //&& subNode2.InnerText.Contains(subNodeValue2+"("))//can be a problem...
                                && IsConditionContainsObject(subNode2.InnerText, subNodeValue2))
                            )
                            {
                                if(!collected.Contains(node))
                                {
                                    collected.Add(node);
                                //debug
                                //Console.WriteLine("collected:"+subNode.InnerText+"/"+(subNode2 == null ? "" : subNode2.InnerText));
                                }
                            }
                        }
                    }

                    //go rec
                    if (node.HasChildNodes)
                    {
                        CollectNodes(node.ChildNodes, collected, nodeName, subNodeName, subNodeValue, subNodeName2, subNodeValue2);
                    }
                }
            }
        }

        public static XmlNode? FindNode(XmlNodeList list, string? nodeName
        , string? subNodeName = null, string? subNodeValue = null
        , string? subNodeName2 = null, string? subNodeValue2 = null)
        {
            if(nodeName == null)
                return null;

            if (list.Count > 0)
            {
                foreach (XmlNode node in list)
                {
                    /*
                    if(subNodeName != null)
                    {
                        if(subNodeName == node.Name)
                            Console.WriteLine("subnode:"+node.Name+" val:"+node.InnerText);
                    }
                    */
                    if (node.Name.Equals(nodeName)) 
                    {
                        if(subNodeName == null)
                            return node;

                        //Console.WriteLine("subnodes:"+node.ChildNodes.Count);

                        XmlNode? subNode = FindNode(node.ChildNodes, subNodeName);
                        XmlNode? subNode2 = FindNode(node.ChildNodes, subNodeName2);
                        if (subNode != null && (subNodeName2 == null || subNode2 != null ))
                        {
                            if(subNode.InnerText == subNodeValue
                                && (subNodeName2 == null || subNode2 != null && subNode2.InnerText == subNodeValue2)
                            )
                                return node;
                        }
                    }

                    //go rec
                    if (node.HasChildNodes)
                    {
                        XmlNode? nodeFound = FindNode(node.ChildNodes, nodeName, subNodeName, subNodeValue, subNodeName2, subNodeValue2);
                        if (nodeFound != null)
                            return nodeFound;
                    }
                }
            }
            return null;
        }

        public static HashSet<string> InnerFunctions = new HashSet<string>()
        {
            "AvxExpressionResultTimeSpan",
            "AvxExpressionResultDateTime",
            "FromTicks",

            "Date",
            "DateTime",
            "AddDays",
            "AddSeconds",
            "IntersectsWith",
            "NotIntersectsWith",
            "In",
            "NotIn",

            "_Sum_",
            "_AsText_",
            "_Exists_",

            "CustomSum",

            "true",
            "false",
            "True",
            "False",

            "0",
        };

        public static bool IsApplicableObject(string calc)
        {
            if(string.IsNullOrEmpty(calc))
                return false;
            if(calc.Trim().Length == 0)
                return false;

            return !InnerFunctions.Contains(calc);
        }

        public static HashSet<string> ExtractCalculatorsFromCondition(string condition)
        {
            HashSet<string> ret = new HashSet<string>();
            //(Activity_223_BLH_Sum() < new AvxExpressionResultTimeSpan(TimeSpan.FromTicks(3600000000000)))  &&  ("CA".IntersectsWith(_AssignedRank_()))
            
            //,Activity_223_BLH_Sum
            //,) < new AvxExpressionResultTimeSpan
            //,TimeSpan.FromTicks,3600000000000)))  &&  
            //,"CA".IntersectsWith
            //,_AssignedRank_
            //,)))
            string specChars = " &.,)\"!";
            foreach(var s in condition.Split("("))
            {
                //go from the last char and until some specific
                var ss = s.ToCharArray().Reverse();
                var news="";
                foreach(var c in ss)
                {
                    if(!specChars.Contains(c))
                        news+=c;
                    else break;
                }

                var toAdd = new string(news.ToCharArray().Reverse().ToArray());
                if(IsApplicableObject(toAdd))
                    ret.Add(toAdd);
            }
            specChars += "(";
            foreach(var s in condition.Split("\""))
            {
                //go from the last char and until some specific
                var ss = s.ToCharArray().Reverse();
                var news="";
                foreach(var c in ss)
                {
                    if(!specChars.Contains(c))
                        news+=c;
                    else break;
                }

                var toAdd = new string(news.ToCharArray().Reverse().ToArray());
                if(IsApplicableObject(toAdd))
                    ret.Add(toAdd);
            }

            return ret;
        }

        public static bool IsConditionContainsObject(string condition, string name)
        {
            var t = condition.Contains(name+"");
            if(t)
            {
                if(condition.Contains(Environment.NewLine))
                {
                    //solve it later
                    t = false;
                }
                else
                {
                    var hs = ExtractCalculatorsFromCondition(condition);  
                    //Console.WriteLine($"name: {name} condition: {condition} extracted: {hs}");
                    //Console.WriteLine($"{condition}");
                    return hs.Contains(name);
                }
            }
            return t;
        }

        public static void PrintTree(Node tree, int level = 0, int maxDeepLevel = int.MaxValue, bool stopIfShown = false, HashSet<string> shownNames = null, bool first = true, string indent = "", bool last = true)
        {
            string intendLast = "└╴";
            string intendMid = "├╴";
            string intendSkip = "│ ";
            string intendEmpty = "  ";
            bool skipIfShown = false;

            //temp
            /*
            skipIfShown = true;
            intendLast ="  ";
            intendMid ="  ";
            intendSkip ="  ";
            intendEmpty = "  ";
            */

            if(level>= maxDeepLevel && tree.Children.Count > 0)
            {
                string linked=$"->(it goes deeper than:{maxDeepLevel})";
                if(first)
                    Console.WriteLine(tree.Name+linked);
                else
                    Console.WriteLine(indent + (last ? intendLast : intendMid) + tree.Name+linked);

                return;
            }

            if(stopIfShown && tree.Children.Count > 0)
            {
                if(shownNames == null)
                    shownNames = new HashSet<string>();

                if(shownNames.Contains(tree.Name))
                {
                    if(skipIfShown)
                        return;

                    string linked="->...(already shown)";
                    if(first)
                        Console.WriteLine(tree.Name+linked);
                    else
                        Console.WriteLine(indent + (last ? intendLast : intendMid) + tree.Name+linked);

                    return;
                }
                shownNames.Add(tree.Name);
            }

            if(first)
                Console.WriteLine(tree.Name);
            else
                Console.WriteLine(indent + (last ? intendLast : intendMid) + tree.Name);

            indent = indent + (last ? intendEmpty : intendSkip);

            for (int i = 0; i < tree.Children.Count; i++)
                PrintTree(tree.Children[i], level+1, maxDeepLevel, stopIfShown, shownNames, false, indent, i == tree.Children.Count - 1);
        }
    }
}