using System;
using System.IO;
using System.Windows.Forms;

namespace pryDiesenberg_ArregloDeProyecto
{
    public partial class frmBuscarProveedor : Form
    {
        private string rutaProveedores;
        int posicion;

        public frmBuscarProveedor()
        {
            InitializeComponent();

            // Eventos enlazados UNA sola vez
            btnMostrarProveedor.Click += btnMostrarProveedor_Click;
            btnNuevoProveedor.Click += btnNuevoProveedor_Click;
            btnModificarProveedor.Click += btnModificarProveedor_Click;
            btnEliminarProveedor.Click += btnEliminarProveedor_Click;
            btnLimpiar.Click += btnLimpiar_Click;

            dgrArchivos.CellClick += dgrArchivos_CellContentClick;
            dgrArchivos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgrArchivos.MultiSelect = false;

            treDirectorios.Nodes.Clear();

            rutaProveedores = Path.Combine(Application.StartupPath, "Resources", "Proveedores");

            if (!Directory.Exists(rutaProveedores))
            {
                MessageBox.Show(
                    $"La carpeta no existe: {rutaProveedores}\n\n" +
                    $"Crea la carpeta 'Resources/Proveedores' en: {Application.StartupPath}",
                    "Error de ruta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(rutaProveedores);
            treDirectorios.Nodes.Add(crearArbol(dirInfo));
        }

        private void frmBuscarProveedor_Load(object sender, EventArgs e)
        {
            pnlCargarProveedor.Visible = false;
        }

        // ──────────────────────────────────────────────
        // ÁRBOL DE DIRECTORIOS
        // ──────────────────────────────────────────────
        private TreeNode crearArbol(DirectoryInfo rutaBase)
        {
            TreeNode newNode = new TreeNode(rutaBase.Name) { Tag = rutaBase.FullName };

            try
            {
                foreach (var dir in rutaBase.GetDirectories())
                    newNode.Nodes.Add(crearArbol(dir));

                foreach (var file in rutaBase.GetFiles())
                    newNode.Nodes.Add(new TreeNode(file.Name) { Tag = file.FullName });
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"Sin permisos para acceder a: {rutaBase.FullName}");
            }

            return newNode;
        }

        // ──────────────────────────────────────────────
        // BOTÓN MOSTRAR
        // ──────────────────────────────────────────────
        private void btnMostrarProveedor_Click(object sender, EventArgs e)
        {
            if (treDirectorios.SelectedNode == null)
            {
                MessageBox.Show("Seleccioná un archivo para cargar la grilla.");
                return;
            }

            string rutaArchivo = treDirectorios.SelectedNode.Tag as string;

            if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
            {
                MessageBox.Show("Seleccioná un archivo válido.");
                return;
            }

            if (Path.GetExtension(rutaArchivo).ToLower() != ".csv")
            {
                MessageBox.Show("Seleccioná un archivo con extensión .csv");
                return;
            }

            try
            {
                dgrArchivos.Rows.Clear();
                dgrArchivos.Columns.Clear();

                using (StreamReader sr = new StreamReader(rutaArchivo, true))
                {
                    string encabezado = sr.ReadLine();
                    if (string.IsNullOrEmpty(encabezado))
                    {
                        MessageBox.Show("El archivo está vacío.");
                        return;
                    }

                    foreach (string col in encabezado.Split(';'))
                        dgrArchivos.Columns.Add(col, col);

                    while (!sr.EndOfStream)
                    {
                        string linea = sr.ReadLine();
                        if (!string.IsNullOrEmpty(linea))
                            dgrArchivos.Rows.Add(linea.Split(';'));
                    }
                }

                pnlCargarProveedor.Visible = true;
                btnNuevoProveedor.Visible = true;
                btnModificarProveedor.Visible = true;
                btnEliminarProveedor.Visible = true;
                btnLimpiar.Visible = true;

                limpiar();

                btnMostrarProveedor.Visible = false;  // ← ÚLTIMA LÍNEA DEL TRY
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el archivo: " + ex.Message);
            }
        }

        // ──────────────────────────────────────────────
        // BOTÓN NUEVO
        // ──────────────────────────────────────────────
        private void btnNuevoProveedor_Click(object sender, EventArgs e)
        {
            if (dgrArchivos.Columns.Count == 0)
            {
                MessageBox.Show("Primero cargá un archivo con 'Mostrar Proveedor'.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNumero.Text))
            {
                MessageBox.Show("Completá al menos el campo N° antes de agregar.");
                txtNumero.Focus();
                return;
            }

            dgrArchivos.Rows.Add(
                txtNumero.Text, txtEntidad.Text, txtApertura.Text,
                txtNumExpediente.Text, txtJuzg.Text, txtJurisd.Text,
                txtDireccion.Text, txtLiquidador.Text);

            limpiar();
            txtNumero.Focus();
            GuardarCambiosEnCSV();
        }

        // ──────────────────────────────────────────────
        // BOTÓN MODIFICAR
        // ──────────────────────────────────────────────
        private void btnModificarProveedor_Click(object sender, EventArgs e)
        {
            if (dgrArchivos.CurrentRow == null || dgrArchivos.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccioná una fila de la grilla para modificar.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNumero.Text))
            {
                MessageBox.Show("El campo N° no puede estar vacío.");
                txtNumero.Focus();
                return;
            }

            // Actualiza la fila en la grilla con lo que hay en los campos
            dgrArchivos[0, posicion].Value = txtNumero.Text;
            dgrArchivos[1, posicion].Value = txtEntidad.Text;
            dgrArchivos[2, posicion].Value = txtApertura.Text;
            dgrArchivos[3, posicion].Value = txtNumExpediente.Text;
            dgrArchivos[4, posicion].Value = txtJuzg.Text;
            dgrArchivos[5, posicion].Value = txtJurisd.Text;
            dgrArchivos[6, posicion].Value = txtDireccion.Text;
            dgrArchivos[7, posicion].Value = txtLiquidador.Text;

            GuardarCambiosEnCSV();

            MessageBox.Show("Proveedor modificado correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            limpiar(); // Limpia DESPUÉS de guardar
            txtNumero.Focus();
        }

        // ──────────────────────────────────────────────
        // BOTÓN ELIMINAR
        // ──────────────────────────────────────────────
        private void btnEliminarProveedor_Click(object sender, EventArgs e)
        {
            if (dgrArchivos.CurrentRow == null)
            {
                MessageBox.Show("Seleccioná una fila para eliminar.");
                return;
            }

            // Evita eliminar la fila vacía de nueva entrada
            if (dgrArchivos.CurrentRow.IsNewRow)
            {
                MessageBox.Show("No hay proveedor seleccionado para eliminar.");
                return;
            }

            var confirm = MessageBox.Show(
                "¿Seguro que querés eliminar este proveedor?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                dgrArchivos.Rows.RemoveAt(posicion);
                limpiar();
                txtNumero.Focus();
                GuardarCambiosEnCSV();
            }
        }

        // ──────────────────────────────────────────────
        // BOTÓN LIMPIAR
        // ──────────────────────────────────────────────
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            limpiar();
        }

        // ──────────────────────────────────────────────
        // GUARDAR CSV
        // ──────────────────────────────────────────────
        private void GuardarCambiosEnCSV()
        {
            try
            {
                string archivoSeleccionado = treDirectorios.SelectedNode?.Tag as string;
                if (string.IsNullOrEmpty(archivoSeleccionado))
                {
                    MessageBox.Show("No se encontró la ruta del archivo seleccionado.");
                    return;
                }

                using (StreamWriter sw = new StreamWriter(archivoSeleccionado, false))
                {
                    // Encabezados
                    for (int i = 0; i < dgrArchivos.Columns.Count; i++)
                    {
                        sw.Write(dgrArchivos.Columns[i].HeaderText);
                        if (i < dgrArchivos.Columns.Count - 1) sw.Write(";");
                    }
                    sw.WriteLine();

                    // Filas
                    foreach (DataGridViewRow row in dgrArchivos.Rows)
                    {
                        if (row.IsNewRow) continue;
                        for (int i = 0; i < dgrArchivos.Columns.Count; i++)
                        {
                            sw.Write(row.Cells[i].Value ?? "");
                            if (i < dgrArchivos.Columns.Count - 1) sw.Write(";");
                        }
                        sw.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        // ──────────────────────────────────────────────
        // CLICK EN GRILLA
        // ──────────────────────────────────────────────
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

        // ──────────────────────────────────────────────
        // LIMPIAR CAMPOS
        // ──────────────────────────────────────────────
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

        // ──────────────────────────────────────────────
        // VALIDACIONES KeyPress (sin cambios)
        // ──────────────────────────────────────────────
        private void txtNumero_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 47) || (e.KeyChar >= 58 && e.KeyChar <= 255))
            { MessageBox.Show("Solo Números", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); e.Handled = true; return; }
            if (e.KeyChar == (char)Keys.Return) { txtEntidad.Focus(); e.Handled = true; }
        }

        private void txtEntidad_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 64) || (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 255))
            { MessageBox.Show("Solo Letras", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); e.Handled = true; return; }
            if (e.KeyChar == (char)Keys.Return) { txtApertura.Focus(); e.Handled = true; }
        }

