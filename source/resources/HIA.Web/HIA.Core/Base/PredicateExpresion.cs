using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace HIA.Core.Base
{
    public static class PredicateExpresion<T>
    {
        static public Expression<Func<T, bool>> BuildExpressionForFilter(Expression<Func<T, bool>> filterSearch, PropertyInfo propertyInfo, string key) 
        {
            if (propertyInfo.PropertyType == typeof(String))
            {
                filterSearch = filterSearch.Or(x => EF.Property<string>(x, propertyInfo.Name).ToLower().Contains(key));
            }
            else if (propertyInfo.PropertyType == typeof(Int32))
            {
                filterSearch = filterSearch.Or(x => EF.Property<int>(x, propertyInfo.Name).ToString().Contains(key));
            }
            else if (propertyInfo.PropertyType == typeof(Nullable<Int32>))
            {
                filterSearch = filterSearch.Or(x => EF.Property<Nullable<Int32>>(x, propertyInfo.Name).Value.ToString().Contains(key));
            }       
            else if (propertyInfo.PropertyType == typeof(Nullable<Double>))
            {
                filterSearch = filterSearch.Or(x => EF.Property<Nullable<Double>>(x, propertyInfo.Name).Value.ToString().Contains(key));
            }
            else if (propertyInfo.PropertyType == typeof(Nullable<DateTime>))
            {
                filterSearch = filterSearch.Or(x => EF.Property<Nullable<DateTime>>(x, propertyInfo.Name).Value.ToString().Contains(key));
            }
            else if (propertyInfo.PropertyType == typeof(DateTime))
            {
                filterSearch = filterSearch.Or(x => EF.Property<DateTime>(x, propertyInfo.Name).ToString().Contains(key));
            }

            return filterSearch;
        }
    }
}
