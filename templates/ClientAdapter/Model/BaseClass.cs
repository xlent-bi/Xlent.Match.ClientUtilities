using System;
using System.Collections.Generic;

namespace ClientAdapter.Model
{
    public class BaseClass
    {
        public int Id { get; private set; }

        public BaseClass(int id)
        {
            Id = id;
        }

        public override bool Equals(object other)
        {
            var key = other as Customer;
            if (key == null) return false;

            return key.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}", Id);
        }
    }
}
