using System.Threading.Tasks;

namespace Aurelio.Public.Classes.Interfaces;

public interface IAurelioRequestableClosePage
{
    Task<bool> RequestClose(object? sender);
}