using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class Options
{
    private static Dictionary<string, object> options;

    public static void LoadOptions(string fname)
    {
        options = new Dictionary<string, object>();
        options.Add("shutDown", false);
        options.Add("lightEngine", false);
        options.Add("role", State.System);

        options.Add("scramble", false);
        options.Add("fog", false);
        options.Add("paralyze", false);
        options.Add("robotLight", false);
        options.Add("painting", false);

        if (File.Exists(fname))
        {
            StreamReader sr = File.OpenText(fname);

            string line = "";
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();

                if (line.StartsWith("#") || line == string.Empty) continue;
                else if (line.Contains("="))
                {
                    if (line.IndexOf("#") != -1) line = line.Substring(0, line.IndexOf("#"));
                    string[] tmp = line.Split('=');

                    object val = null;
                    if (tmp[1].ToLower() == "shutdown") continue;
                    else if (tmp[1].ToLower() == "lightengine") continue;
                    else if (tmp[1].ToLower() == "role") continue;
                    if (tmp[1].StartsWith("["))
                    {
                        switch (tmp[1].Substring(1, tmp[1].IndexOf("]") - 1).ToLower())
                        {
                            case "int": val = int.Parse(tmp[1].Substring(tmp[1].IndexOf("]") + 1));
                                break;
                            case "bool": val = bool.Parse(tmp[1].Substring(tmp[1].IndexOf("]") + 1));
                                break;
                            case "float": val = float.Parse(tmp[1].Substring(tmp[1].IndexOf("]") + 1));
                                break;
                        }
                    }
                    else val = tmp[1];
                    if (options.ContainsKey(tmp[0])) options[tmp[0]] = val;
                    else options.Add(tmp[0], val);
                }
            }
            sr.Close();
        }
        else
        {
            options.Add("name", "Wamo"+Guid.NewGuid().ToString().Substring(0,2));
            options.Add("defaultIP", "127.0.0.1");
            options.Add("musicVolume", 1.00f);
            options.Add("soundVolume", 1.00f);
        }
        
    }

    public static void SaveOptions(string fname)
    {

    }

    public static void SetValue(string name, object value)
    {
        if (options.ContainsKey(name)) options[name] = value;
        else options.Add(name, value);
    }

    public static T GetValue<T>(string name)
    {
        if (options.ContainsKey(name)) return (T)options[name];
        else return default(T);
    }
}
