using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace pryDiesenberg_ArregloDeProyecto
{
    public partial class frmCargarProveedor : Form
    {
        public frmCargarProveedor()
        {
            InitializeComponent();
            btnGuardar.Enabled = false;

            string defaultPath = Path.Combine(Application.StartupPath, "Resources", "Proveedores");
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            fbdSeleccionCarpeta.SelectedPath = defaultPath;
        }

        private void btnSeleccionCarpeta_Click(object sender, EventArgs e)
        {
            // Mostrar diálogo y usar su resultado
            DialogResult result = fbdSeleccionCarpeta.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrEmpty(fbdSeleccionCarpeta.SelectedPath))
            {
                lblDireccion.Text = fbdSeleccionCarpeta.SelectedPath;
                btnGuardar.Enabled = true;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreArchivo.Text))
            {
                MessageBox.Show("Ingresa un nombre de archivo válido.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ruta seleccionada y nombre de archivo
            string carpeta = fbdSeleccionCarpeta.SelectedPath;
            string nombreArchivo = txtNombreArchivo.Text.Trim() + ".csv";

            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            try
            {
                // Crear/reescribir el archivo con encabezados
                using (StreamWriter ManejoArchivo = new StreamWriter(rutaCompleta, false, Encoding.UTF8))
                {
                    ManejoArchivo.Write("N°;");
                    ManejoArchivo.Write("Entidad;");
                    ManejoArchivo.Write("APERTURA;");
                    ManejoArchivo.Write("N° EXPTE.;");
                    ManejoArchivo.Write("JUZG.;");
                    ManejoArchivo.Write("JURISD.;");
                    ManejoArchivo.Write("DIRECCION;");
                    ManejoArchivo.WriteLine("LIQUIDADOR RESPONSABLE");
                }

                MessageBox.Show("Archivo creado en:\n" + rutaCompleta, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblDireccion.Text = "";
                txtNombreArchivo.Clear();
                btnGuardar.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear el archivo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmCargarProveedor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Application.Exit();
            }
        }

        private void frmCargarProveedor_Load(object sender, EventArgs e)
        {

        }
    }
}
