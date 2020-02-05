using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcademenu
{
    class InputManager
    {
        private readonly HashSet<Keys> usedKeys = new HashSet<Keys> { Keys.Up, Keys.Down, Keys.Enter };
        private HashSet<Keys> lastTickKeys;
        private const float minDelay = 0.05f;
        private const float defaultDelay = 0.5f;
        private float currentDelay = defaultDelay;
        private float deltaTime = 0;
        private int direction = 0;

        public InputManager Initialize()
        {
            lastTickKeys = new HashSet<Keys>();
            return this;
        }

        public void Update(float _gameTime)
        {
            HashSet<Keys> thisTickKeys = new HashSet<Keys>();

            foreach (Keys key in Keyboard.GetState().GetPressedKeys())
            {
                //if key is not mapped, throw it away
                if (!usedKeys.Contains(key))
                    continue;
                //if key is mapped save it for next tick
                thisTickKeys.Add(key);
                //if key was pressed last tick, remove it from the old list (so we don't have to worry about it later)
                if (lastTickKeys.Contains(key))
                    lastTickKeys.Remove(key);
                else //if not it must have been pressed just now!!
                    Pressed(key, true);
            }

            //any key left appearently was pressed last tick but not anymore
            lastTickKeys.ForEach(key => Pressed(key, false));

            //set up all the keys that have been pressed this tick for next tick
            lastTickKeys = thisTickKeys;
        }

        private void Pressed(Keys key, bool down)
        {
            if (down)
            {
                switch (key)
                {
                    case Keys.Up:
                        direction--;
                        Menu.instance.Scroll(direction);
                        break;
                    case Keys.Down:
                        direction++;
                        Menu.instance.Scroll(direction);
                        break;
                    case Keys.Left:
                        direction--;
                        Menu.instance.Scroll(direction);
                        break;
                    case Keys.Right:
                        direction++;
                        Menu.instance.Scroll(direction);
                        break;
                    case Keys.Enter:
                        break;
                    default:
                        throw new Exception("Key \"" + key + "\" is not mapped");
                }
            }
            else
            {
                switch (key)
                {
                    case Keys.Up:
                        direction++;
                        currentDelay = defaultDelay;
                        deltaTime = 0;
                        break;
                    case Keys.Down:
                        direction--;
                        currentDelay = defaultDelay;
                        deltaTime = 0;
                        break;
                    case Keys.Enter:
                        Menu.instance.RunExe();
                        break;
                    default:
                        throw new Exception("Key \"" + key + "\" is not mapped");
                }
            }
        }
    }
}