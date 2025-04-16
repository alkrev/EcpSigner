using System.Threading.Tasks;

namespace EcpSigner
{
    class Program
    {
        static void Main(string[] args)
        {
            var signer = new Signer(args);
            Task.Run(async () => await signer.Run()).Wait();
        }
    }
}
