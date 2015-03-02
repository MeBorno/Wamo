using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class FileManager
{
    enum LoadType { Attribute, Contents };

    LoadType type;

    List<string> tmpAttributes;
    List<string> tmpContents;

    bool identifierFound = false;

    public void LoadContent(string filename, List<List<string>> attributes, List<List<string>> contents)
    {
        using (StreamReader reader = new StreamReader("Content/"+filename))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if(line.Contains("Load="))
                {
                    tmpAttributes = new List<string>();
                    line = line.Remove(0, line.IndexOf("=") + 1);
                    type = LoadType.Attribute;
                }
                else
                {
                    tmpContents = new List<string>();
                    type = LoadType.Contents;
                }

                string[] lineArray = line.Split(']');
                foreach(string li in lineArray)
                {
                    string newLine = li.Trim('[', ' ', ']');
                    if (newLine != string.Empty)
                    {
                        if (type == LoadType.Contents) tmpContents.Add(newLine);
                        else tmpAttributes.Add(newLine);
                    }
                }

                if(type == LoadType.Contents && tmpContents.Count > 0)
                {
                    contents.Add(tmpContents);
                    attributes.Add(tmpAttributes);
                }
            }
        }
    }

    public void LoadContent(string filename, List<List<string>> attributes, List<List<string>> contents, string identifier)
    {
        using (StreamReader reader = new StreamReader(filename))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line.Contains("EndLoad=") && line.Contains(identifier))
                {
                    identifierFound = false;
                    break;
                }
                else if (line.Contains("Load=") && line.Contains(identifier))
                {
                    identifierFound = true;
                    continue;
                }


                if (identifierFound)
                {
                    if (line.Contains("Load="))
                    {
                        tmpAttributes = new List<string>();
                        line.Remove(0, line.IndexOf("=") + 1);
                        type = LoadType.Attribute;
                    }
                    else
                    {
                        tmpContents = new List<string>();
                        type = LoadType.Contents;
                    }

                    string[] lineArray = line.Split(']');
                    foreach (string li in lineArray)
                    {
                        string newLine = li.Trim('[', ' ', ']');
                        if (newLine != string.Empty)
                        {
                            if (type == LoadType.Contents) tmpContents.Add(newLine);
                            else tmpAttributes.Add(newLine);
                        }
                    }

                    if (type == LoadType.Contents && tmpContents.Count > 0)
                    {
                        contents.Add(tmpContents);
                        attributes.Add(tmpAttributes);
                    }
                }
            }
        }
    }
}
