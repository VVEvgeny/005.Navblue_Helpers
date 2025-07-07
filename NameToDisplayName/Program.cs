using System.Text;
using System.Windows.Markup;
using System.Xml;
using NameToDisplayName;
#pragma warning disable
using xml = NameToDisplayName.XmlHelpers;

Console.OutputEncoding = System.Text.Encoding.UTF8; //unicode symbols

//Console.WriteLine("Dir:"+Directory.GetCurrentDirectory());

var libName = "";

foreach(var f in Directory.GetFiles(Directory.GetCurrentDirectory()))
{
    //Console.WriteLine("File:"+f);
    if(f.EndsWith(".xml"))
    {
        libName = f;
        break;
    }
}

if(string.IsNullOrEmpty(libName))
{
    Console.WriteLine("No library found (.xml) in current directory");
    return;
}
else
{
    Console.WriteLine("Library to check: "+libName);
}

XmlDocument doc = new XmlDocument();
doc.Load(libName);

do
{
    switch(Menu.ShowMenu())
    {
        case MenuItem.Exit:
            return;
        case MenuItem.DispayName:
            DisplayNameHandler();
            break;
        case MenuItem.Name:
            NameHandler();
            break;
        case MenuItem.NameById:
            NameByIdHandler();
            break;
        case MenuItem.Usage:
            UsingsHandler();
            break;
        case MenuItem.UsageTree:
            UsingsTreeHandler();
            break;
        case MenuItem.Conditions:
            ConditionsHandler();
            break;
        case MenuItem.Uses:
            UsesHandler();
            break;
        case MenuItem.UsesTree:
            UsesTreeHandler();
            break;
        case MenuItem.GetUniqNames:
            GetUniqNamesHandler();
            break;
        case MenuItem.GetAccIndexes:
            GetAccIndexesHandler();
            break;
        case MenuItem.GetAllAccumulators:
            GetAllAccumulatorsHandler();
            break;
        case MenuItem.GetAllRules:
            GetAllRulesHandler();
            break;
        case MenuItem.GetAllHistorical:
            GetAllHistoricalHandler();
            break;
        case MenuItem.GetAllAccumulated:
            GetAllAccumulatedHandler();
            break;
        case MenuItem.GetBook:
            GetBookHandler();
            break;
        case MenuItem.BatchGetBooks:
            BatchGetBooksHandler();
            break;
        default:
            throw new Exception("Unknown menu type");
    }
} while (true);




void NameByIdHandler()
{
    Console.Write("Enter Id to find: ");
    var id = Console.ReadLine();

    var name = xml.GetNameForId(doc, id);
    Console.WriteLine("Name: "+name);

    var displayName = xml.GetXXXForId(doc,"DisplayName", id);
    Console.WriteLine("DisplayName: "+displayName);
}

void DisplayNameHandler()
{
    Console.Write("Enter name to find: ");
    var name = Console.ReadLine();

    var id = xml.GetIdForName(doc, name);
    Console.WriteLine("Id: "+id);

    var type = xml.GetXXXBaseForId(doc, "Kind" , id);
    Console.WriteLine($"Type: "+GetObjectType(type));

    var flags = xml.GetXXXForId(doc, "Flags", id);
    if(!string.IsNullOrEmpty(flags))
        Console.WriteLine("Flags: "+getFlags(flags));

    var displayName = xml.GetXXXForId(doc,"DisplayName", id);
    Console.WriteLine("DisplayName: "+displayName);
}

void NameHandler()
{
    Console.Write("Enter display name to find: ");
    var displayName = Console.ReadLine();

    var id = xml.GetIdForDisplayName(doc, displayName);
    Console.WriteLine("Id: " + id);

    var name = xml.GetNameForId(doc, id);
    Console.WriteLine("Name: " + name);
}

