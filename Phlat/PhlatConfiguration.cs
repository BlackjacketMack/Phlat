using System;
using System.Collections.Generic;
using System.Reflection;

namespace Phlatware
{
    public class PhlatConfiguration
    {
        private Dictionary<Type, IPhlatType> _typeRegistry = new Dictionary<Type, IPhlatType>();

        public PhlatConfiguration()
        {

        }

        public PhlatType<T> Configure<T>(Action<T, T> update = null)
        {
            var config = new PhlatType<T>
            {
                Update = update
            };

            if(_typeRegistry.ContainsKey(typeof(T)))
                throw new ApplicationException("That config already exists.");

            _typeRegistry.Add(typeof(T), config);

            return config;
        }

        public IPhlatType GetPhlatType(Type type)
        {
            if (!_typeRegistry.TryGetValue(type, out IPhlatType val))
                throw new ApplicationException("The type you specified is not registerd.");

            return val;
        }

        public PhlatType<T> GetPhlatType<T>() => GetPhlatType(typeof(T)) as PhlatType<T>;
    }
}