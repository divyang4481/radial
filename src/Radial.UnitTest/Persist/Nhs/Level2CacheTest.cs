﻿using NUnit.Framework;
using Radial.Persist;
using Radial.Persist.Nhs;
using Radial.UnitTest.Persist.Nhs.Domain;
using Radial.UnitTest.Persist.Nhs.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radial.UnitTest.Persist.Nhs
{
    [TestFixture]
    public class Level2CacheTest
    {
        [Test]
        public void Test1()
        {
            //User u = new User { Id = RandomCode.NewInstance.Next(1, int.MaxValue), Name = "测试" };

            //using (IUnitOfWork uow = new NhUnitOfWork())
            //{
            //    uow.RegisterNew<User>(u);

            //    uow.Commit();
            //}

            using (IUnitOfWork uow = new NhUnitOfWork())
            {
                UserRepository usrRepo = new UserRepository(uow);
                int total=0;
                var us = usrRepo.FindAll(10, 1, out total);

                Console.Write(total);
            }

            User u = new User { Id = RandomCode.NewInstance.Next(1, int.MaxValue), Name = "测试" };

            using (IUnitOfWork uow = new NhUnitOfWork())
            {
                uow.RegisterNew<User>(u);

                uow.Commit();
            }

            using (IUnitOfWork uow = new NhUnitOfWork())
            {
                UserRepository usrRepo = new UserRepository(uow);
                int total = 0;
                var us = usrRepo.FindAll(10, 1, out total);

                Console.Write(total);
            }

            //using (IUnitOfWork uow = new NhUnitOfWork())
            //{
            //    UserRepository usrRepo = new UserRepository(uow);
            //    var us = usrRepo[123425828];
            //}
        }
    }
}