void GetBookHandler()
{
    Console.Write("Enter name to find: ");
    var name = Console.ReadLine();
    if (name.Contains(":"))
    {
        name = name.Substring(0, name.IndexOf(":"));
        //Console.WriteLine("normal name: " + name);
    }

    var id = xml.GetIdForName(doc, name);
    if (!string.IsNullOrEmpty(id))
        Console.WriteLine("Id: " + id);

    var parent = id;
    while (true)
    {
        var type = xml.GetXXXBaseForId(doc, "Kind", parent);
        //Console.WriteLine($"Type: " + GetObjectType(type));

        if (GetObjectType(type) == "Unknown")
        {
            Console.WriteLine($"Container NOT found");
            break;
        }
        if (GetObjectType(type) == "Container")
        {
            //if parent for our container is Library then return
            var tmp = xml.GetXXXBaseForId(doc, "ParentId", parent);
            var tmpType = xml.GetXXXBaseForId(doc, "Kind", tmp);
            if (GetObjectType(tmpType) == "Library")
            {
                var parentName = xml.GetNameForId(doc, parent);
                Console.WriteLine($"Container:{parentName}");
                break;
            }
        }

        //go to parent
        parent = xml.GetXXXBaseForId(doc, "ParentId", parent);
        //Console.WriteLine($"Parent: " + parent);
    }
}

void BatchGetBooksHandler()
{
    Console.Write("Enter names to find: ");
    string line;
    var list = new List<string>();
    while(!string.IsNullOrEmpty(line = Console.ReadLine()))
    {
        list.Add(line);
    }
    Console.WriteLine("Names to process:" + list.Count);

    var results = new Dictionary<string, string>();

    foreach (var l in list)
    {
        var name = l;
        if (name.Contains(":"))
            name = name.Substring(0, name.IndexOf(":"));

        if (!results.ContainsKey(name))
        {
            var parent = xml.GetIdForName(doc, name);
            while (true)
            {
                //Console.WriteLine("Search name:" + name+" id:"+parent);

                var type = xml.GetXXXBaseForId(doc, "Kind", parent);
                if (GetObjectType(type) == "Unknown")
                {
                    results.Add(name, "-");
                    break;
                }
                if (GetObjectType(type) == "Container")
                {
                    //if parent for our container is Library then return
                    var tmp = xml.GetXXXBaseForId(doc, "ParentId", parent);
                    var tmpType = xml.GetXXXBaseForId(doc, "Kind", tmp);
                    if (GetObjectType(tmpType) == "Library")
                    {
                        var parentName = xml.GetNameForId(doc, parent);
                        results.Add(name, parentName);
                        break;
                    }
                }
                parent = xml.GetXXXBaseForId(doc, "ParentId", parent);
            }
        }
        Console.WriteLine(results[name]);
    }
}

void UsingsHandler()
{
    Console.Write("Enter name to find: ");
    var name = Console.ReadLine();

    var usages = xml.GetUsages(doc, name);
    //convert DefinitionId to names (display names)

    var uniq = new HashSet<string>();
    foreach(var node in usages)
    {
        uniq.Add(xml.FindNode(node.ChildNodes, "DefinitionId")!.InnerText);
    }

    var usedNames = new HashSet<string>() { name };//exclude itself

    if(uniq.Count == 0)
    {
        Console.WriteLine("No usage");
        return;
    }

    Console.WriteLine("Used in:");
    foreach(var node in uniq)
    {
        var n = xml.GetNameForId(doc,node);
        if(n.IndexOf(":") > 0)
            n = n.Remove(n.IndexOf(":"));

        if(usedNames.Contains(n))
            continue;

        var displayName = xml.GetXXXForId(doc,"DisplayName",node);
        Console.WriteLine(" - "+n+(string.IsNullOrEmpty(displayName)?"":(" ("+displayName+")")));

        usedNames.Add(n);
    }
}

