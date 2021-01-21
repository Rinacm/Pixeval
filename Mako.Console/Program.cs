using System.Collections.Generic;
using System.Threading.Tasks;
using Mako.Model;

namespace Mako.Console
{
    public static class Program
    {
        public static async Task Main()
        {
            var makoClient = new MakoClient("account", "password");
            await makoClient.Login();
            var list = new List<Illustration>();
            await foreach (var illustration in makoClient.Gallery(makoClient.ContextualBoundedSession.Id, RestrictionPolicy.Private))
            {
                list.Add(illustration);
                if (illustration != null) System.Console.WriteLine(illustration.Title);
            }

            System.Console.WriteLine(list.Count);
        }
    }
}