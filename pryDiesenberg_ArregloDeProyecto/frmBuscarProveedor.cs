using System;
using System.IO;
using System.Windows.Forms;

namespace pryDiesenberg_ArregloDeProyecto
{
    public partial class frmBuscarProveedor : Form
    {
        private string rutaProveedores;

        public frmBuscarProveedor()
        {
            InitializeComponent();
            treDirectorios.Nodes.Clear();

            // Construir la ruta correcta
            rutaProveedores = Path.Combine(Application.StartupPath, "Resources", "Proveedores");

            // Validar que la carpeta existe
            if (!Directory.Exists(rutaProveedores))
            {
                MessageBox.Show(
                    $"La carpeta no existe: {rutaProveedores}\n\n" +
                    $"Por favor, crea la carpeta 'Resources/Proveedores' en: {Application.StartupPath}",
                    "Error de ruta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(rutaProveedores);
            treDirectorios.Nodes.Add(crearArbol(dirInfo));
        }

        private void frmBuscarProveedor_Load(object sender, EventArgs e)
        {
            pnlCargarProveedor.Visible = false;
        }

        private TreeNode crearArbol(DirectoryInfo rutaBase)
        {
            TreeNode newNode = new TreeNode(rutaBase.Name)
            {
                Tag = rutaBase.FullName
            };

            try
            {
                foreach (var dir in rutaBase.GetDirectories())
                {
                    newNode.Nodes.Add(crearArbol(dir));
                }

                foreach (var file in rutaBase.GetFiles())
                {
                    TreeNode fileNode = new TreeNode(file.Name)
                    {
                        Tag = file.FullName
                    };
                    newNode.Nodes.Add(fileNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"No tienes permisos para acceder a: {rutaBase.FullName}");
            }

            return newNode;
        }

        private void btnMostrarProveedor_Click(object sender, EventArgs e)
        {
            limpiar();
            string leerLinea;
            string[] separarDatos;
            try
            {
                if (treDirectorios.SelectedNode == null)
                {
                    MessageBox.Show("Selecciona un archivo para cargar la grilla");
                    return;
                }

                string rutaArchivo = treDirectorios.SelectedNode.Tag as string;
                if (string.IsNullOrEmpty(rutaArchivo))
                {
                    MessageBox.Show("Selecciona un archivo válido (.csv).");
                    return;
                }

                if (!File.Exists(rutaArchivo))
                {
                    MessageBox.Show($"El archivo no existe: {rutaArchivo}");
                    return;
                }

                if (Path.GetExtension(rutaArchivo).ToLower() != ".csv")
                {
                    MessageBox.Show("Selecciona un archivo con extensión .csv");
                    return;
                }

                btnNuevoProveedor.Visible = true;
                btnModificarProveedor.Visible = true;
                btnEliminarProveedor.Visible = true;
                btnLimpiar.Visible = true;
                pnlCargarProveedor.Visible = true;
                dgrArchivos.Rows.Clear();
                dgrArchivos.Columns.Clear();

                using (StreamReader sr = new StreamReader(rutaArchivo, true))
                {
                    leerLinea = sr.ReadLine();
                    if (string.IsNullOrEmpty(leerLinea))
                    {
                        MessageBox.Show("El archivo está vacío");
                        return;
                    }

                    separarDatos = leerLinea.Split(';');

                    for (int i = 0; i < separarDatos.Length; i++)
                    {
                        dgrArchivos.Columns.Add(separarDatos[i], separarDatos[i]);
                    }

                    while (!sr.EndOfStream)
                    {
                        leerLinea = sr.ReadLine();
                        if (!string.IsNullOrEmpty(leerLinea))
                        {
                            separarDatos = leerLinea.Split(';');
                            dgrArchivos.Rows.Add(separarDatos);
                        }
                    }
                }

                limpiar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el archivo: " + ex.Message);
            }
        }

        private void btnModificarProveedor_Click(object sender, EventArgs e)
        {
            if (treDirectorios.SelectedNode == null)
            {
                MessageBox.Show("Selecciona el archivo a modificar");
                return;
            }

            // Actualizo valores en la grilla y guardo (la función GuardarCambiosEnCSV usará el Tag del nodo)
            dgrArchivos[0, posicion].Value = txtNumero.Text;
            dgrArchivos[1, posicion].Value = txtEntidad.Text;
            dgrArchivos[2, posicion].Value = txtApertura.Text;
            dgrArchivos[3, posicion].Value = txtNumExpediente.Text;
            dgrArchivos[4, posicion].Value = txtJuzg.Text;
            dgrArchivos[5, posicion].Value = txtJurisd.Text;
            dgrArchivos[6, posicion].Value = txtDireccion.Text;
            dgrArchivos[7, posicion].Value = txtLiquidador.Text;

            limpiar();
            txtNumero.Focus();
            GuardarCambiosEnCSV();
        }

        private void btnEliminarProveedor_Click(object sender, EventArgs e)
        {
            dgrArchivos.Rows.RemoveAt(posicion);
            limpiar();
            txtNumero.Focus();
            GuardarCambiosEnCSV();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            limpiar();
        }

        private void GuardarCambiosEnCSV()
        {
            try
            {
                string archivoSeleccionado = treDirectorios.SelectedNode.Tag as string;
                if (string.IsNullOrEmpty(archivoSeleccionado))
                {
                    MessageBox.Show("No se encontró la ruta del archivo seleccionado.");
                    return;
                }

                using (StreamWriter swGuardar = new StreamWriter(archivoSeleccionado, false))
                {
                    for (int i = 0; i < dgrArchivos.Columns.Count; i++)
                    {
                        swGuardar.Write(dgrArchivos.Columns[i].HeaderText);
                        if (i < dgrArchivos.Columns.Count - 1)
                            swGuardar.Write(";");
                    }
                    swGuardar.WriteLine();

                    foreach (DataGridViewRow row in dgrArchivos.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            for (int i = 0; i < dgrArchivos.Columns.Count; i++)
                            {
                                swGuardar.Write(row.Cells[i].Value);
                                if (i < dgrArchivos.Columns.Count - 1)
                                    swGuardar.Write(";");
                            }
                            swGuardar.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        int posicion;
        private void dgrArchivos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgrArchivos.CurrentRow == null) return;

            posicion = dgrArchivos.CurrentRow.Index;

            txtNumero.Text = dgrArchivos[0, posicion].Value?.ToString() ?? "";
            txtEntidad.Text = dgrArchivos[1, posicion].Value?.ToString() ?? "";
            txtApertura.Text = dgrArchivos[2, posicion].Value?.ToString() ?? "";
            txtNumExpediente.Text = dgrArchivos[3, posicion].Value?.ToString() ?? "";
            txtJuzg.Text = dgrArchivos[4, posicion].Value?.ToString() ?? "";
            txtJurisd.Text = dgrArchivos[5, posicion].Value?.ToString() ?? "";
            txtDireccion.Text = dgrArchivos[6, posicion].Value?.ToString() ?? "";
            txtLiquidador.Text = dgrArchivos[7, posicion].Value?.ToString() ?? "";
        }

        void limpiar()
        {
            txtNumero.Clear();
            txtEntidad.Clear();
            txtApertura.Clear();
            txtNumExpediente.Clear();
            txtJuzg.Clear();
            txtJurisd.Clear();
            txtDireccion.Clear();
            txtLiquidador.Clear();
            dgrArchivos.ClearSelection();
        }

        // ... (resto de validaciones KeyPress sin cambios)
        private void txtNumero_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 47) || (e.KeyChar >= 58 && e.KeyChar <= 255))
            {
                MessageBox.Show("Solo Numeros", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Return)
            {
                txtEntidad.Focus();
                e.Handled = true;
            }
        }

        private void txtEntidad_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 64) || (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 255))
            {
                MessageBox.Show("Solo Letras", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Return)
            {
                txtApertura.Focus();
                e.Handled = true;
            }
        }

        private void txtApertura_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 46) || (e.KeyChar >= 58 && e.KeyChar <= 255))
            {
                MessageBox.Show("Solo Numeros y /", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Return)
            {
                txtNumExpediente.Focus();
                e.Handled = true;
            }
        }