//IsDutyEnd->CantBeSplitDuty->(ActualGapAfterDutyActivity >= NextMinOnDutyCIAO)->MinOnDutyCIAO->MinOnDuty
void UsingsTreeHandler()
{
    Console.Write("Enter name to find: ");
    var name = Console.ReadLine();

        var q = new Queue<string>();
        q.Enqueue(name);

        var used = new HashSet<string>();

        var tree = new Node(name);
        var treeHash = new HashSet<Node>();
        treeHash.Add(tree);

        int cnt = 0;
        while(q.Count > 0)
        {
            var el = q.Dequeue();
            //Console.WriteLine("Element:"+el);

            Node workNode = treeHash.Where(x=>x.Name == el).FirstOrDefault();
                if(workNode == null) throw new Exception("No work node for object:"+el);

            var usages = XmlHelpers.GetUsagesNamesUniq(doc, el, true);
            //Console.WriteLine("t1, usages:"+usages.Count);
            foreach(var n in usages.OrderBy(x=>x))
            {
                var newNode = XmlHelpers.GetOrCreateNode(treeHash, n, workNode);

                workNode.Children.Add(newNode);
                treeHash.Add(newNode);

                if(!used.Contains(n))
                {
                    q.Enqueue(n);
                    used.Add(n);
                }
            }

/*
            //debug
            cnt++;
            if(cnt >= 100) //100
                break;
                */
        }

        //PrintTree(tree, 0, 10);
        if(tree.Children.Count == 0)
            Console.WriteLine("No usage");
        else
            xml.PrintTree(tree, 0, int.MaxValue, true);
}

void ConditionsHandler()
{
    Console.Write("Enter name to find: ");
    var name = Console.ReadLine();

    var id = xml.GetIdForName(doc, name);
    Console.WriteLine("Id: "+id);

    var displayName = xml.GetXXXForId(doc,"DisplayName", id);
    Console.WriteLine("DisplayName: "+displayName);

    foreach(var c in xml.ConditionsContainingCalculators)
    {
        var cc = xml.ExtractCalculatorsFromCondition(xml.GetXXXForId(doc, c, id));
        if(cc.Count > 0)
            Console.WriteLine(c + ": " + string.Join(" ",cc));
    }

    //alternatives
    var alternatives = xml.GetAlternatives(doc, name);
    if(alternatives.Count > 0)
    {
        Console.WriteLine($"!!! Warning this object has {alternatives.Count} alternative(s) !!!");

        Console.WriteLine($"Show them? (y) or press any key to ignore");
        var rl = Console.ReadLine();
        if(rl == "y" || rl == "Y")
        {
            foreach(var r in alternatives)
            {
                var ids = xml.FindNode(r.ChildNodes, "DefinitionId")!.InnerText;

                var nn = xml.GetNameForId(doc, ids);
                if(!string.IsNullOrEmpty(nn))
                    Console.WriteLine("Name: "+nn);

                var dn = xml.GetXXXForId(doc,"DisplayName", ids);
                if(!string.IsNullOrEmpty(dn))
                    Console.WriteLine("DisplayName: "+dn);

                foreach(var c in xml.ConditionsContainingCalculators)
                {
                    var cc = xml.ExtractCalculatorsFromCondition(xml.GetXXXForId(doc, c, ids));
                    if(cc.Count > 0)
                        Console.WriteLine(c + ": " + string.Join(" ",cc));
                }
            }
        }
    }
}

