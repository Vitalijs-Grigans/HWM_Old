using System.Threading.Tasks;

namespace HWM.Parser.Interfaces
{
    public interface IParser
    {
        public Task CollectDataAsync();

        public Task ProcessDataAsync();
    }
}
