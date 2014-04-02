using System.IO.Abstractions;
namespace Chauffeur.DependencyBuilders
{
    class FileSystemBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<IFileSystem>(() => new FileSystem());
        }
    }
}
