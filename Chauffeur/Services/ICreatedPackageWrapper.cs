using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco.cms.businesslogic.packager;

namespace Chauffeur.Services
{
    public interface ICreatedPackageWrapper
    {
        List<CreatedPackage> GetAllCreatedPackages();
        CreatedPackage GetById(int id);
    }
}
