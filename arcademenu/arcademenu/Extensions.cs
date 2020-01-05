﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arcademenu
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static float DeltaTime(this GameTime _gameTime)
        {
            return (float)_gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
