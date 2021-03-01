using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DigBuild.Platform.Util
{
    internal static class FormatDescriptor<T> where T : unmanaged
    {
        public static readonly FormatDescriptor Instance = FormatDescriptor.Get<T>();
    }

    internal sealed class FormatDescriptor
    {
        public readonly Element[] Elements;

        private FormatDescriptor(Element[] elements)
        {
            Elements = elements;
        }

        internal static FormatDescriptor Get<T>() where T : unmanaged
        {
            var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);
            Element[] elements = new Element[fields.Length];

            uint i = 0;
            foreach (var field in fields)
            {
                elements[i].Type = NumericTypeHelper.GetType(field.FieldType);
                elements[i].Offset = (uint) Marshal.OffsetOf<T>(field.Name).ToInt32();
                i++;
            }

            // TODO: possibly support overlaying types
            Array.Sort(elements, (a, b) => a.Offset.CompareTo(b.Offset));
            for (uint j = 0; j < elements.Length; j++)
                elements[j].Location = j;

            return new FormatDescriptor(elements);
        }
        
        internal struct Element
        {
            internal uint Location;
            internal NumericType Type;
            internal uint Offset;
        }
    }
}