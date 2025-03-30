using System;
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

            public Paciente (int Id, int LlegadaHospital, int TiempoConsulta)
            {
                this.Id = Id;
                this.LlegadaHospital = LlegadaHospital;
                this.TiempoConsulta = TiempoConsulta;
            }

            public string ObtenerEstadoEnTexto() {
                return Estado switch {
                    1 => "Espera",
                    2 => "Consulta",
                    3 => "Finalizado",
                    _ => "Desconocido"
                };
            }


        }

        static SemaphoreSlim SalaEspera = new SemaphoreSlim(20);

        static bool[] Medicos = new bool[4]{ true, true, true,true};

        static Random rnd = new Random();
        static readonly object locker = new object();
    
        static int numpaciente = 0;
         
        static void Main(string[] args) {

            Stopwatch stopwatch= new Stopwatch();
            
            stopwatch.Start();
            
            for (int i = 0; i < 4; i++) {
                numpaciente = i+1;
                Paciente elpaciente;
                lock (locker) {
                elpaciente = new Paciente(rnd.Next(0, 100), 0, rnd.Next(5000, 15000));}
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
            
            paciente.Estado = 1;
            sw1a2.Start();
            Console.WriteLine("Paciente {0}. Llegado el {1}. Estado: {2}.",paciente.Id, num,paciente.ObtenerEstadoEnTexto());   
            
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
            
            paciente.Estado = 3;
            Console.WriteLine("Paciente {0}. Llegado el {1}. Estado: {2}. Duración Consulta: {3} segundos.",
            paciente.Id, num,paciente.ObtenerEstadoEnTexto(),tiempo);   

            lock (locker) {
               Medicos[medicoAsignado] = true;
            }
            SalaEspera.Release();
            

        }
       
    }
}