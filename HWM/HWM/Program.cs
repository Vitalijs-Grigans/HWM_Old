using HWM.Parser;

namespace HWM
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new LeaderGuildParser();

            parser.CollectData();
        }
    }
}
