using FestivalManagementSystem.Controller;
using FestivalManagementSystem.Data;
using FestivalManagementSystem.Interfaces;
using FestivalManagementSystem.Services;
using FestivalManagementSystem.View;

namespace FestivalManagementSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPersonRepository repository = new MySqlPersonRepository(DbConfig.ConnectionString);
            PersonService service = new PersonService(repository);
            ConsoleView view = new ConsoleView();
            PersonController controller = new PersonController(service, view);

            controller.Run();
        }
    }
}