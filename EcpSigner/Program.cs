using System.Threading.Tasks;

namespace EcpSigner
{
    class Program
    {
        static void Main(string[] args)
        {
            var signer = new Signer();
            Task.Run(async () => await signer.DoWork(args)).Wait();
        }
    }
}
