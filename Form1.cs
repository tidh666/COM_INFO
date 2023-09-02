using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace COM_INFO


{
    public partial class Form1 : Form
    {
        private Timer timer;
        private List<string> currentPorts;

        public Form1()
        {
            InitializeComponent();

            // Inicializar el Timer para actualizar la información periódicamente
            timer = new Timer();
            timer.Interval = 5000; // Actualiza cada 5 segundos (ajusta el valor según tus necesidades)
            timer.Tick += Timer_Tick;
            timer.Start();

            // Inicializar la lista de puertos actuales
            currentPorts = GetAvailablePorts().ToList();

            // Agregar un evento para mostrar el menú contextual al hacer clic derecho
            COM_info.MouseClick += NotifyIcon1_MouseClick;

            // Crear el menú contextual
            CreateContextMenu();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Obtener los puertos COM disponibles actualmente
            List<string> newPorts = GetAvailablePorts().ToList();

            // Comparar los puertos antes y después del cambio
            var addedPorts = newPorts.Except(currentPorts);
            var removedPorts = currentPorts.Except(newPorts);

            // Actualizar la lista de puertos actuales
            currentPorts = newPorts;

            // Mostrar los cambios en el icono de la bandeja del sistema
            string portInfo = "Puertos COM disponibles:" + Environment.NewLine;
            portInfo += string.Join(Environment.NewLine, newPorts);

            foreach (string port in addedPorts)
            {
                portInfo += " - NUEVO";
            }

            COM_info.Text = portInfo;
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            // Mostrar el menú contextual al hacer clic derecho
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(Cursor.Position);
            }
            // Restaurar el formulario al hacer clic izquierdo
            else if (e.Button == MouseButtons.Left)
            {
                //this.WindowState = FormWindowState.Normal;
                //this.ShowInTaskbar = true;
            }
        }

        private void CreateContextMenu()
        {
            // Crear el menú contextual
            contextMenuStrip1 = new ContextMenuStrip();

            // Agregar un elemento de menú "Salir"
            var menuItemSalir = new ToolStripMenuItem("Salir");
            menuItemSalir.Click += MenuItemSalir_Click;
            contextMenuStrip1.Items.Add(menuItemSalir);

            // Asignar el menú contextual al NotifyIcon
            COM_info.ContextMenuStrip = contextMenuStrip1;
        }

        private void MenuItemSalir_Click(object sender, EventArgs e)
        {
            // Manejar el clic en "Salir" para cerrar la aplicación
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ocultar el formulario en lugar de cerrarlo
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private IEnumerable<string> GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}
