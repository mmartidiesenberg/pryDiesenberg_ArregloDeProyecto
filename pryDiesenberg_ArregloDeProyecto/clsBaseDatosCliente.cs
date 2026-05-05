using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace pryDiesenberg_ArregloDeProyecto
{
    class clsBaseDatosCliente
    {
        //Objetos
        OleDbConnection conexionBD;
        OleDbCommand comandoBD;
        OleDbDataReader lectorBD;
        OleDbDataAdapter adaptadorBD;
        DataSet objDataSet = new DataSet();

        public string estadoConexion = "";
        public string datosTabla;

        private string ObtenerRutaBD()
        {
            return Path.Combine(Application.StartupPath, "EL_CLUB.accdb");
        }

        public void ConectarBD()
        {
            try
            {
                string ruta = ObtenerRutaBD();
                conexionBD = new OleDbConnection(
                    $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ruta};"
                );
                conexionBD.Open();
                estadoConexion = "Conectado";
            }
            catch (Exception ex)
            {
                estadoConexion = "Error de conexión";
                MessageBox.Show("ERROR al conectar: " + ex.Message);
            }
        }

        private void CerrarConexion()
        {
            lectorBD?.Close();
            if (conexionBD != null && conexionBD.State == ConnectionState.Open)
            {
                conexionBD.Close();
                conexionBD.Dispose();
            }
        }

        public void TraerDatos(DataGridView grilla)
        {
            try
            {
                ConectarBD();
                comandoBD = new OleDbCommand
                {
                    Connection = conexionBD,
                    CommandType = CommandType.TableDirect,
                    CommandText = "CLIENTES"
                };

                lectorBD = comandoBD.ExecuteReader();

                grilla.Columns.Add("CODIGO_SOCIO", "CODIGO_SOCIO");
                grilla.Columns.Add("Nombre", "Nombre");
                grilla.Columns.Add("Apellido", "Apellido");
                grilla.Columns.Add("Edad", "Edad");
                grilla.Columns.Add("Sexo", "Sexo");
                grilla.Columns.Add("Ingreso", "Ingreso");
                grilla.Columns.Add("Puntaje", "Puntaje");
                grilla.Columns.Add("Actividad", "Actividad");

                while (lectorBD.Read())
                {
                    string actividad = (bool)lectorBD["Actividad"] ? "Activo" : "Inactivo";
                    grilla.Rows.Add(
                        lectorBD[0], lectorBD[1], lectorBD[2],
                        lectorBD[4], lectorBD[5], lectorBD[6],
                        lectorBD[7], actividad);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al traer datos: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        // BUSCAR — ahora abre y cierra conexión correctamente
        public void BuscarPorID(int codigo)
        {
            try
            {
                ConectarBD();

                comandoBD = new OleDbCommand
                {
                    Connection = conexionBD,
                    CommandType = CommandType.TableDirect,
                    CommandText = "CLIENTES"
                };

                lectorBD = comandoBD.ExecuteReader();

                bool encontrado = false;

                while (lectorBD.Read())
                {
                    if (int.Parse(lectorBD[0].ToString()) == codigo)
                    {
                        MessageBox.Show(
                            $"Cliente encontrado:\nCódigo: {lectorBD[0]}\n" +
                            $"Nombre: {lectorBD[1]} {lectorBD[2]}",
                            "Cliente Existente",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    MessageBox.Show($"No existe un cliente con código {codigo}.",
                        "No encontrado", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public void actividadCliente(int codigo)
        {
            try
            {
                ConectarBD();
                objDataSet = new DataSet();

                comandoBD = new OleDbCommand
                {
                    Connection = conexionBD,
                    CommandType = CommandType.TableDirect,
                    CommandText = "CLIENTES"
                };

                adaptadorBD = new OleDbDataAdapter(comandoBD);
                adaptadorBD.Fill(objDataSet, "CLIENTES");

                DataTable dt = objDataSet.Tables["CLIENTES"];

                foreach (DataRow dr in dt.Rows)
                {
                    if ((int)dr["CODIGO_SOCIO"] == codigo)
                    {
                        dr.BeginEdit();
                        dr["ACTIVIDAD"] = !(bool)dr["ACTIVIDAD"]; // toggle
                        dr.EndEdit();
                        break;
                    }
                }

                OleDbCommandBuilder cb = new OleDbCommandBuilder(adaptadorBD);
                adaptadorBD.Update(objDataSet, "CLIENTES");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al modificar actividad: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}
