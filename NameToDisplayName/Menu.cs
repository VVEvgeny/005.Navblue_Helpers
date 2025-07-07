#pragma warning disable

namespace NameToDisplayName
{
    public class Description : Attribute
    {
        public string Text;
        public Description(string desc)
        {
            Text = desc;
        }
    }

    public enum MenuItem
    {
        [Description("Exit")]
        Exit = 0,
        [Description("Get display name by name")]
        DispayName,
        [Description("Get name by display name")]
        Name,
        [Description("Get name by Id")]
        NameById,
        [Description("Get usage by name")]
        Usage,
        [Description("Get usage tree (where it used)")]
        UsageTree,
        [Description("Get conditions by name")]
        Conditions,
        [Description("Get uses by name (what it uses)")]
        Uses,
        [Description("Get uses tree by name")]
        UsesTree,
        [Description("Get uniq names by name")]
        GetUniqNames,
        [Description("Get Accumulate Indexes by name/id")]
        GetAccIndexes,
        [Description("Get All Accumulators")]
        GetAllAccumulators,
        [Description("Get All Rules")]
        GetAllRules,
        [Description("Get All Historical")]
        GetAllHistorical,
        [Description("Get All Accumulated")]
        GetAllAccumulated,
    };

    public static class Menu
    {
        public static MenuItem ShowMenu()
        {
            MenuItem c = MenuItem.Exit;
            while(true)
            {
                Console.Write("Type: \n");
                var names = Enum.GetNames(typeof(MenuItem));
                foreach (var name in names)
                {
                    var field = typeof(MenuItem).GetField(name);
                    var fds = field.GetCustomAttributes(typeof(Description), true);
                    foreach (Description fd in fds)
                    {
                        int value = (int)Enum.Parse(typeof(MenuItem), name);
                        Console.Write("\t" + value + " - " + fd.Text + "\n");
                    }
                }
                Console.Write("\n:");
                
                try{
                    var line = int.Parse(Console.ReadLine());
                    if(!Enum.IsDefined(typeof(MenuItem), line))
                        throw new Exception();

                    c = (MenuItem)line;
                    break;
                }
                catch
                {
                    Console.WriteLine("Incorrect choice, try again");
                }
            }
            return c;
        }
    }
}