        private void txtNumExpediente_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 46) || (e.KeyChar >= 58 && e.KeyChar <= 255))
            {
                MessageBox.Show("Solo Numeros y /", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Return)
            {
                txtJuzg.Focus();
                e.Handled = true;
            }
        }

        private void txtJuzg_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 47) || (e.KeyChar >= 58 && e.KeyChar <= 64) ||
                (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 247) ||
                (e.KeyChar >= 249 && e.KeyChar <= 255))
            {
                MessageBox.Show("Solo Letras y Numeros", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Return)
            {
                txtJurisd.Focus();
                e.Handled = true;
            }
        }

        private void txtJurisd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 33 && e.KeyChar <= 64) || (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 255))
            {
                MessageBox.Show("Solo Letras", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Return)
            {
                txtDireccion.Focus();
                e.Handled = true;
            }
        }

        private void txtDireccion_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 33 && e.KeyChar <= 43) || (e.KeyChar >= 58 && e.KeyChar <= 64) ||
                (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 255))
            {
                MessageBox.Show("Solo Letras", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Return)
            {
                txtLiquidador.Focus();
                e.Handled = true;
            }
        }

        private void txtLiquidador_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 64) || (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 255))
            {
                MessageBox.Show("Solo Letras", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
            }
        }
    }
}