void UsesHandler()
{
    Console.Write("Enter name to find: ");
    var name = Console.ReadLine();

    var id = xml.GetIdForName(doc, name);
    Console.WriteLine("Id: "+id);

    var displayName = xml.GetXXXForId(doc,"DisplayName", id);
    Console.WriteLine("DisplayName: "+displayName);

    var uniq = new HashSet<string>();
    //get calcs from main condition
    foreach(var c in xml.ConditionsContainingCalculators)
    {
        foreach(var e in xml.ExtractCalculatorsFromCondition(xml.GetXXXForId(doc, c, id)))
        {
            uniq.Add(e);
        }
    }
    
    //get calcs from alternatives
    var alternatives = xml.GetAlternatives(doc, name);
    foreach(var r in alternatives)
    {
        var ids = xml.FindNode(r.ChildNodes, "DefinitionId")!.InnerText;
        foreach(var c in xml.ConditionsContainingCalculators)
        {
            foreach(var e in xml.ExtractCalculatorsFromCondition(xml.GetXXXForId(doc, c, ids)))
            {
                uniq.Add(e);
            }
        }
    }

    Console.WriteLine("Uses:");
    foreach(var r in uniq)
    {
        var ids = xml.GetIdForName(doc, r);
        var dn = xml.GetXXXForId(doc,"DisplayName", ids);
        //var ids = xml.FindNode(r.ChildNodes, "DefinitionId")!.InnerText;
        //var n = xml.FindNode(r.ChildNodes, "Name")!.InnerText;
        //var dn = xml.GetXXXForId(doc,"DisplayName",ids);

        Console.WriteLine(" - "+r+(string.IsNullOrEmpty(dn)?"":(" ("+dn+")")));
    }
}

void UsesTreeHandler()
{


//TBD
//option to exclude fileds "_xxx_"
//option to exclude NON calculators (which we don't have definitions)



    Console.Write("Enter name to find: ");
    var name = Console.ReadLine();
    //var name = "IsDutyStart";//"MixedFlightCrewComplements";



    //var uses = xml.GetUses(doc, name);
    /*
    Console.WriteLine("Use:");
    foreach(var r in uses)
    {
        var ids = xml.FindNode(r.ChildNodes, "DefinitionId")!.InnerText;
        var n = xml.FindNode(r.ChildNodes, "Name")!.InnerText;
        var dn = xml.GetXXXForId(doc,"DisplayName",ids);

        Console.WriteLine(" - "+n+(string.IsNullOrEmpty(dn)?"":(" ("+dn+")")));
    }*/

    var q = new Queue<string>();
    q.Enqueue(name);

    var used = new HashSet<string>();

    var tree = new Node(name);
    var treeHash = new HashSet<Node>();
    treeHash.Add(tree);

    int cnt = 0;
    while(q.Count > 0)
    {
        var el = q.Dequeue();
        //Console.WriteLine("Element: "+el);

        Node workNode = treeHash.Where(x=>x.Name == el).FirstOrDefault();
            if(workNode == null) throw new Exception("No work node for object:"+el);

        var uniq = new HashSet<string>();
        //get calcs from main condition
        var id = xml.GetIdForName(doc, el);
        foreach(var c in xml.ConditionsContainingCalculators)
        {
            foreach(var e in xml.ExtractCalculatorsFromCondition(xml.GetXXXForId(doc, c, id)))
            {
                uniq.Add(e);
            }
        }

        //get calcs from alternatives
        var alternatives = xml.GetAlternatives(doc, el);
        foreach(var r in alternatives)
        {
            var ids = xml.FindNode(r.ChildNodes, "DefinitionId")!.InnerText;
            foreach(var c in xml.ConditionsContainingCalculators)
            {
                foreach(var e in xml.ExtractCalculatorsFromCondition(xml.GetXXXForId(doc, c, ids)))
                {
                    uniq.Add(e);
                }
            }
        }

        //remove fileds
        var uniq2 = new HashSet<string>();
        foreach(var r in uniq)
        {
            if(!(r.StartsWith("_") && r.EndsWith("_")))
                uniq2.Add(r);
        }
        uniq = uniq2;

        //Console.WriteLine("Uses:");
        foreach(var r in uniq)
        {
            //Console.WriteLine("foreach(var r in uniq): "+"|"+r+"|");
            //var ids = xml.GetIdForName(doc, r);
            //var dn = xml.GetXXXForId(doc,"DisplayName", ids);
            //var ids = xml.FindNode(r.ChildNodes, "DefinitionId")!.InnerText;
            //var n = xml.FindNode(r.ChildNodes, "Name")!.InnerText;
            //var dn = xml.GetXXXForId(doc,"DisplayName",ids);

            //Console.WriteLine(" - "+r+(string.IsNullOrEmpty(dn)?"":(" ("+dn+")")));
            var newNode = XmlHelpers.GetOrCreateNode(treeHash, r, workNode);
            workNode.Children.Add(newNode);
            treeHash.Add(newNode);
            if(!used.Contains(r))
            {
                q.Enqueue(r);
                used.Add(r);
            }
        }


            //debug
            cnt++;
            if(cnt >= 100) //100
                break;
                
        }

        //PrintTree(tree, 0, 10);
        if(tree.Children.Count == 0)
            Console.WriteLine("No usage");
        else
            xml.PrintTree(tree, 0, int.MaxValue, true);
}

