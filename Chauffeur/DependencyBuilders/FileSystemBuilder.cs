using System.IO.Abstractions;

namespace Chauffeur.DependencyBuilders
{
    class FileSystemBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register<FileSystem, IFileSystem>();
        }
    }
}
