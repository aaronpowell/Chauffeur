using Chauffeur.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco.cms.businesslogic.packager;

namespace Chauffeur.Services
{
    class CreatedPackageService : ICreatedPackageService
    {
        CreatedPackage _package;

        public PackageInstance Data { get => _package.Data; set => throw new NotImplementedException(); }

        public List<CreatedPackage> GetAllCreatedPackages()
        {
            return CreatedPackage.GetAllCreatedPackages();
        }

        public void Publish()
        {
            _package.Publish();
        }

        //public CreatedPackage GetById(int id)
        //{
        //    return CreatedPackage.GetById(id);
        //}

        ICreatedPackageService ICreatedPackageService.GetById(int id)
        {
            _package = CreatedPackage.GetById(id);
            return this;
        }


    }
}