        private void txtApertura_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 46) || (e.KeyChar >= 58 && e.KeyChar <= 255))
            { MessageBox.Show("Solo Números y /", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); e.Handled = true; return; }
            if (e.KeyChar == (char)Keys.Return) { txtNumExpediente.Focus(); e.Handled = true; }
        }

        private void txtNumExpediente_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 46) || (e.KeyChar >= 58 && e.KeyChar <= 255))
            { MessageBox.Show("Solo Números y /", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); e.Handled = true; return; }
            if (e.KeyChar == (char)Keys.Return) { txtJuzg.Focus(); e.Handled = true; }
        }

        private void txtJuzg_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 47) || (e.KeyChar >= 58 && e.KeyChar <= 64) ||
                (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 247) || (e.KeyChar >= 249 && e.KeyChar <= 255))
            { MessageBox.Show("Solo Letras y Números", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); e.Handled = true; return; }
            if (e.KeyChar == (char)Keys.Return) { txtJurisd.Focus(); e.Handled = true; }
        }

        private void txtJurisd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 33 && e.KeyChar <= 64) || (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 255))
            { MessageBox.Show("Solo Letras", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); e.Handled = true; return; }
            if (e.KeyChar == (char)Keys.Return) { txtDireccion.Focus(); e.Handled = true; }
        }

        private void txtDireccion_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 33 && e.KeyChar <= 43) || (e.KeyChar >= 58 && e.KeyChar <= 64) ||
                (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 255))
            { MessageBox.Show("Solo Letras", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); e.Handled = true; return; }
            if (e.KeyChar == (char)Keys.Return) { txtLiquidador.Focus(); e.Handled = true; }
        }

        private void txtLiquidador_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 32 && e.KeyChar <= 64) || (e.KeyChar >= 91 && e.KeyChar <= 96) || (e.KeyChar >= 123 && e.KeyChar <= 255))
            { MessageBox.Show("Solo Letras", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); e.Handled = true; return; }
            if (e.KeyChar == (char)Keys.Return) { e.Handled = true; }
        }       
    }
}


