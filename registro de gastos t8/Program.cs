using System;
using System.Collections.Generic;
using System.IO;

namespace RegistroGastos
{
    // 1. Clase Gasto con sus propiedades y override de ToString()
    public class Gasto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string Categoria { get; set; }

        public override string ToString()
        {
            return $"{Id} {Descripcion} Q {Monto:N2} {Categoria}";
        }
    }

    class Program
    {
        // Nombre del archivo CSV para la persistencia
        private static string archivoCsv = "gastos.csv";

        public static void Main(string[] args)
        {
            List<Gasto> listaGastos = new List<Gasto>();
            int proximoId = 1;

            // Cargar datos al iniciar el programa y recalcular el Id
            CargarGastos(listaGastos, ref proximoId);

            int opcion;
            do
            {
                opcion = LeerOpcionMenu();

                switch (opcion)
                {
                    case 1:
                        AgregarGasto(listaGastos, ref proximoId);
                        break;
                    case 2:
                        ListarGastos(listaGastos);
                        break;
                    case 3:
                        BuscarPorCategoria(listaGastos);
                        break;
                    case 4:
                        GuardarGastos(listaGastos);
                        Console.WriteLine("Saliendo...");
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
                Console.WriteLine();
            } while (opcion != 4);
        }

        // 2. Método para mostrar el menú y leer la opción elegida
        private static int LeerOpcionMenu()
        {
            Console.WriteLine("=== REGISTRO DE GASTOS ===");
            Console.WriteLine("1. Agregar gasto");
            Console.WriteLine("2. Listar gastos");
            Console.WriteLine("3. Buscar por categoría");
            Console.WriteLine("4. Salir");
            Console.Write("Elige una opción: ");

            string entrada = Console.ReadLine();
            if (int.TryParse(entrada, out int opcion))
            {
                return opcion;
            }
            return 0; // Retorna 0 si ingresan texto inválido, lo que activará "Opción no válida."
        }

        // 3. Método para agregar un gasto validando con TryParse y ref para el Id
        private static void AgregarGasto(List<Gasto> gastos, ref int proximoId)
        {
            Console.Write("Descripción: ");
            string descripcion = Console.ReadLine();

            Console.Write("Monto: ");
            string inputMonto = Console.ReadLine();

            Console.Write("Categoría: ");
            string categoria = Console.ReadLine();

            // Validar que el monto sea un número válido mayor a 0 y que los textos no estén vacíos
            if (decimal.TryParse(inputMonto, out decimal monto) && monto > 0 &&
                !string.IsNullOrWhiteSpace(descripcion) && !string.IsNullOrWhiteSpace(categoria))
            {
                Gasto nuevoGasto = new Gasto
                {
                    Id = proximoId,
                    Descripcion = descripcion.Trim(),
                    Monto = monto,
                    Categoria = categoria.Trim()
                };

                gastos.Add(nuevoGasto);
                proximoId++;
                Console.WriteLine("Gasto agregado.");
            }
            else
            {
                Console.WriteLine("Datos inválidos.");
            }
        }

        // 4. Método para listar todos los gastos y calcular el total general
        private static void ListarGastos(List<Gasto> gastos)
        {
            if (gastos.Count == 0)
            {
                Console.WriteLine("No hay gastos registrados.");
                return;
            }

            decimal totalGeneral = 0;
            string resultado = "";

            for (int i = 0; i < gastos.Count; i++)
            {
                resultado += gastos[i].ToString();
                totalGeneral += gastos[i].Monto;

                if (i < gastos.Count - 1)
                {
                    resultado += " | ";
                }
            }

            Console.WriteLine(resultado);
            Console.WriteLine($"TOTAL GASTADO: Q {totalGeneral:N2}");
        }

        // 5. Método para buscar por texto en la categoría
        private static void BuscarPorCategoria(List<Gasto> gastos)
        {
            Console.Write("Categoría a buscar: ");
            string busqueda = Console.ReadLine()?.Trim().ToLower();

            bool encontrado = false;
            foreach (var g in gastos)
            {
                if (g.Categoria.ToLower().Contains(busqueda))
                {
                    Console.WriteLine($"- {g.ToString()}");
                    encontrado = true;
                }
            }

            if (!encontrado)
            {
                Console.WriteLine("No se encontraron gastos con esa categoría.");
            }
        }

        // 6. Métodos de Persistencia en CSV (Guardar)
        private static void GuardarGastos(List<Gasto> gastos)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(archivoCsv))
                {
                    foreach (var g in gastos)
                    {
                        // Guardamos separados por comas: Id,Descripcion,Monto,Categoria
                        sw.WriteLine($"{g.Id},{g.Descripcion},{g.Monto},{g.Categoria}");
                    }
                }
                Console.WriteLine($"Gastos guardados en {archivoCsv} ({gastos.Count} registros).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el archivo: {ex.Message}");
            }
        }

        // 6. Métodos de Persistencia en CSV (Cargar y recalcular el próximo Id)
        private static void CargarGastos(List<Gasto> gastos, ref int proximoId)
        {
            if (File.Exists(archivoCsv))
            {
                try
                {
                    string[] lineas = File.ReadAllLines(archivoCsv);
                    foreach (var linea in lineas)
                    {
                        string[] partes = linea.Split(',');
                        if (partes.Length == 4)
                        {
                            Gasto g = new Gasto
                            {
                                Id = int.Parse(partes[0]),
                                Descripcion = partes[1],
                                Monto = decimal.Parse(partes[2]),
                                Categoria = partes[3]
                            };
                            gastos.Add(g);
                        }
                    }

                    // Recalcular el Id basándose en el ID más alto existente para que no se repita
                    if (gastos.Count > 0)
                    {
                        int maxId = 0;
                        foreach (var g in gastos)
                        {
                            if (g.Id > maxId)
                            {
                                maxId = g.Id;
                            }
                        }
                        proximoId = maxId + 1;
                    }

                    Console.WriteLine($"Cargados {gastos.Count} gastos desde {archivoCsv}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cargar el archivo: {ex.Message}");
                }
            }
        }
    }
}