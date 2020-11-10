using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Moonrise.Utils.Database.Extensions
{
    /// <summary>
    /// Extension methods for an Entitity Framework context
    /// </summary>
    public static class ContextExtensions
    {
        //// https://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record/15339512#15339512
        //public static void AddOrUpdate(this DbContext ctx, object entity)
        //{
        //    DbEntityEntry entry = ctx.Entry(entity);

        //    switch (entry.State)
        //    {
        //        case EntityState.Detached:
        //            ctx.Add(entity);
        //            break;
        //        case EntityState.Modified:
        //            ctx.Update(entity);
        //            break;
        //        case EntityState.Added:
        //            ctx.Add(entity);
        //            break;
        //        case EntityState.Unchanged:

        //            //item already in db no need to do anything  
        //            break;

        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}

        /// <summary>
        /// Adds an entity if it is not already there. IT DOES NOT DO AN UPDATE!<para>
        /// NOTE: It doesn't handle linked entities!
        /// </para>
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entities">The entity set being extended.</param>
        /// <param name="addition">The entity to add.</param>
        /// <param name="keys">The key(s) - listed in key order if more than one!</param>
        /// <returns>An indication as to whether it was added or not.</returns>
        public static bool AddIfNotThere<T>(this DbSet<T> entities, T addition, params object[] keys)
            where T : class
        {
            bool retVal = false;

            if (entities.Find(keys) == null)
            {
                entities.AddRange(new[] { addition });
                retVal = true;
            }

            return retVal;
        }

        /// <summary>
        /// Updates an entity, or adds it if it is not already there.<para>
        /// NOTE: It doesn't handle linked entities!
        /// </para>
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entities">The entity set being extended.</param>
        /// <param name="context">The context the entity set is in.</param>
        /// <param name="item">The entity to add or update.</param>
        /// <param name="keys">The key(s) - listed in key order if more than one!</param>
        /// <returns>
        /// An indication as to whether it was added (true) or not.
        /// </returns>
        public static bool AddOrUpdate<T>(this DbSet<T> entities, DbContext context, T item, params object[] keys)
            where T : class
        {
            bool retVal = false;

            T entity = entities.Find(keys);

            if (entity == null)
            {
                entities.AddRange(new[] { item });
                retVal = true;
            }
            else
            {
                context.Entry(entity).State = EntityState.Detached;
                entities.Attach(item);
                context.Entry(item).State = EntityState.Modified;
            }

            return retVal;
        }
    }
}
