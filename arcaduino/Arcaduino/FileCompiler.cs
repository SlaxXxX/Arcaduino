using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsInput;

namespace Arcaduino
{
    class FileCompiler
    {
        KeyManager keyManager = new KeyManager();
        MultiKeyManager multiKeyManager = new MultiKeyManager();
        KeyCombinationManager keyCombinationManager = new KeyCombinationManager();
        KeyAxisManager keyAxisManager = new KeyAxisManager();

        public void readFile(string path)
        {
            string[] fileContent = System.IO.File.ReadAllLines(path);
            foreach (string line in fileContent)
            {
                string line_wc = line;
                keyManager.clear();
                multiKeyManager.clear();
                keyCombinationManager.clear();
                keyAxisManager.clear();

                Match match = Regex.Match(line_wc, @"^(A?\d+)\s*-\s*([\w\s+,|]+)$");
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    line_wc = Regex.Replace(match.Groups[2].Value, @"\s", "");

                    line_wc = keyManager.replaceAll(line_wc);
                    line_wc = multiKeyManager.replaceAll(line_wc);
                    line_wc = keyCombinationManager.replaceAll(line_wc);
                    line_wc = keyAxisManager.replaceAll(line_wc);

                    if (Regex.IsMatch(line_wc,@"\$\d+\w+%"))
                    {

                    }
                }
            }
        }
    }

    abstract class TokenManager
    {
        abstract protected string idKey { get; }
        abstract protected string rawPattern { get; }

        List<string> token;

        public string replaceAll(string str)
        {
            List<int> startPos = new List<int>();
            foreach (Match match in Regex.Matches(str, rawPattern))
            {
                startPos.Add(match.Groups[0].Index);
                token.Add(match.Value);
            }

            int lastEnd = 0;
            string newStr = "";
            Regex regex = new Regex(rawPattern);
            for (int i = 0; i < token.Count; i++)
            {
                newStr += str.Substring(lastEnd, startPos[i] - lastEnd) + "$" + i + idKey + "%";
                lastEnd = startPos[i] + token[i].Length;
                //str = regex.Replace(str, match => { return "$" + nextId++ + idKey + "%"; }, 1);
            }
            return newStr;
        }

        public void clear()
        {
            token = new List<string>();
        }

        public List<string> getAllToken()
        {
            return token;
        }
        public string getElement(int index)
        {
            return token[index];
        }
        public string getElement(string id)
        {
            return token[Convert.ToInt16(Regex.Match(id, @"(?<=\$)\d+").Value)];
        }
    }

    class KeyManager : TokenManager
    {
        protected override string idKey
        {
            get { return "key"; }
        }
        protected override string rawPattern
        {
            get { return @"\w+"; }
        }
        public static string idPattern = @"\$\d+key%";
    }

    class MultiKeyManager : TokenManager
    {
        protected override string idKey
        {
            get { return "mkey"; }
        }
        protected override string rawPattern
        {
            get { return KeyManager.idPattern + @"(?:\+" + KeyManager.idPattern + ")+"; }
        }
        public static string idPattern = @"\$\d+mkey%";
    }

    class KeyCombinationManager : TokenManager
    {
        protected override string idKey
        {
            get { return "keyc"; }
        }
        protected override string rawPattern
        {
            get { return "(" + KeyManager.idPattern + "|" + MultiKeyManager.idPattern + "),(" + KeyManager.idPattern + "|" + MultiKeyManager.idPattern + ")"; }
        }
        public static string idPattern = @"\$\d+keyc%";
    }

    class KeyAxisManager : TokenManager
    {
        protected override string idKey
        {
            get { return "keya"; }
        }
        protected override string rawPattern
        {
            get { return "(" + KeyManager.idPattern + "|" + MultiKeyManager.idPattern + "|" + KeyCombinationManager.idPattern + @")\|(" + KeyManager.idPattern + "|" + MultiKeyManager.idPattern + "|" + KeyCombinationManager.idPattern + ")"; }
        }
        public static string idPattern = @"\$\d+keya%";
    }

    abstract class KeyExecutor
    {
        public abstract void keysDown(InputSimulator inSim);

        public abstract void keysUp(InputSimulator inSim);
    }

    class Key : KeyExecutor
    {
        public override void keysDown(InputSimulator inSim)
        {
            throw new NotImplementedException();
        }

        public override void keysUp(InputSimulator inSim)
        {
            throw new NotImplementedException();
        }
    }

    class MultiKey : KeyExecutor
    {
        public override void keysDown(InputSimulator inSim)
        {
            throw new NotImplementedException();
        }

        public override void keysUp(InputSimulator inSim)
        {
            throw new NotImplementedException();
        }
    }

    class KeyCombination : KeyExecutor
    {
        public override void keysDown(InputSimulator inSim)
        {
            throw new NotImplementedException();
        }

        public override void keysUp(InputSimulator inSim)
        {
            throw new NotImplementedException();
        }
    }

    class KeyAxis : KeyExecutor
    {
        public override void keysDown(InputSimulator inSim)
        {
            throw new NotImplementedException();
        }

        public override void keysUp(InputSimulator inSim)
        {
            throw new NotImplementedException();
        }
    }
}
