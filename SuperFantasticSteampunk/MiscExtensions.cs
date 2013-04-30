﻿using System;
using System.ComponentModel;

namespace SuperFantasticSteampunk
{
    static class MiscExtensions
    {
        public static T Tap<T>(this T self, Action<T> action)
        {
            action(self);
            return self;
        }

        public static void WriteLineProperties(this Object self)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(self))
                Console.WriteLine("{0}={1}", descriptor.Name, descriptor.GetValue(self));
        }
    }
}