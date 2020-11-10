using System;
using System.Collections.Generic;
using CodeFirstMigration;
using Moonrise.Utils.Database.Migrations;
using SampleContext;
using SampleDomainModel;

namespace SampleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                MigrationState<SweetContext4Migration, SweetContextConfiguration>.CheckDatabase();

                using (SweetContext ctx = new SweetContext())
                {
                    ChocolateBox box = new ChocolateBox
                                       {
                                           Name = "Milk Tragic",
                                           Contents = new List<Chocolate>
                                                      {
                                                          new Chocolate
                                                          {
                                                              Name = "Fudge Square"
                                                          },
                                                          new Chocolate {Name = "Orange Cream"}
                                                      }
                                       };

                    ctx.ChocolateBoxes.Add(box);
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadKey();
        }
    }
}
