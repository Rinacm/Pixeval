using System.Threading.Tasks;

namespace Mako.Console
{
    public static class Program
    {
        public static async Task Main()
        {
            var makoClient = new MakoClient("account", "password");
            await makoClient.Login();
            await foreach (var illustration in makoClient.Gallery(makoClient.ContextualBoundedSession.Id, RestrictionPolicy.Public))
            {
                System.Console.WriteLine(illustration.Title);
            }
        }
    }
}