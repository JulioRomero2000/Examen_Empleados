using SQLite;
using Examen_Empleados.Models;

namespace Examen_Empleados.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private async Task<SQLiteAsyncConnection> InitAsync()
        {
            if (_database != null)
                return _database;

            await _initLock.WaitAsync();
            try
            {
                if (_database == null)
                {
                    string dbPath = Path.Combine(
                        FileSystem.AppDataDirectory,
                        "empleados.db3");
                    var database = new SQLiteAsyncConnection(dbPath);
                    await database.CreateTableAsync<Empleado>();
                    _database = database;
                }
                return _database;
            }
            finally
            {
                _initLock.Release();
            }
        }

        // Obtener todos los empleados
        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            var database = await InitAsync();
            return await database
                .Table<Empleado>()
                .ToListAsync();
        }
        // Registrar o actualizar empleado
        public async Task<int> SaveEmpleadoAsync(Empleado empleado)
        {
            var database = await InitAsync();
            if (empleado.Id == 0)
            {
                return await database.InsertAsync(empleado);
            }
            else
            {
                return await database.UpdateAsync(empleado);
            }
        }
        // Eliminar empleado
        public async Task<int> DeleteEmpleadoAsync(Empleado empleado)
        {
            var database = await InitAsync();

            return await database.DeleteAsync(empleado);
        }
    }
}