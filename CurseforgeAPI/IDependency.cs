using System;

namespace CurseForgeAPI
{
    public interface IDependency : IEquatable<IDependency>
    {
        public IMod Mod { get; }
        public DependencyType DependencyType { get; }
    }
}