void GetUniqNamesHandler()
{
    Console.Write("Enter names: ");

    var names = new List<string>();   
    string line;
    while ((line = Console.ReadLine()) != null && line != "") 
        names.Add(line.Trim());
    

    var hs = new HashSet<string>();
    foreach(var n in names)
    {
        hs.Add(n.Trim());
    }
    Console.WriteLine("Result were:"+names.Count+" now:"+hs.Count);
    foreach(var n in hs.OrderBy(x=>x))
    {
        Console.WriteLine(n);
    }
}

string GetAccumulateType(string type)
{
    switch(type)
    {
        case "0": 
        case "1": 
        case "2": 
        case "3": 
        case "4": 
        case "5": 
        case "6": 
        case "7": 
            return "Daily";
        case "8": 
            return "Monthly";
        case "9": 
            return "Quarterly";
        case "10": 
            return "Yearly";
        case "11": 
            return "Weekly";
        case "12": 
            return "Dates";
        case "13": 
            return "Hourly";
        case "14": 
            return "Calculation period";
        case "16": 
            return "Custom periods";
    }
    return "Daily"; //default value if not exist
} 

string GetObjectType(string type)
{
    switch(type)
    {
        case "0": 
            return "Unknown";
        case "1": 
            return "Library";
        case "2": 
            return "Container";
        case "3": 
        case "4": 
            return "Rule";
        case "5": 
        case "6": 
            return "Calculator";
        case "7": 
            return "Accumulator";
        case "8": 
            return "DataTableInternal";
        case "9": 
            return "DataTableExternal";
        case "10": 
            return "BuildInField";
        case "11": 
            return "BuildInFunction";
        case "12": 
            return "CustomDefinedFunction";
    }
    return "Unknown";
}

void GetAccIndexesHandler()
{
    Console.Write("Enter name/id to find: ");
    var name_or_id = Console.ReadLine();

    var id = xml.GetIdForName(doc, name_or_id);
    if(string.IsNullOrEmpty(id))
    {
        var name = xml.GetNameForId(doc, name_or_id);
        if(string.IsNullOrEmpty(name))
        {
            Console.WriteLine("Not found");
            return;
        }
        Console.WriteLine("Name: "+name);
        id = name_or_id;
    }
    else
    {
        Console.WriteLine("Id: "+id);
    }

    var displayName = xml.GetXXXForId(doc,"DisplayName", id);
    Console.WriteLine("DisplayName: "+displayName);

    var type = xml.GetXXXForId(doc, "AccumulateType" , id);
    var start_end = type != "16" 
        ? ( xml.GetXXXForId(doc, "AccumulateStartIndex" , id) + ", "+xml.GetXXXForId(doc, "AccumulateEndIndex" , id))
        : (xml.GetXXXForId(doc, "AccumulatePeriods" , id) //custom
        );

    Console.WriteLine($"Type: "+GetAccumulateType(type));
    Console.WriteLine($"Indexes: {start_end}");
}

