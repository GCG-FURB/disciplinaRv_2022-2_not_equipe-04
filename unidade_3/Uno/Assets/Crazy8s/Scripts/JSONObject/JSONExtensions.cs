using System.Linq;

public static class JSONExtensions
{

    public static float LoadFloatValue(this JSONObject data, string fieldID, float defaultValue = 0)
    {
        if (!data.ContainsKey(fieldID))
            data.AddField(fieldID, defaultValue);
        return data[fieldID].f;
    }

    public static int LoadIntValue(this JSONObject data, string fieldID, int defaultValue = 0)
    {
        if (!data.ContainsKey(fieldID))
            data.AddField(fieldID, defaultValue);
        return data[fieldID].n;
    }

    public static bool LoadBoolValue(this JSONObject data, string fieldID, bool defaultValue = false)
    {
        if (!data.ContainsKey(fieldID))
            data.AddField(fieldID, defaultValue);
        return data[fieldID].b;
    }

    public static string LoadStringValue(this JSONObject data, string fieldID, string defaultValue = "")
    {
        if (!data.ContainsKey(fieldID))
            data.AddField(fieldID, defaultValue);
        return data[fieldID].str;
    }

    public static bool[] LoadArrayValue(this JSONObject data, string fieldID, bool[] defaultValue)
    {
        if (!data.ContainsKey(fieldID))
            data.AddField(fieldID, defaultValue.ToList());

        bool[] values = new bool[data[fieldID].Count];
        for (int i = 0; i < data[fieldID].Count; i++)
        {
            values[i] = data[fieldID][i].b;
        }
        return values;
    }
}
