using System.Windows.Forms;

namespace ClienteGUI;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormCliente());
    }
}