void GetAllAccumulatorsHandler()
{
    var ret = new HashSet<XmlNode>();
    xml.CollectNodes(doc.ChildNodes, ret, "AvxMimerDefiniton","Kind","7");//accumulators
    
    foreach(var n in ret)
    {
        var id = xml.FindNode(n.ChildNodes, "DefinitionId")!.InnerText;
        var accName = xml.GetNameForId(doc,id);
        var type = xml.GetXXXForId(doc, "AccumulateType" , id);
        var start_end = type != "16" 
        ? ( xml.GetXXXForId(doc, "AccumulateStartIndex" , id) + ", "+xml.GetXXXForId(doc, "AccumulateEndIndex" , id))
        : (xml.GetXXXForId(doc, "AccumulatePeriods" , id) //custom
        );
        Console.Write(id+" "+accName);

        //+conditions
        var cc = xml.ExtractCalculatorsFromCondition(xml.GetXXXForId(doc, "Formula", id));
        if(cc.Count > 0)
            Console.Write(" | " + string.Join(" ",cc));

        Console.Write(" | "+GetAccumulateType(type));
  
        //+indexes
        if(!string.IsNullOrEmpty(start_end) && start_end != ", ")
            Console.Write(" |" + start_end+"|");

        Console.Write(Environment.NewLine);
    }
}

void GetAllRulesHandler()
{
    var ret = new HashSet<XmlNode>();
    xml.CollectNodes(doc.ChildNodes, ret, "AvxMimerDefiniton","Kind","3");//rules
    
    foreach(var n in ret)
    {
        var id = xml.FindNode(n.ChildNodes, "DefinitionId")!.InnerText;
        var name = xml.GetNameForId(doc,id);
        var displayName = xml.GetXXXForId(doc,"DisplayName", id);
        var comment = xml.GetXXXForId(doc,"Comment", id);
        var enabled = xml.GetXXXForId(doc,"Enabled", id);//No=2
/*
        Console.WriteLine(id + " " + name 
        + (string.IsNullOrEmpty(displayName) ? "" : (" ("+displayName+")")) 
        + (enabled == "2" ? " DISABLED" : ""));
        */

        Console.WriteLine((enabled == "2" ? "DISABLED " : "")
        +(string.IsNullOrEmpty(displayName) ? name : displayName) 
        +(string.IsNullOrEmpty(comment) ? "" : (" :"+comment)) 
        );
    }
}

void GetAllHistoricalHandler()
{
    var ret = new HashSet<XmlNode>();
    xml.CollectNodes(doc.ChildNodes, ret, "AvxMimerDefiniton","Kind","5");//Calculator
    
    foreach(var n in ret)
    {
        var id = xml.FindNode(n.ChildNodes, "DefinitionId")!.InnerText;
        var flags = xml.GetXXXForId(doc, "Flags", id);
        if (string.IsNullOrEmpty(flags))
            continue;
        var e = (AvxMimerDefinitionFlags)int.Parse(flags);
        if ((e & AvxMimerDefinitionFlags.Historical) == AvxMimerDefinitionFlags.Historical)
        {
            var name = xml.GetNameForId(doc, id);
            //var displayName = xml.GetXXXForId(doc, "DisplayName", id);
            //var comment = xml.GetXXXForId(doc, "Comment", id);
            var enabled = xml.GetXXXForId(doc, "Enabled", id);//No=2

            Console.WriteLine(name
                //(enabled == "2" ? "DISABLED " : "")
            //+ (string.IsNullOrEmpty(displayName) ? name : displayName)
            //+ (string.IsNullOrEmpty(comment) ? "" : (" :" + comment))
            );
        }
    }
}

