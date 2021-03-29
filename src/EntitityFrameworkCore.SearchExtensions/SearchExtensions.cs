﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using static System.Linq.Expressions.Expression;

namespace EntitityFrameworkCore.SearchExtensions
{
    public static class SearchExtensions
    {
        public static IQueryable<T> Search<T, TProperty>(
            this IQueryable<T> source,
            Expression<Func<T, TProperty>> properptySelector,
            string searchTerm)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (properptySelector is null || string.IsNullOrEmpty(searchTerm))
            {
                return source;
            }

            searchTerm = $"%{searchTerm}%";
            var itemParameter = Parameter(typeof(T), "item");

            var functions = Property(null, typeof(EF).GetProperty(nameof(EF.Functions)));
            var like = typeof(DbFunctionsExtensions)
                .GetMethod(nameof(DbFunctionsExtensions.Like), new Type[] { functions.Type, typeof(string), typeof(string) });

            Expression expressionProperty = properptySelector;

            if (typeof(TProperty) != typeof(string))
            {
                expressionProperty = Call(
                    expressionProperty,
                    typeof(object).GetMethod(nameof(object.ToString), Array.Empty<Type>()));
            }

            var selector = Call(
                       null,
                       like,
                       functions,
                       expressionProperty,
                       Constant(searchTerm));

            return source.Where(Lambda<Func<T, bool>>(selector, itemParameter));
        }

        public static IQueryable<T> Search<T>(this IQueryable<T> source, string propertyName, string searchTerm)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(searchTerm))
            {
                return source;
            }

            var property = typeof(T).GetProperty(propertyName);

            if (property is null)
            {
                return source;
            }

            searchTerm = $"%{searchTerm}%";
            var itemParameter = Parameter(typeof(T), "item");

            var functions = Property(null, typeof(EF).GetProperty(nameof(EF.Functions)));
            var like = typeof(DbFunctionsExtensions)
                .GetMethod(nameof(DbFunctionsExtensions.Like), new Type[] { functions.Type, typeof(string), typeof(string) });

            Expression expressionProperty = Property(itemParameter, property.Name);

            if (property.PropertyType != typeof(string))
            {
                expressionProperty = Call(
                    expressionProperty,
                    typeof(object).GetMethod(nameof(object.ToString), Array.Empty<Type>()));
            }

            var selector = Call(
                       null,
                       like,
                       functions,
                       expressionProperty,
                       Constant(searchTerm));

            return source.Where(Lambda<Func<T, bool>>(selector, itemParameter));
        }

        public static IQueryable<T> Search<T>(this IQueryable<T> source, string searchTerm)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrEmpty(searchTerm))
            {
                return source;
            }

            var properties = typeof(T).GetProperties();

            if (properties.Length == 0)
            {
                return source;
            }

            searchTerm = $"%{searchTerm}%";

            Expression selector = null;

            var itemParameter = Parameter(typeof(T), "item");

            foreach (var property in properties)
            {
                var functions = Property(null, typeof(EF).GetProperty(nameof(EF.Functions)));
                var like = typeof(DbFunctionsExtensions)
                    .GetMethod(
                        nameof(DbFunctionsExtensions.Like),
                        new Type[]
                        {
                            functions.Type,
                            typeof(string),
                            typeof(string)
                        });

                Expression expressionProperty = Property(itemParameter, property.Name);

                if (property.PropertyType != typeof(string))
                {
                    expressionProperty = Call(
                        expressionProperty,
                        typeof(object).GetMethod(nameof(object.ToString), Array.Empty<Type>()));
                }

                var newSelector = Call(
                    null,
                    like,
                    functions,
                    expressionProperty,
                    Constant(searchTerm));

                if (selector is null)
                {

                    selector = newSelector;
                }
                else
                {
                    selector = Or(selector, newSelector);
                }
            }

            return source.Where(Lambda<Func<T, bool>>(selector, itemParameter));
        }
    }
}