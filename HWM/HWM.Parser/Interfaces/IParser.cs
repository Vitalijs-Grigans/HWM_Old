using System.Threading.Tasks;

namespace HWM.Parser.Interfaces
{
    public interface IParser
    {
        public Task CollectData();

        public Task ProcessData();
    }
}
