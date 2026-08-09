using System.Text;

namespace DataLibrary.Models;
public interface IMiscInfo
{
    string EnvVersion { get; set; }
    Dictionary<string, string> MiscVariables { get; set; }
}
// Contains miscellaneous information related to the application not project data
// 
public class MiscInfo: IMiscInfo
{
    public string EnvVersion { get; set; } = string.Empty;
    public Dictionary<string, string> MiscVariables { get; set; } = new Dictionary<string, string>();

    public MiscInfo()
    {

    }

    public string GetEnvVariable(string envVar)
    {
        string newKey = new string(envVar.Reverse().ToArray());
        if (MiscVariables.Keys.Contains(newKey))
        {
            string target = MiscEnvIn(MiscVariables[newKey], EnvVersion);
            return target;
        }
        return string.Empty;
    }


    public static void PrepEnv(Dictionary<string,string> envVariables, string eV)
    {
        // Implementation for preparing environment variables
        foreach (string key in envVariables.Keys)
            envVariables[key] = MiscEnvout(envVariables[key], eV);
    }

    public static string MiscEnvout(string pText, string sk)
    {
        byte[] pBytes = Encoding.UTF8.GetBytes(pText);
        byte[] ksBytes = Encoding.UTF8.GetBytes(sk);
        byte[] rBytes = new byte[pBytes.Length];

        for (int i = 0; i < pBytes.Length; i++)
        {
            rBytes[i] = (byte)(pBytes[i] ^ ksBytes[i % ksBytes.Length]);
        }

        return Convert.ToBase64String(rBytes);
    }

    public static string MiscEnvIn(string cText, string sk)
    {
        byte[] cBytes = Convert.FromBase64String(cText);
        byte[] ksBytes = Encoding.UTF8.GetBytes(sk);
        byte[] rBytes = new byte[cBytes.Length];

        for (int i = 0; i < cBytes.Length; i++)
        {
            rBytes[i] = (byte)(cBytes[i] ^ ksBytes[i % ksBytes.Length]);
        }

        return Encoding.UTF8.GetString(rBytes);
    }

}