void GetAllAccumulatedHandler()
{
    var ret = new HashSet<XmlNode>();
    xml.CollectNodes(doc.ChildNodes, ret, "AvxMimerDefiniton","Kind","5");//Calculator
    
    foreach(var n in ret)
    {
        var id = xml.FindNode(n.ChildNodes, "DefinitionId")!.InnerText;
        var flags = xml.GetXXXForId(doc, "Flags", id);
        if (string.IsNullOrEmpty(flags))
            continue;
        var e = (AvxMimerDefinitionFlags)int.Parse(flags);
        if ((e & AvxMimerDefinitionFlags.ExternalCache) == AvxMimerDefinitionFlags.ExternalCache)
        {
            var name = xml.GetNameForId(doc, id);
            var displayName = xml.GetXXXForId(doc, "DisplayName", id);
            var comment = xml.GetXXXForId(doc, "Comment", id);
            //var enabled = xml.GetXXXForId(doc, "Enabled", id);//No=2

            Console.WriteLine(name
                //+(enabled == "2" ? "DISABLED " : "")
                +(string.IsNullOrEmpty(displayName)?";":(";"+displayName))
                +(string.IsNullOrEmpty(comment) ? ";" : (";" + comment))
            );
        }
    }
}

void test()
{
    var strs = new string[]{
//"(Activity_223_BLH_Sum() < new AvxExpressionResultTimeSpan(TimeSpan.FromTicks(3600000000000)))  &&  (\"CA\".IntersectsWith(_AssignedRank_()))",
//"(Activity_E90_BLH_Sum() < new AvxExpressionResultTimeSpan(TimeSpan.FromTicks(3600000000000)))  &&  (\"CA\".IntersectsWith(_AssignedRank_()))",
//"(BlockHours365Days() > new AvxExpressionResultTimeSpan(TimeSpan.FromTicks(34200000000000)))  &&  (IsDutyCrew() && IsFlightDeckCrew())  &&  (_Sum_(\"BLH\", _TimeIndex_(), _TimeIndex_().Date().AddDays(1).AddSeconds(-1)) > new AvxExpressionResultTimeSpan(TimeSpan.FromTicks(0)))",
//"_Sum_(\"SBLH()\",RotationStartUTC(),RotationEndUTC())"
"(ExactSum(\"Far121_BLH_Scheduled\", DutyStartUTC(), Far121_ActivityEndUTC())) + (ExactSum(\"Far121_BLH_Best\", Far121_ActivityEndUTC().AddHours(-24), DutyStartUTC().AddMinutes(-1)))"
    };
    foreach(var s in strs)
    {
        //IsConditionContainsObject
        Console.WriteLine($"s:{s}:{Environment.NewLine}{(string.Join(Environment.NewLine,xml.ExtractCalculatorsFromCondition(s)))}");


        Console.WriteLine($"s:{s}:"+(XmlHelpers.IsConditionContainsObject(s,"BLH")?"+++":"---"));
    }
}

string getFlags(string f)
{
    var e = (AvxMimerDefinitionFlags)int.Parse(f);
    var enums = Enum.GetValues(typeof(AvxMimerDefinitionFlags));
    var list = new List<string>();

    foreach (var v in enums)
    {
        if ((e & (AvxMimerDefinitionFlags)v) == (AvxMimerDefinitionFlags)v)
            list.Add(v.ToString());
    }

    return list.Count == 0 ? string.Empty : string.Join(",",list);
}

enum AvxMimerDefinitionFlags
{
    //None = 0,
    ExternalCache = 1,
    AllPossibleResults = 1 << 1,
    UpdateData = 1 << 3,
    SplitAtMidnight = 1 << 4,
    Merge_Results = 1 << 5,
    OriginalExternalCacheItem = 1 << 6,
    Historical = 1 << 7,
    ExternalCacheDetailed = 1 << 8,
}

//bool bHistorical = (Flags & AvxMimerDefinitionFlags.Historical) == AvxMimerDefinitionFlags.Historical


