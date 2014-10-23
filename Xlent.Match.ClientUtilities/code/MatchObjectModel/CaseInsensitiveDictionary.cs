using System;
using System.Collections;
using System.Collections.Generic;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{
    public class CaseInsensitiveDictionary<T> : Dictionary<string, T>
    {
        public CaseInsensitiveDictionary()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        public CaseInsensitiveDictionary(IDictionary<string, T> caseInsensitiveDictionary)
            : base(caseInsensitiveDictionary.Count, StringComparer.InvariantCultureIgnoreCase)
        {
            foreach (var keyValuePair in caseInsensitiveDictionary)
            {
                this.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public CaseInsensitiveDictionary(int size)
            : base(size, StringComparer.InvariantCultureIgnoreCase)
        {
        }
    }
}