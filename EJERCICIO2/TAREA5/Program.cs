using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;


namespace Tarea1{

    class Program{

        public class Paciente {
            public int Id {get; set;}
            public int LlegadaHospital {get; set;}
            public int TiempoConsulta {get; set;}
            public int Estado {get; set;}

            public bool requiereDiagnostico {get; set;}

            public int Prioridad {get; set;}

            public Paciente (int Id, int LlegadaHospital, int TiempoConsulta, bool requierediagnostico, int Prioridad)
            {
                this.Id = Id;
                this.LlegadaHospital = LlegadaHospital;
                this.TiempoConsulta = TiempoConsulta;
                this.requiereDiagnostico = requierediagnostico;
                this.Prioridad = Prioridad;
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

        static bool[] Medicos = new bool[4]{ true, true, true,true};

        static Random rnd = new Random();
        static readonly object locker = new object();
    
        static int numpaciente = 0;

        static int globalEmergencias = 0;

        static int globalUrgencias = 0;

        static int globalConsultas = 0;

        static List<Paciente> EmLista = new List<Paciente>();
        static List<Paciente> UrLista = new List<Paciente>();
        static List<Paciente> CoLista = new List<Paciente>();
         
        static int sinecesita = 0;
        static int nonecesita = 0;
        static void Main(string[] args) {

            Stopwatch stopwatch= new Stopwatch();
            
            stopwatch.Start();
            
            for (int i = 0; i < 20; i++) {
                int numPrio = rnd.Next(1,4);
                numpaciente = i+1;
                Paciente elpaciente;
                lock (locker) {
                elpaciente = new Paciente(rnd.Next(0, 100), 0, rnd.Next(5000, 15000),true,numPrio);}
                Thread Medico = new Thread(Comportamiento);
                Medico.Start(elpaciente);
                Thread.Sleep(2000);                
            }

            if (numpaciente == 20) {
                Thread.Sleep(15000);
            stopwatch.Stop();
            Console.WriteLine("\nFin del Dia");
            Console.WriteLine("\nPacientes atendidos: ");
            Console.WriteLine("\nEmergencias:{0}",globalEmergencias);
            Console.WriteLine("\nUrgencias:{0}",globalUrgencias);
            Console.WriteLine("\nConsultas generales:{0}",globalConsultas);
            Console.WriteLine("\n\nTiempo promedio de espera:");
            Console.WriteLine("\nEmergencias: {0} segundos", EmLista.Average(p => p.TiempoConsulta)/100);
            Console.WriteLine("\nUrgencias: {0} segundos", UrLista.Average(p => p.TiempoConsulta)/100);
            Console.WriteLine("\nConsultas generales: {0} segundos", CoLista.Average(p => p.TiempoConsulta)/100);
            }


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
            lock (locker) {
                if(paciente.Prioridad == 1){
                    globalEmergencias++;
                    EmLista.Add(paciente);
                }
                else if(paciente.Prioridad ==2) {
                    globalUrgencias++;
                    UrLista.Add(paciente);
                }
                else if (paciente.Prioridad==3) {
                    globalConsultas++;
                    CoLista.Add(paciente);
                }
                if(paciente.requiereDiagnostico) {
                    sinecesita++;
                }else {
                    nonecesita++;
                }

            }

            
            
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
       
    }
}
