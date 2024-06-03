using System.Collections.Generic;
namespace GrooperGit
{
    public class Utils
    {
        public string MonoSpace(string input)
        {
            List<char> newstring = new List<char>();
            foreach (char c in input)
            {
                newstring.Add(c);
                newstring.Add(' ');
            }
            return newstring.ToArray().ToString();
        }
    }
}
