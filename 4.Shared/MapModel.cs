using System;
using System.Collections.Generic;
using System.Reflection;


namespace PT.Shared
{
    public class MapModel<O> where O : new()
    {
        public static O Go<I>(I input)
        {
            var result = new O();
            try
            {
                IList<PropertyInfo> iProps = new List<PropertyInfo>(input.GetType().GetProperties());
                IList<PropertyInfo> oProps = new List<PropertyInfo>(result.GetType().GetProperties());
                foreach (PropertyInfo prop in iProps)
                {
                    foreach (PropertyInfo item in oProps)
                    {
                        if (prop.Name == item.Name)
                        {
                            if (item.PropertyType.FullName.Contains(prop.PropertyType.Name) || prop.PropertyType.FullName.Contains(item.PropertyType.Name) || prop.PropertyType.FullName == item.PropertyType.Name)
                            {
                                item.SetValue(result, prop.GetValue(input, null));
                            }
                            continue;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                string a = ex.ToString();
            }
            return default(O);
        }
    }
}
