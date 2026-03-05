using System;

namespace TestScenarios
{
    public class ShoppingCart
    {
        // 1. Caso Directo (Vulnerable)
        public void SearchProduct()
        {
            Console.WriteLine("Buscar producto:");
            string searchTerm = Console.ReadLine(); // Source
            
            // Simulación de construcción de query insegura
            string query = "SELECT * FROM Products WHERE Name = '" + searchTerm + "'";
            
            ExecuteSql(query); // Sink -> DEBE DETECTARSE
        }

        // 2. Caso Seguro (Sin flujo de datos externos)
        public void ListAllProducts()
        {
            string category = "Electronics"; // Safe source
            string query = "SELECT * FROM Products WHERE Category = '" + category + "'";
            
            ExecuteSql(query); // Sink -> SEGURO (No debería detectarse)
        }

        // 3. Flujo Indirecto (Vulnerable)
        public void ProcessOrder()
        {
            string userId = Console.ReadLine(); // Source
            string customerRef = userId;        // Taint propagation
            string finalRef = customerRef;      // Taint propagation
            
            ExecuteSql("UPDATE Orders SET Ref = " + finalRef); // Sink -> DEBE DETECTARSE
        }

        // 4. Falso Positivo / Limpieza (Depende de la implementación del análisis)
        // En nuestro motor actual, si reasignamos una variable con algo seguro, se limpia.
        public void ResetSearch()
        {
            string input = Console.ReadLine(); // Tainted
            input = "Default";                 // Cleaned
            
            ExecuteSql(input); // Sink -> SEGURO
        }

        // Método Sink simulado
        private void ExecuteSql(string sql) 
        { 
            Console.WriteLine("Ejecutando DB: " + sql); 
        }
    }
}
