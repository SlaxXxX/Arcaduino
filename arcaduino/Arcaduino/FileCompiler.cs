using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace Arcaduino
{
    class FileCompiler
    {
        KeyManager keyManager = KeyManager.Instance();
        MultiKeyManager multiKeyManager = MultiKeyManager.Instance();
        KeyCombinationManager keyCombinationManager = KeyCombinationManager.Instance();
        KeyAxisManager keyAxisManager = KeyAxisManager.Instance();

        public Dictionary<string, KeyExecutor> readFile(string path)
        {
            string[] fileContent = System.IO.File.ReadAllLines(path);
            Dictionary<string, KeyExecutor> executors = new Dictionary<string, KeyExecutor>();
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

                    if (Regex.IsMatch(line_wc, @"\$\d+\w+%"))
                    {
                        TokenManager highestLevel = keyAxisManager.getHighestLevel();
                        if (highestLevel != null)
                        {
                            highestLevel.buildExecutorTree(line_wc, id, executors);
                        }
                    }
                }
            }
            return executors;
        }
    }

    abstract class TokenManager
    {
        abstract protected string idKey { get; }
        abstract protected string rawPattern { get; }
        abstract public TokenManager getHighestLevel();
        abstract public void buildExecutorTree(string keyName, string id, Dictionary<string, KeyExecutor> executors);
        abstract public List<VirtualKeyCode> getKeys(int id);

        protected List<string> token;

        public string replaceAll(string str)
        {
            bool matches = false;
            List<int> startPos = new List<int>();
            foreach (Match match in Regex.Matches(str, rawPattern))
            {
                startPos.Add(match.Groups[0].Index);
                token.Add(match.Value);
                matches = true;
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
            if (matches)
                return newStr;
            else
                return str;
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
            return token[idFromName(id)];
        }

        public TokenManager getManager(string key)
        {
            string type = Regex.Match(key, @"(?<=key).").Value;
            switch (type)
            {
                case "%":
                    return KeyManager.Instance();
                case "m":
                    return MultiKeyManager.Instance();
                case "c":
                    return KeyCombinationManager.Instance();
                case "a":
                    return KeyAxisManager.Instance();
            }
            return null;
        }

        protected VirtualKeyCode keyToCode(string key)
        {
            string keyCode = key.Length == 1 ? "VK_" + key : key;
            return (VirtualKeyCode)System.Enum.Parse(typeof(VirtualKeyCode), keyCode, true);
        }

        protected int idFromName(string name)
        {
            return Convert.ToInt16(Regex.Match(name, @"(?<=\$)\d+").Value);
        }
    }

    class KeyManager : TokenManager
    {
        private static KeyManager instance;
        public static KeyManager Instance()
        {
            if (instance == null)
                instance = new KeyManager();
            return instance;
        }
        private KeyManager()
        { }

        public override TokenManager getHighestLevel()
        {
            if (token.Count > 0)
                return this;
            else
                return null;
        }

        public override void buildExecutorTree(string keyName, string id, Dictionary<string, KeyExecutor> executors)
        {
            executors.Add(id, new Key(keyToCode(getElement(keyName))));
        }

        public override List<VirtualKeyCode> getKeys(int id)
        {
            return new List<VirtualKeyCode>() { keyToCode(token[id]) };
        }

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
        private static MultiKeyManager instance;
        public static MultiKeyManager Instance()
        {
            if (instance == null)
                instance = new MultiKeyManager();
            return instance;
        }
        private MultiKeyManager()
        { }

        public override TokenManager getHighestLevel()
        {
            if (token.Count > 0)
                return this;
            else
                return KeyManager.Instance().getHighestLevel();
        }

        public override void buildExecutorTree(string keyName, string id, Dictionary<string, KeyExecutor> executors)
        {
            List<VirtualKeyCode> codes = new List<VirtualKeyCode>();
            string[] keys = getElement(keyName).Split('+');
            foreach (string key in keys)
            {
                codes.Add(keyToCode(KeyManager.Instance().getElement(key)));
            }
            executors.Add(id, new MultiKey(codes));
        }

        public override List<VirtualKeyCode> getKeys(int id)
        {
            List<VirtualKeyCode> codes = new List<VirtualKeyCode>();
            string[] keys = token[0].Split('+');
            foreach (string key in keys)
            {
                codes.Add(keyToCode(KeyManager.Instance().getElement(key)));
            }
            return codes;
        }

        protected override string idKey
        {
            get { return "keym"; }
        }
        protected override string rawPattern
        {
            get { return KeyManager.idPattern + @"(?:\+" + KeyManager.idPattern + ")+"; }
        }
        public static string idPattern = @"\$\d+keym%";
    }

    class KeyCombinationManager : TokenManager
    {
        private static KeyCombinationManager instance;
        public static KeyCombinationManager Instance()
        {
            if (instance == null)
                instance = new KeyCombinationManager();
            return instance;
        }
        private KeyCombinationManager()
        { }

        public override TokenManager getHighestLevel()
        {
            if (token.Count > 0)
                return this;
            else
                return MultiKeyManager.Instance().getHighestLevel();
        }

        public override void buildExecutorTree(string keyName, string id, Dictionary<string, KeyExecutor> executors)
        {
            string[] keys = getElement(keyName).Split(',');
            executors.Add(id, new KeyCombination(getManager(keys[0]).getKeys(idFromName(keys[0])), getManager(keys[1]).getKeys(idFromName(keys[1]))));
        }

        public override List<VirtualKeyCode> getKeys(int id)
        {
            throw new NotImplementedException();
        }

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
        private static KeyAxisManager instance;
        public static KeyAxisManager Instance()
        {
            if (instance == null)
                instance = new KeyAxisManager();
            return instance;
        }
        private KeyAxisManager()
        { }

        public override TokenManager getHighestLevel()
        {
            if (token.Count > 0)
                return this;
            else
                return KeyCombinationManager.Instance().getHighestLevel();
        }

        public override void buildExecutorTree(string keyName, string id, Dictionary<string, KeyExecutor> executors)
        {
            string[] keys = getElement(keyName).Split('|');
            getManager(keys[0]).buildExecutorTree(keys[0], id, executors);
            getManager(keys[1]).buildExecutorTree(keys[1], "-" + id, executors);
        }

        public override List<VirtualKeyCode> getKeys(int id)
        {
            throw new NotImplementedException();
        }

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
        VirtualKeyCode key;
        public Key(VirtualKeyCode vk)
        {
            key = vk;
        }
        public override void keysDown(InputSimulator inSim)
        {
            if (key == VirtualKeyCode.LBUTTON)
                inSim.Mouse.LeftButtonDown();
            else if (key == VirtualKeyCode.RBUTTON)
                inSim.Mouse.RightButtonDown();
            else
                inSim.Keyboard.KeyDown(key);
        }

        public override void keysUp(InputSimulator inSim)
        {
            if (key == VirtualKeyCode.LBUTTON)
                inSim.Mouse.LeftButtonUp();
            else if (key == VirtualKeyCode.RBUTTON)
                inSim.Mouse.RightButtonUp();
            else
                inSim.Keyboard.KeyUp(key);
        }
    }

    class MultiKey : KeyExecutor
    {
        List<VirtualKeyCode> keys;
        public MultiKey(List<VirtualKeyCode> vks)
        {
            keys = vks;
        }
        public override void keysDown(InputSimulator inSim)
        {
            foreach (VirtualKeyCode key in keys)
                inSim.Keyboard.KeyDown(key);
        }

        public override void keysUp(InputSimulator inSim)
        {
            foreach (VirtualKeyCode key in keys)
                inSim.Keyboard.KeyUp(key);
        }
    }

    class KeyCombination : KeyExecutor
    {
        List<VirtualKeyCode> primary;
        List<VirtualKeyCode> secondary;

        public KeyCombination(List<VirtualKeyCode> prim, List<VirtualKeyCode> scnd)
        {
            primary = prim;
            secondary = scnd;
        }
        public override void keysDown(InputSimulator inSim)
        {
            inSim.Keyboard.ModifiedKeyStroke(primary, secondary);
        }

        public override void keysUp(InputSimulator inSim) { }
    }
}
