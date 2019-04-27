﻿using Chauffeur.Services.Interfaces;
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
        public List<CreatedPackage> GetAllCreatedPackages()
        {
            return CreatedPackage.GetAllCreatedPackages();
        }

        public CreatedPackage GetById(int id)
        {
            return CreatedPackage.GetById(id);
        }
    }
}
