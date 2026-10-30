using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEditor.PackageManager.Requests;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

public class GridCell 
{
    public int x;
    public int y;
    public string block;
}
public class TestScript : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadDoc());
    }

    private IEnumerator LoadDoc() 
    {
        string url = $"https://docs.google.com/document/d/e/2PACX-1vSvM5gDlNvt7npYHhp_XfsJvuntUhq184By5xO_pA4b_gCWeXb6dM6ZxwN8rE6S4ghUsCj2VKR21oEP/pub";
    
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success) 
        {
            //error
            yield break;
        }

        string data = req.downloadHandler.text;
        //Debug.Log(data);
        ParseGrid(data);
    }
    /// <summary>
    /// condenses down the request results to just the table
    /// then parses the table data to x, y, and blockType
    /// puts each chunk of data per row in a List made from a custom class
    /// finally prints the List to the console to show the "F"
    /// </summary>
    /// <param name="data"></param>
    private void ParseGrid(string data) 
    {
        List<GridCell> cells = new List<GridCell>();
        int tblStart = data.IndexOf("<table");
        int tblEnd = data.IndexOf("</table");

        string tblTxt = data.Substring(tblStart, tblEnd - tblStart + "</table>".Length);
        //Debug.Log(tblTxt);
        XDocument doc;

        try
        {
            doc = XDocument.Parse(tblTxt);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            return;
        }

        IEnumerable<XElement> rows = doc.Descendants("tr");
        foreach (XElement row in rows) 
        {
            List<string> cols = row.Descendants("td").Select(td => td.Value.Trim()).ToList();

            if (cols.Count < 3 || cols[0] == "x-coordinate") 
            {
                continue;
            }

            if (!int.TryParse(cols[0], out int x))
            {
               
                continue;
            }
            if (!int.TryParse(cols[2], out int y))
            {
                
                continue;
            }

            string blockType = cols[1];

            //Debug.Log(blockType + x + y);
            cells.Add(new GridCell { x = x, y = y, block = blockType});
            
        }

        PrintGrid(cells);
    }

    private void PrintGrid(List<GridCell> cells) 
    {
        int maxX = cells.Max(c => c.x);
        int maxY = cells.Max(c => c.y);

        System.Text.StringBuilder output = new();
        for (int y = maxY; y >= 0; y --)
        {
            string row = "";
            for (int x = 0; x < maxX; x++) 
            {
                GridCell cell = cells.FirstOrDefault(c => c.x == x && c.y == y);

                if (cell != null)
                {
                    row += cell.block;
                }
                else
                {
                    row += " ";
                }
                output.Append(cell != null ? cell.block : " ");
                
            }

            output.AppendLine();

            Debug.ClearDeveloperConsole();
            //Debug.Log(output.ToString());
            //prints to the console
            Debug.Log(row);
        }
    }
    
}
