using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Phlatware
{
    /// <summary>
    /// This is an awesome implementation that includes the 'OldValue' of a change.
    /// </summary>
    public class SnapshotT_UsingEmit<T> : ISnapshot
    {
        static Func<T, IDictionary<string,object>> cloner;
        private T _original;
        private IDictionary<string, object> _startValues;

        public IDictionary<string, object> Start()
        {
            _startValues = Values();

            return _startValues;
        }

        public IDictionary<string, object> Values()
        {
            return GetValues();
        }

        public SnapshotT_UsingEmit(T original)
        {
            _original = original;
        }

        public IDictionary<string,object> GetValues()
        {
            cloner = cloner ?? GenerateCloner();
            return cloner(_original);
        }

        private static bool areEqual(object first, object second)
        {
            if (first == null && second == null) return true;
            if (first == null) return false;
            return first.Equals(second);
        }

        private static List<PropertyInfo> RelevantProperties()
        {
            return typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p =>
                    p.GetSetMethod(true) != null &&
                    p.GetGetMethod(true) != null &&
                    (
                    //no focus on the property type
                    p.PropertyType.IsValueType ||

                    //strings are fine
                    p.PropertyType == typeof(string) ||

                    //nullabletypes are fine
                    (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    )
                    ).ToList();
        }

        public static Dictionary<string,object> SampleDictionaryAddToCopy()
        {
            var dict = new Dictionary<string, object>();

            var key = "Name";
            var value = "Bill";

            dict.Add(key, value);

            return dict;
        }
        
        // adapted from http://stackoverflow.com/a/966466/17174
        private static Func<T, Dictionary<string,object>> GenerateCloner()
        {
            var dictType = typeof(Dictionary<string, object>);
            var dm = new DynamicMethod("DoClone", dictType, new Type[] { typeof(T) }, true);
            var ctor = dictType.GetConstructor(new Type[] { });
            var il = dm.GetILGenerator();

            il.DeclareLocal(dictType);

            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc_0);

            foreach (var prop in RelevantProperties())
            {
                il.Emit(OpCodes.Ldloc_0);

                //get name
                il.Emit(OpCodes.Ldstr, prop.Name);

                // [clone]
                il.Emit(OpCodes.Ldarg_0);

                // [clone, source]
                il.Emit(OpCodes.Callvirt, prop.GetGetMethod(true));

                //box if not string
                if (prop.PropertyType != typeof(string))
                {
                    il.Emit(OpCodes.Box, prop.PropertyType);
                }

                il.Emit(OpCodes.Callvirt, dictType.GetMethod("Add",new[] { typeof(string),typeof(object) }));
            }

            // Load new constructed obj on eval stack -> 1 item on stack
            il.Emit(OpCodes.Ldloc_0);
            // Return constructed object.   --> 0 items on stack
            il.Emit(OpCodes.Ret);

            var myExec = dm.CreateDelegate(typeof(Func<T, Dictionary<string, object>>));

            return (Func<T, Dictionary<string, object>>)myExec;
        }
    }
}
