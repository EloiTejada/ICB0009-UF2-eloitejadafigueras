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

            Paciente paciente = (Paciente)objeto;
            
            paciente.Estado = 1;
            Console.WriteLine("El paciente {0} ha llegado el numero {1} a la sala de consultas.",paciente.Id, numpaciente);
            paciente.LlegadaHospital(int)stopwatch.Elapsed.TotalMilliseconds;
            SalaEspera.Wait();
            
            
            paciente.Estado = 2;
            
            int medicoAsignado = 1;
            lock (locker) {
               List<int> medicosDisponibles = Medicos.Select((disponible, index) => new {disponible,index}).Where(m => m.disponible).Select(m=>m.index).ToList();
             
               if (medicosDisponibles.Count > 0) {
                   medicoAsignado = medicosDisponibles[rnd.Next(medicosDisponibles.Count)];
                   Medicos[medicoAsignado] = false;
               }
            }

            Thread.Sleep(10000);
            paciente.Estado = 3;
            Console.WriteLine("El médico {0} ha acabado de atender al paciente {1}",medicoAsignado+1,paciente.Id);

            lock (locker) {
               Medicos[medicoAsignado] = true;
            }
            SalaEspera.Release();
            

        }
    }
}