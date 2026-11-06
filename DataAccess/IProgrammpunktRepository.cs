using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public interface IProgrammpunktRepository : IRepository<Programmpunkt>
{
    List<Programmpunkt> GetByReise(int reiseId);
}
