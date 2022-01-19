using System;
using System.Collections.Generic;

namespace Phlatware
{
    public class PhlatConfiguration
    {
        public Dictionary<Type, IPhlatType> Registry { get; private set; } = new Dictionary<Type, IPhlatType>();

        public PhlatType<T> Configure<T>(Action<T, T> update)
        {
            var config = new PhlatType<T>
            {
                Update = update
            };

            if(Registry.ContainsKey(typeof(T)))
                throw new ApplicationException("That config already exists.");

            Registry.Add(typeof(T), config);

            return config;
        }
    }
}