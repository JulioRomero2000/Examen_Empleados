using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Examen_Empleados.Models;
using Examen_Empleados.Services;

namespace Examen_Empleados
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private readonly ObservableCollection<Empleado> _empleadosList = new();
        private Empleado? _empleadoSeleccionado;
        public MainPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            listViewEmpleados.ItemsSource = _empleadosList;
            dpFechaIngreso.Date = DateTime.Today;
        }
        // Cargar empleados al abrir la pantalla
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }
        // Obtener empleados desde SQLite
        private async Task LoadDataAsync()
        {
            try
            {
                var empleados = await _dbService.GetEmpleadosAsync();
                _empleadosList.Clear();
                foreach (var empleado in empleados)
                {
                    _empleadosList.Add(empleado);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Error",
                    $"No se pudieron cargar los empleados: {ex.Message}",
                    "OK");
            }
        }
        // Registrar o actualizar empleado
        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            try
            {
                string nombres = txtNombres.Text?.Trim() ?? "";
                string apellidos = txtApellidos.Text?.Trim() ?? "";
                string puesto = txtPuesto.Text?.Trim() ?? "";
                string salarioTexto = txtSalario.Text?.Trim() ?? "";
                string correo = txtCorreo.Text?.Trim() ?? "";
                // Validar nombres
                if (string.IsNullOrWhiteSpace(nombres))
                {
                    await MostrarCampoObligatorio(
                        "Debe ingresar los nombres del empleado.",
                        txtNombres);
                    return;
                }
                // Validar apellidos
                if (string.IsNullOrWhiteSpace(apellidos))
                {
                    await MostrarCampoObligatorio(
                        "Debe ingresar los apellidos del empleado.",
                        txtApellidos);
                    return;
                }
                // Validar puesto
                if (string.IsNullOrWhiteSpace(puesto))
                {
                    await MostrarCampoObligatorio(
                        "Debe ingresar el puesto del empleado.",
                        txtPuesto);
                    return;
                }
                // Validar salario vacío
                if (string.IsNullOrWhiteSpace(salarioTexto))
                {
                    await MostrarCampoObligatorio(
                        "Debe ingresar el salario del empleado.",
                        txtSalario);
                    return;
                }
                // Validar salario numérico
                if (!decimal.TryParse(
                    salarioTexto,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out decimal salario) || salario <= 0)
                {
                    await DisplayAlertAsync(
                        "Salario inválido",
                        "El salario debe ser un número mayor que cero.",
                        "OK");
                    txtSalario.Focus();
                    return;
                }
                // Validar correo vacío
                if (string.IsNullOrWhiteSpace(correo))
                {
                    await MostrarCampoObligatorio(
                        "Debe ingresar el correo electrónico.",
                        txtCorreo);
                    return;
                }
                // Validar formato de correo
                if (!Regex.IsMatch(
                    correo,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    await DisplayAlertAsync(
                        "Correo inválido",
                        "Ingrese un correo electrónico válido.",
                        "OK");
                    txtCorreo.Focus();
                    return;
                }
                // Validar fecha de ingreso
                DateTime fechaIngreso =
                    dpFechaIngreso.Date ?? DateTime.Today;
                if (fechaIngreso.Date > DateTime.Today)
                {
                    await DisplayAlertAsync(
                        "Fecha inválida",
                        "La fecha de ingreso no puede ser futura.",
                        "OK");
                    return;
                }
                // Registrar nuevo empleado
                if (_empleadoSeleccionado == null)
                {
                    var nuevoEmpleado = new Empleado
                    {
                        Nombres = nombres,
                        Apellidos = apellidos,
                        Puesto = puesto,
                        Salario = salario,
                        Correo = correo,
                        FechaIngreso = fechaIngreso
                    };
                    await _dbService.SaveEmpleadoAsync(nuevoEmpleado);
                    await DisplayAlertAsync(
                        "Registro exitoso",
                        "Empleado registrado correctamente.",
                        "OK");
                }
                else
                {
                    // Actualizar empleado existente
                    _empleadoSeleccionado.Nombres = nombres;
                    _empleadoSeleccionado.Apellidos = apellidos;
                    _empleadoSeleccionado.Puesto = puesto;
                    _empleadoSeleccionado.Salario = salario;
                    _empleadoSeleccionado.Correo = correo;
                    _empleadoSeleccionado.FechaIngreso = fechaIngreso;
                    await _dbService.SaveEmpleadoAsync(
                        _empleadoSeleccionado);
                    await DisplayAlertAsync(
                        "Actualización exitosa",
                        "Empleado actualizado correctamente.",
                        "OK");
                }
                // Limpiar formulario
                LimpiarFormulario();
                // Actualizar listado
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Error",
                    $"No se pudo guardar el empleado: {ex.Message}",
                    "OK");
            }
        }
        // Mostrar qué campo falta
        private async Task MostrarCampoObligatorio(
            string mensaje, Entry campo)
        {
            await DisplayAlertAsync(
                "Campo obligatorio",
                mensaje,
                "OK");
            campo.Focus();
        }
        // Seleccionar empleado desde ListView
        private void OnEmpleadoSelected(
            object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is not Empleado empleado)
            {
                _empleadoSeleccionado = null;
                return;
            }
            _empleadoSeleccionado = empleado;
            txtNombres.Text = empleado.Nombres;
            txtApellidos.Text = empleado.Apellidos;
            txtPuesto.Text = empleado.Puesto;
            txtSalario.Text = empleado.Salario.ToString(CultureInfo.CurrentCulture);
            txtCorreo.Text = empleado.Correo;
            dpFechaIngreso.Date = empleado.FechaIngreso;
        }
        // Eliminar empleado
        private async void OnEliminarClicked(object sender, EventArgs e)
        {
            if (_empleadoSeleccionado == null)
            {
                await DisplayAlertAsync(
                    "Aviso",
                    "Seleccione un empleado para eliminar.",
                    "OK");
                return;
            }
            bool confirmar = await DisplayAlertAsync(
                "Confirmar eliminación",
                "¿Está seguro de que desea eliminar este empleado?",
                "Sí",
                "No");
            if (!confirmar)
                return;
            try
            {
                await _dbService.DeleteEmpleadoAsync(
                    _empleadoSeleccionado);

                await DisplayAlertAsync(
                    "Eliminación exitosa",
                    "Empleado eliminado correctamente.",
                    "OK");

                LimpiarFormulario();

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Error",
                    $"No se pudo eliminar el empleado: {ex.Message}",
                    "OK");
            }
        }
        // Botón limpiar
        private void OnLimpiarClicked(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }
        // Limpiar formulario
        private void LimpiarFormulario()
        {
            txtNombres.Text = string.Empty;
            txtApellidos.Text = string.Empty;
            txtPuesto.Text = string.Empty;
            txtSalario.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            dpFechaIngreso.Date = DateTime.Today;
            _empleadoSeleccionado = null;
            listViewEmpleados.SelectedItem = null;
        }
    }
}