using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pryDiesenberg_ArregloDeProyecto
{
    public partial class frmClientes : Form
    {
        clsBaseDatosCliente objBaseDatosCliente;

        public frmClientes()
        {
            InitializeComponent();
        }

        private void frmClientes_Load(object sender, EventArgs e)
        {
            objBaseDatosCliente = new clsBaseDatosCliente();
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            dgvCliente.Rows.Clear();
            dgvCliente.Columns.Clear();
            objBaseDatosCliente.TraerDatos(dgvCliente);
            lblEstadoConexion.Text = "Conectado";
            lblEstadoConexion.BackColor = Color.Green;
        }

        // BOTÓN BUSCAR
        private void btnLocura_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                MessageBox.Show("Ingresá un código para buscar.", "Campo vacío",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtBuscar.Text, out int codigo))
            {
                MessageBox.Show("El código debe ser un número.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            objBaseDatosCliente.BuscarPorID(codigo);
        }

        // BOTÓN MODIFICAR ACTIVIDAD
        private void btnActividad_Click(object sender, EventArgs e)
        {
            if (dgvCliente.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccioná un cliente de la grilla.", "Sin selección",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object codigoSocioValue = dgvCliente.SelectedRows[0].Cells["CODIGO_SOCIO"].Value;

            if (!int.TryParse(codigoSocioValue?.ToString(), out int codigoSocio))
            {
                MessageBox.Show("No se pudo obtener el código del cliente.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            objBaseDatosCliente.actividadCliente(codigoSocio);
            CargarGrilla();

            MessageBox.Show("Actividad modificada correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void frmClientes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                this.Close(); // Cierra solo este form, no toda la app
            }
        }
    }
}
