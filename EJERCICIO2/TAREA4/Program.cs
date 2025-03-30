/*using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;


namespace Tarea1{

    class Program{

        public class Paciente {
            public int Id {get; set;}
            public int LlegadaHospital {get; set;}
            public int TiempoConsulta {get; set;}
            public int Estado {get; set;}

            public int Prioridad {get; set;}

            public bool requiereDiagnostico {get; set;}

            public Paciente (int Id, int LlegadaHospital, int TiempoConsulta, bool requierediagnostico, int prioridad)
            {
                this.Id = Id;
                this.LlegadaHospital = LlegadaHospital;
                this.TiempoConsulta = TiempoConsulta;
                this.requiereDiagnostico = requierediagnostico;
                this.Prioridad = prioridad;
            }

            public string ObtenerEstadoEnTexto() {
                return Estado switch {
                    1 => "EsperaConsulta",
                    2 => "Consulta",
                    3 => "EsperaDiagnóstico",
                    4 => "Finalizado",
                    _ => "Desconocido"
                };
            }


        }

        static SemaphoreSlim SalaEspera = new SemaphoreSlim(4);
        static SemaphoreSlim maquinas = new SemaphoreSlim(2);
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static PriorityQueue<Paciente, int> queue = new PriorityQueue<Paciente, int>();
        static bool[] Medicos = new bool[4]{ true, true, true, true };
        static Random rnd = new Random();
        static readonly object locker = new object();
        static int numpaciente = 0;
         
        static void Main(string[] args) {

            Stopwatch stopwatch= new Stopwatch();
            
            stopwatch.Start();
            
            for (int i = 0; i < 20; i++) {
                numpaciente = i+1;
                Paciente elpaciente;
                lock (locker) {
                elpaciente = new Paciente(rnd.Next(0, 100), 0, rnd.Next(5000, 15000),true,rnd.Next(1,4));}
                Thread Medico = new Thread(Comportamiento);
                Medico.Start(elpaciente);
                Thread.Sleep(2000);                
            }
            stopwatch.Stop();
       
        }

        static void Comportamiento(object objeto) {

            Stopwatch sw1a2 = new Stopwatch();
            Stopwatch sw2a3 = new Stopwatch();
            Stopwatch stopwatch = new Stopwatch();

            int num = numpaciente;

            Paciente paciente = (Paciente)objeto;

            int numrnd = rnd.Next(0, 2);
            if(numrnd == 0) {
                paciente.requiereDiagnostico = true;
            } else {
                paciente.requiereDiagnostico = false;
            }

            string nombrePrioridad ="";

            if(paciente.Prioridad == 1) {
                    nombrePrioridad = "Emergencias";
            }else if (paciente.Prioridad == 2) {
                nombrePrioridad = "Urgencias";
            }else if (paciente.Prioridad == 3) {
                nombrePrioridad = "Consultas Generales";
            }
             
            paciente.Estado = 1;
            sw1a2.Start();
            Console.WriteLine("Paciente {0}. Llegado el {1}. Prioridad: {3} Estado: {2}.",paciente.Id, num,paciente.ObtenerEstadoEnTexto(),nombrePrioridad);   
             semaphore.Wait();
            queue.Enqueue(paciente, paciente.Prioridad);
            semaphore.Release();

            while (true) {
                semaphore.Wait();
                if (queue.Peek() == paciente) {
                    queue.Dequeue();
                    semaphore.Release();
                    break;
                }
                semaphore.Release();
                Thread.Sleep(10);
            }
            SalaEspera.Wait();
            
            TimeSpan tiempoTardado = sw1a2.Elapsed;
            string tiempo = string.Format("{0}",tiempoTardado.Seconds);

            sw2a3.Start();
            paciente.Estado = 2;

            int medicoAsignado = 1;
            lock (locker) {
               List<int> medicosDisponibles = Medicos
               .Select((disponible, index) => new {disponible,index})
               .Where(m => m.disponible)
               .Select(m=>m.index)
               .ToList();
             
               if (medicosDisponibles.Count > 0) {
                   medicoAsignado = medicosDisponibles[rnd.Next(medicosDisponibles.Count)];
                   Medicos[medicoAsignado] = false;
               }
            }

            Console.WriteLine("Paciente {0}. Llegado el {1}. Estado: {2} con el médico {4}. Duración Espera: {3} segundos.",
            paciente.Id, num,paciente.ObtenerEstadoEnTexto(),tiempo,medicoAsignado);   
            

            Thread.Sleep(paciente.TiempoConsulta);

            tiempoTardado = sw2a3.Elapsed;
            tiempo = string.Format("{0}",tiempoTardado.Seconds);
            if(paciente.requiereDiagnostico) {
                paciente.Estado = 3;
                Console.WriteLine("Paciente {0}. Llegado el {1}. Estado: {2}. Duración Consulta: {3} segundos.",
            paciente.Id, num,paciente.ObtenerEstadoEnTexto(),tiempo);
                maquinas.Wait();
                Thread.Sleep(15000);
                maquinas.Release();
            }
            tiempoTardado = sw2a3.Elapsed;
            tiempo = string.Format("{0}",tiempoTardado.Seconds);
            paciente.Estado = 4;
            Console.WriteLine("Paciente {0}. Llegado el {1}. Estado: {2}. Duración Consulta: {3} segundos.",
            paciente.Id, num,paciente.ObtenerEstadoEnTexto(),tiempo);   

            

            lock (locker) {
               Medicos[medicoAsignado] = true;
            }
            SalaEspera.Release();
            

        }

public async Task OrdenarYPriorizar(Paciente paciente)
{
    await semaphore.WaitAsync();
    try
    {
        queue.Enqueue(paciente, paciente.Prioridad);

        while (queue.Peek() != paciente)
        {
            await Task.Delay(10); // Yield control instead of blocking
        }

        queue.Dequeue(); // Remove patient being attended
    }
    finally
    {
        semaphore.Release();
    }
}

    }
        
}

*/