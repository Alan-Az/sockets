using Eto.Forms;

namespace ClienteGUI;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var platform = Eto.Platform.Detect;
        new Application(platform).Run(new FormCliente());
    }
}
