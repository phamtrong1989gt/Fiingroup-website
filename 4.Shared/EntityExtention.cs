using System;
using System.Linq;


namespace PT.Shared
{
    public class EntityExtention<T> where T : class
    {
        public static Func<IQueryable<T>, IOrderedQueryable<T>> OrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> a)
        {
            return a;
        }
    }
}
