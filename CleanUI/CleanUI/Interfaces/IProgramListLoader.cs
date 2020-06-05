using System.Collections.Generic;

namespace CleanUI.Interfaces
{
    public interface IProgramListLoader
    {
        IEnumerable<string> GetProgramList();
    }
}