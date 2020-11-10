using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Moonrise.Utils.Database.Extensions
{
    public static class ContextExtensions
    {
        // https://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record/15339512#15339512
        public static void AddOrUpdate(this DbContext ctx, object entity, )
        {
            DbEntityEntry entry = ctx.Entry(entity);

            switch (entry.State)
            {
                case EntityState.Detached:
                    ctx.Add(entity);
                    break;
                case EntityState.Modified:
                    ctx.Update(entity);
                    break;
                case EntityState.Added:
                    ctx.Add(entity);
                    break;
                case EntityState.Unchanged:

                    //item already in db no need to do anything  
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
